using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using SO;
using UniRx;
using UnityEngine;
using Views;
using Zenject;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class GameController: IDisposable
    {
        [Inject(Id = "sun")] private CelestialObject _sun;
        private PlanetViewController.Factory _planetFactory;
        private SettingsSO.GameSettings _gameSettings;
        private PlanetViewController _playerPlanet;
        private List<PlanetViewController> _enemies = new List<PlanetViewController>();
        private List<CelestialObject> _celestialObjects = new List<CelestialObject>();
        private List<RocketViewController> _rockets = new List<RocketViewController>();
        private MainMenuView _mainMenuView;
        private List<SettingsSO.RocketSettings> _rocketSettings;
        private AISystem _aiSystem;
        private RocketAmmoPanel _ammoPanel;
        private GameState currentGameState = GameState.MainMenu;
        private LocalSaveController _localSaveController;
        private GameModel gameModel = new GameModel();
        private RocketViewController.Factory _rocketFactory;
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        public List<CelestialObject> CelestialObjects => _celestialObjects;

        [Inject]
        void Construct(
            PlanetViewController.Factory planetFactory,
            SettingsSO.GameSettings gameSettings,
            List<SettingsSO.RocketSettings> rocketSettings,
            RocketAmmoPanel ammoPanel,
            LocalSaveController localSaveController,
            RocketViewController.Factory rocketFactory,
            AISystem aiSystem,
            MainMenuView mainMenuView
        )
        {
            _planetFactory = planetFactory;
            _rocketFactory = rocketFactory;
            _gameSettings = gameSettings;
            _rocketSettings = rocketSettings;
            _ammoPanel = ammoPanel;
            _aiSystem = aiSystem;
            _localSaveController = localSaveController;
            _mainMenuView = mainMenuView;
            Application.targetFrameRate = 60;
            ConfigureInput();
            SubscribeToUpdateAiSystem();
        }

       

        #region PUBLIC METHODS

        public void StartNewGame()
        {
            gameModel.CreateRandomizedState(_gameSettings, _rocketSettings);
            ApplyStates();
            ResumeGame();
            _aiSystem.RegisterEnemies(_enemies);
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

            foreach (var rocket in _rockets)
            {
                rocket.Dispose();
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

                _aiSystem.RegisterEnemies(_enemies);
            }, Debug.LogError);
        }

        public void SaveGame()
        {
            gameModel.ClearRocketStates();
            foreach (var rocket in _rockets)
            {
                gameModel.AddNewRocketModel(rocket.GetCurrentState());
            }
            var json = JsonUtility.ToJson(gameModel);
            _localSaveController.SaveProgress(json);
        }
        
        
        public void Dispose()
        {
            _compositeDisposable.Clear();
            _playerPlanet.OnDieEvent -= GameOver;
            foreach (var enemy in _enemies)
            {
                enemy.OnDieEvent -= CheckAllEnemiesDead;
            }

        }

        public void UnregisterRocket(RocketViewController rocketController)
        {
            Observable.IntervalFrame(1).Take(1).Subscribe(_ => { _rockets.Remove(rocketController); })
                .AddTo(_compositeDisposable);
        }

        public void RegisterRocket(RocketViewController rocketController)
        {
            _rockets.Add(rocketController);
        }

        #endregion

        #region PRIVATE METHODS

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
                    _playerPlanet.OnDieEvent += GameOver;
                    foreach (var ammoInfo in planetState.GetAmmoModelList())
                    {
                        _ammoPanel.SetRocketAmmo(ammoInfo.RocketType, ammoInfo.Ammo);
                    }
                }
                else
                {
                    planet.OnDieEvent += CheckAllEnemiesDead;
                    _enemies.Add(planet);
                }

                _celestialObjects.Add(planet);
                _celestialObjects.Add(_sun);
            }
        }

        private void CheckAllEnemiesDead()
        {
            if (_enemies.All(enemy => enemy.IsDead))
            {
                GameWon();
            }
        }

        private void ConfigureInput()
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Space) && currentGameState == GameState.Running)
                .Subscribe(_ =>
                {
                    _playerPlanet.Shoot();
                    _ammoPanel.SetRocketAmmo(_playerPlanet.GetCurrentRocketSettings().rocketType,
                        _playerPlanet.GetCurrentAmmo());
                    if (_playerPlanet.GetCurrentAmmo() <= 0)
                    {
                        SwitchToNextNonEmptyAmmo();
                    }
                }).AddTo(_compositeDisposable);

            Observable.EveryUpdate().Where(_ => (Input.GetKeyDown(KeyCode.LeftShift) ||
                                                 Input.GetKeyDown(KeyCode.RightShift)) &&
                                                currentGameState == GameState.Running).Subscribe(_ =>
            {
                SwitchToNextNonEmptyAmmo();
            }).AddTo(_compositeDisposable);

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape) && currentGameState == GameState.Running)
                .Subscribe(_ =>
                {
                    currentGameState = GameState.Paused;
                    Time.timeScale = 0;
                    _mainMenuView.Show(currentGameState);
                }).AddTo(_compositeDisposable);
        }
        
        private void SubscribeToUpdateAiSystem()
        {
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (currentGameState == GameState.Running)
                {
                    _aiSystem.MakeDecisions();
                }
            }).AddTo(_compositeDisposable);
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
            _mainMenuView.Show(GameState.Finished);
        }

        private void GameWon()
        {
            Time.timeScale = 0;
            currentGameState = GameState.YouWin;
            _mainMenuView.Show(GameState.YouWin);
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
}