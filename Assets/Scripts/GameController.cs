using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Models;
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
    [SerializeField] private MainMenuController _mainMenuController;
    private List<SettingsSO.RocketSettings> _rocketSettings;
    private AISystem _aiSystem;
    private RocketAmmoPanel _ammoPanel;
    private GameState currentGameState = GameState.MainMenu;

    [Inject]
    void Construct(
        PlanetController.Factory planetFactory,
        SettingsSO.GameSettings gameSettings,
        List<SettingsSO.RocketSettings> rocketSettings,
        RocketAmmoPanel ammoPanel
        )
    {
        _planetFactory = planetFactory;
        _gameSettings = gameSettings;
        _rocketSettings = rocketSettings;
        _ammoPanel = ammoPanel;
    }
    void Start()
    {
        ConfigureInput();
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (currentGameState == GameState.Running)
        {
            _aiSystem.MakeDecisions();
        }
    }

    public void DisposePools()
    {
        _ammoPanel.SetCurrentRocketType(RocketType.Normal);
        _playerPlanet.DisposeAllRockets();
        _playerPlanet.Dispose();
        foreach (var enemy in _enemies)
        {
            enemy.DisposeAllRockets();
            enemy.Dispose();
        }
    }
    public void StartNewGame()
    {
        //CreatePlayer();
        CreateEnemies();
        ResumeGame();
        _aiSystem = new AISystem(_sun, _playerPlanet, _enemies);
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
            
            var planetState = new PlanetState();
            planetState.settings = randomPlanetSetting;
            planetState.hp = _gameSettings.initialPlanetHP;
            var isPlayer = playerIndex == i;
            planetState.isPlayer = isPlayer;
            foreach (var rocket in _rocketSettings)
            {
                planetState.rocketAmmo.Add(rocket.rocketType, Random.Range(rocket.minAmmo, rocket.maxAmmo));
            }

            var planet = _planetFactory.Create(planetState);
            if (isPlayer)
            {
                _playerPlanet = planet;
                foreach (var key in planetState.rocketAmmo.Keys)
                {
                    _ammoPanel.SetRocketAmmo(key, planetState.rocketAmmo[key]);
                }
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
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Space) && currentGameState == GameState.Running).Subscribe(_ =>
        {
            _playerPlanet.Shoot();
            _ammoPanel.SetRocketAmmo(_playerPlanet.GetCurrentRocketSettings().rocketType, _playerPlanet.GetCurrentAmmo());
            if (_playerPlanet.GetCurrentAmmo() <= 0)
            {
                SwitchToNextNonEmptyAmmo();
            }
            
        }).AddTo(this);
        Observable.EveryUpdate().Where(_ => (Input.GetKeyDown(KeyCode.LeftShift) ||
                                            Input.GetKeyDown(KeyCode.RightShift)) && currentGameState == GameState.Running).Subscribe(_ =>
        {
            SwitchToNextNonEmptyAmmo();
        }).AddTo(this);
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Escape) && currentGameState == GameState.Running).Subscribe(_ =>
            {
                currentGameState = GameState.Paused;
                Time.timeScale = 0;
                _mainMenuController.Show(currentGameState);
            }).AddTo(this);
    }

    private void SwitchToNextNonEmptyAmmo()
    {
        var rocketTypeInt = (int) _playerPlanet.GetCurrentRocketSettings().rocketType;
        for (int i = 0; i < Enum.GetValues(typeof(RocketType)).Length; i++)
        {
            rocketTypeInt++;
            if (rocketTypeInt >= Enum.GetValues(typeof(RocketType)).Length)
            {
                rocketTypeInt = 0;
            }
            _playerPlanet.SetRocketType((RocketType) rocketTypeInt);
            if (_playerPlanet.GetCurrentAmmo() <= 0)
            {
                continue;
            }
            _ammoPanel.SetCurrentRocketType((RocketType) rocketTypeInt);
            break;
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        currentGameState = GameState.Running;
    }
}

public enum GameState{
    MainMenu,
    Running,
    Paused,
    Finished
}