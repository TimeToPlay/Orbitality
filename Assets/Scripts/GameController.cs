using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using SO;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    private PlanetController.Factory _planetFactory;
    private SettingsSO.GameSettings _gameSettings;
    private PlanetController _playerPlanet;
    private List<PlanetController> _enemies = new List<PlanetController>();
    private List<CelestialObject> _celestialObjects = new List<CelestialObject>();
    [SerializeField] private CelestialObject _sun;
    [Inject]
    void Construct(
        PlanetController.Factory planetFactory,
        SettingsSO.GameSettings gameSettings
        )
    {
        _planetFactory = planetFactory;
        _gameSettings = gameSettings;
    }
    void Start()
    {
        StartNewGame();
        ConfigureInput();
    }

    private void StartNewGame()
    {
        //CreatePlayer();
        CreateEnemies();
    }

    private void CreateEnemies()
    {
        var playerIndex = Random.Range(0, _gameSettings.initialPlanetAmount);
        float previousOrbit = _gameSettings.MinimumOrbitRadius;
        _enemies.Clear();
        _celestialObjects.Clear();
        for (int i = 0; i < _gameSettings.initialPlanetAmount; i++)
        {
            var randomPlanetSetting = new SettingsSO.PlanetSettings();
            randomPlanetSetting.planetScale = Random.Range(_gameSettings.MinPlanetScale, _gameSettings.MaxPlanetScale);
            randomPlanetSetting.orbitRadius = previousOrbit  + randomPlanetSetting.planetScale / 2 + Random.Range(2,7);
            previousOrbit = randomPlanetSetting.orbitRadius + randomPlanetSetting.planetScale / 2;
            randomPlanetSetting.clockwise = Random.value > 0.5f;
            randomPlanetSetting.solarAngularVelocity = Random.Range(_gameSettings.SolarAngularVelocityMin, _gameSettings.SolarAngularVelocityMax);
            randomPlanetSetting.selfRotationVelocity = Random.Range(_gameSettings.SelfRotationVelocityMin, _gameSettings.SelfRotationVelocityMax);
            var planet = _planetFactory.Create(randomPlanetSetting);
            if (playerIndex == i)
            {
                _playerPlanet = planet;
            }
            else
            {
                _enemies.Add(planet);
            }
            _celestialObjects.Add(planet);
            _celestialObjects.Add(_sun);
        }
    }

    public List<CelestialObject> GetAllCelestialObjects()
    {
        return _celestialObjects;
    }
    private void ConfigureInput()
    {
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Space)).Subscribe(_ =>
        {
            _playerPlanet.Shoot();
        }).AddTo(this);
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.LeftShift) ||
                                            Input.GetKeyDown(KeyCode.RightShift)).Subscribe(_ =>
        {
            //todo: switch rockets
        }).AddTo(this);
    }
}