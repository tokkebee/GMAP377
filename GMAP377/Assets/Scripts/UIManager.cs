using System;
using UnityEngine;
using GameState = GameManager.GameState;

public class UIManager : MonoBehaviour {
    [Header("GUI Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    public static event Action<GameState> OnGamePaused;
    public static event Action<GameState> OnGameUnPaused;

    private void OnEnable() {
        GameManager.OnStateChanged += UpdateUI;
    }

    private void OnDisable() {
        GameManager.OnStateChanged -= UpdateUI;
    }

    private void UpdateUI(GameState newState) {
        hudPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        switch (newState) {
            case GameState.MainMenu:
                //awaiting logic
                break;
            case GameState.Paused:
                //awaiting logic
                break;
            case GameState.Playing:
                hudPanel.SetActive(true);
                break;
            case GameState.Win:
                winPanel.SetActive(true);
                break;
            case GameState.Lose:
                losePanel.SetActive(true);
                break;
        }
    }
}
