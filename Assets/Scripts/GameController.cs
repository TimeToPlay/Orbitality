using System;
using System.Collections.Generic;
using System.Linq;
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
    private GameModel gameModel = new GameModel();
    private RocketController.Factory _rocketFactory;
    public List<CelestialObject> CelestialObjects => _celestialObjects;

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

    void Update()
    {
        if (currentGameState == GameState.Running)
        {
            _aiSystem.MakeDecisions();
        }
    }

    #region PUBLIC METHODS

    public void StartNewGame()
    {
        CreateRandomizedState();
        ApplyStates();
        ResumeGame();
        _aiSystem = new AISystem(_sun, _enemies);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        currentGameState = GameState.Running;
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

        _localSaveController.LoadSaveFile(delegate(GameModel info)
        {
            gameModel = info;
            ApplyStates();
            foreach (var rocketState in gameModel.GetRocketModels())
            {
                var rocket = _rocketFactory.Create();
                rocket.Configure(rocketState);
            }

            ResumeGame();
            if (_aiSystem == null)
            {
                _aiSystem = new AISystem(_sun, _enemies);
            }
        }, Debug.LogError);
    }

    public void SaveGame()
    {
        gameModel.ClearRocketStates();

        foreach (Transform rocketTr in _rocketPoolRoot)
        {
            if (rocketTr.gameObject.activeSelf)
            {
                gameModel.AddNewRocketModel(rocketTr.GetComponent<RocketController>().GetCurrentState());
            }
        }

        var json = JsonUtility.ToJson(gameModel);
        _localSaveController.SaveProgress(json);
    }

    #endregion

    #region PRIVATE METHODS

    private void CreateRandomizedState()
    {
        var playerIndex = Random.Range(0, _gameSettings.initialPlanetAmount);
        float previousOrbit = _gameSettings.MinimumOrbitRadius;
        gameModel.ClearPlanetStates();
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
            var randomColor = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f));
            var randomAngleToSun = Random.Range(0, Mathf.PI * 2);
            var isPlayer = playerIndex == i;
            var planetState = new PlanetModel(randomPlanetSetting,
                _gameSettings.initialPlanetHP,
                isPlayer,
                randomAngleToSun,
                randomColor
            );
            foreach (var rocket in _rocketSettings)
            {
                planetState.AddRocketAmmo(rocket.rocketType, Random.Range(rocket.minAmmo, rocket.maxAmmo));
            }

            gameModel.AddNewPlanetModel(planetState);
        }
    }

    private void ApplyStates()
    {
        _enemies.Clear();
        _celestialObjects.Clear();
        foreach (var planetState in gameModel.GetPlanetModels())
        {
            var planet = _planetFactory.Create(planetState);
            if (planetState.IsPlayer)
            {
                _playerPlanet = planet;
                _playerPlanet.onDieEvent = GameOver;
                foreach (var ammoInfo in planetState.GetAmmoModelList())
                {
                    _ammoPanel.SetRocketAmmo(ammoInfo.RocketType, ammoInfo.Ammo);
                }
            }
            else
            {
                SetEnemyOnDieListener(planet);
                _enemies.Add(planet);
            }

            _celestialObjects.Add(planet);
            _celestialObjects.Add(_sun);
        }
    }

    private void SetEnemyOnDieListener(PlanetController planet)
    {
        planet.onDieEvent = delegate
        {
            if (_enemies.All(enemy => enemy.IsDead))
            {
                GameWon();
            }
        };
    }

    private void ConfigureInput()
    {
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Space) && currentGameState == GameState.Running)
            .Subscribe(_ =>
            {
                _playerPlanet.Shoot();
                _ammoPanel.SetRocketAmmo(_playerPlanet.GetCurrentRocketSettings().rocketType,
                    _playerPlanet.GetCurrentAmmo());
                if (_playerPlanet.GetCurrentAmmo() <= 0)
                {
                    SwitchToNextNonEmptyAmmo();
                }
            }).AddTo(this);

        Observable.EveryUpdate().Where(_ => (Input.GetKeyDown(KeyCode.LeftShift) ||
                                             Input.GetKeyDown(KeyCode.RightShift)) &&
                                            currentGameState == GameState.Running).Subscribe(_ =>
        {
            SwitchToNextNonEmptyAmmo();
        }).AddTo(this);

        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Escape) && currentGameState == GameState.Running)
            .Subscribe(_ =>
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

    private void GameOver()
    {
        Time.timeScale = 0;
        currentGameState = GameState.Finished;
        _mainMenuController.Show(GameState.Finished);
    }

    private void GameWon()
    {
        Time.timeScale = 0;
        currentGameState = GameState.YouWin;
        _mainMenuController.Show(GameState.YouWin);
    }

    #endregion
}

public enum GameState
{
    MainMenu,
    Running,
    Paused,
    Finished,
    YouWin
}