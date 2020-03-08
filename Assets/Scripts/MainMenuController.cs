using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continuerButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private TextMeshProUGUI statusText;
    private CanvasGroup _canvasGroup;
    private GameController _gameController;
    private GameState currentGameState = GameState.MainMenu;

    [Inject]
    void Construct(GameController gameController)
    {
        _gameController = gameController;
    }
    
    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        saveGameButton.gameObject.SetActive(false);
        newGameButton.OnClickAsObservable().Subscribe(_ =>
        {
            if (currentGameState != GameState.MainMenu)
            {
                _gameController.DisposePools();
            }

            _gameController.StartNewGame();
            HideMenu();
        }).AddTo(this);

        saveGameButton.OnClickAsObservable().Subscribe(_ =>
        {
            _gameController.SaveGame();
            saveGameButton.interactable = false;
        }).AddTo(this);

        continuerButton.gameObject.SetActive(_gameController.CheckSaveFileExists());
        CheckLoadButton();
        loadGameButton.OnClickAsObservable().Subscribe(_ =>
        {
            _gameController.LoadGame();
            HideMenu();
        }).AddTo(this);
        continuerButton.onClick.AddListener(delegate
        {
            _gameController.LoadGame();
            HideMenu();
        });
    }

    private void CheckLoadButton()
    {
        loadGameButton.gameObject.SetActive(_gameController.CheckSaveFileExists());
    }

    private void HideMenu()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Show(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.MainMenu:
                saveGameButton.gameObject.SetActive(false);
                break;
            case GameState.Running:
                break;
            case GameState.Paused:
                statusText.text = "PAUSED";
                saveGameButton.gameObject.SetActive(true);
                saveGameButton.interactable = true;
                CheckLoadButton();
                continuerButton.onClick.RemoveAllListeners();
                continuerButton.onClick.AddListener(delegate
                {
                    HideMenu();
                    _gameController.ResumeGame();
                });
                break;
            case GameState.Finished:
                statusText.text = "GAME OVER";
                saveGameButton.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameState), gameState, null);
        }

        currentGameState = gameState;
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
    }
}
