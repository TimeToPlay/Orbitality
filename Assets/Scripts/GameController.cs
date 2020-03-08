using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Transform _rocketPoolRoot;
    private List<SettingsSO.RocketSettings> _rocketSettings;
    private AISystem _aiSystem;
    private RocketAmmoPanel _ammoPanel;
    private GameState currentGameState = GameState.MainMenu;
    private LocalSaveController _localSaveController;
    private GameSaveInfo _gameSaveInfo = new GameSaveInfo();
    private RocketController.Factory _rocketFactory;

    [Inject]
    void Construct(
        PlanetController.Factory planetFactory,
        SettingsSO.GameSettings gameSettings,
        List<SettingsSO.RocketSettings> rocketSettings,
        RocketAmmoPanel ammoPanel,
        LocalSaveController localSaveController,
        RocketController.Factory rocketFactory
        )
    {
        _planetFactory = planetFactory;
        _rocketFactory = rocketFactory;
        _gameSettings = gameSettings;
        _rocketSettings = rocketSettings;
        _ammoPanel = ammoPanel;
        _localSaveController = localSaveController;
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
        _playerPlanet.Dispose();
        foreach (var enemy in _enemies)
        {
            enemy.Dispose();
        }

        foreach (Transform rocketTr in _rocketPoolRoot)
        {
            if (rocketTr.gameObject.activeSelf)
            {
                rocketTr.GetComponent<RocketController>().Dispose();
            }
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
        
        _gameSaveInfo.PlanetStates.Clear();
        
        for (int i = 0; i < _gameSettings.initialPlanetAmount; i++)
        {
            var randomPlanetSetting = new SettingsSO.PlanetSettings();
            randomPlanetSetting.planetScale = Random.Range(_gameSettings.MinPlanetScale, _gameSettings.MaxPlanetScale);
            randomPlanetSetting.orbitRadius = previousOrbit + randomPlanetSetting.planetScale / 2 + Random.Range(2, 7);
            previousOrbit = randomPlanetSetting.orbitRadius + randomPlanetSetting.planetScale / 2;
            randomPlanetSetting.clockwise = Random.value > 0.5f;
            randomPlanetSetting.solarAngularVelocity = Random.Range(_gameSettings.SolarAngularVelocityMin,
                _gameSettings.SolarAngularVelocityMax);
            randomPlanetSetting.selfRotationVelocity = Random.Range(_gameSettings.SelfRotationVelocityMin,
                _gameSettings.SelfRotationVelocityMax);

            var planetState = new PlanetState();
            planetState.settings = randomPlanetSetting;
            planetState.hp = _gameSettings.initialPlanetHP;
            var isPlayer = playerIndex == i;
            planetState.isPlayer = isPlayer;
            planetState.currentAngleToSun = Random.Range(0, Mathf.PI * 2);
            foreach (var rocket in _rocketSettings)
            {
                planetState.rocketAmmo.Add(rocket.rocketType, Random.Range(rocket.minAmmo, rocket.maxAmmo));
            }
            _gameSaveInfo.PlanetStates.Add(planetState);

        }
        ApplyStates();
    }

    private void ApplyStates()
    {
        _enemies.Clear();
        _celestialObjects.Clear();
        foreach (var planetState in _gameSaveInfo.PlanetStates)
        {
            var planet = _planetFactory.Create(planetState);
            if (planetState.isPlayer)
            {
                _playerPlanet = planet;
                _playerPlanet.onDieEvent = GameOver;
                foreach (var key in planetState.rocketAmmo.Keys)
                {
                    _ammoPanel.SetRocketAmmo(key, planetState.rocketAmmo[key]);
                }
            }
            else
            {
                planet.onDieEvent = delegate
                {
                    if (_enemies.All(enemy => enemy.IsDead))
                    {
                        GameWon();
                    }
                };
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

    public void SaveGame()
    {
        _gameSaveInfo.RocketStates.Clear();
       
        foreach (Transform rocketTr in _rocketPoolRoot)
        {
            if (rocketTr.gameObject.activeSelf)
            {
                _gameSaveInfo.RocketStates.Add(rocketTr.GetComponent<RocketController>().GetCurrentState());
            }
        }
        _gameSaveInfo.PrepareForSerialize();
        var json = JsonUtility.ToJson(_gameSaveInfo);
        _localSaveController.SaveProgress(json);
    }

    public void GameOver()
    {
        Time.timeScale =0;
        currentGameState = GameState.Finished;
        _mainMenuController.Show(GameState.Finished);
    }
    
    public void GameWon()
    {
        Time.timeScale =0;
        currentGameState = GameState.YouWin;
        _mainMenuController.Show(GameState.YouWin);
    }

    public bool CheckSaveFileExists()
    {
        return _localSaveController.IsSaveFileExists();
    }

    public void LoadGame()
    {
        if (_playerPlanet != null)
        {
            DisposePools();
        }
        _localSaveController.LoadSaveFile(delegate(GameSaveInfo info)
        {
            _gameSaveInfo = info;
            _gameSaveInfo.PrepareForDeserialize();
            ApplyStates();
            foreach (var rocketState in _gameSaveInfo.RocketStates)
            {
                var rocket = _rocketFactory.Create();
                rocket.Configure(rocketState);
            }
            ResumeGame();
            if (_aiSystem == null)
            {
                _aiSystem = new AISystem(_sun, _playerPlanet, _enemies);
            }

            
            
        }, Debug.LogError);
        
    }
}

public enum GameState{
    MainMenu,
    Running,
    Paused,
    Finished,
    YouWin
}