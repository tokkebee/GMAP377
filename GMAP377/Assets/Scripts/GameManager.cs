using System;
using UnityEngine;
using UnityEngine.SceneManagement;

//This script handles game states.
//All other systems listen to OnStateChanged event

public class GameManager : MonoBehaviour {
    public enum GameState {
        MainMenu, //TODO: implement logic for "Main Menu" state
        Paused, //TODO: implement logic for "Paused" state
        Playing,
        Win,
        Lose
    }

    [SerializeField] private GameState currentGameState;
    public static event Action<GameState> OnStateChanged;

    private void OnEnable() {
        PlayerHealth.OnPlayerDeath += GameOver;
    }

    private void OnDisable() {
        PlayerHealth.OnPlayerDeath -= GameOver;
    }

    private void Start() {
        ApplyState(currentGameState);
        OnStateChanged?.Invoke(currentGameState);
    }

    public void TogglePause() {
        if (currentGameState == GameState.Playing) ChangeState(GameState.Paused);
        else if (currentGameState == GameState.Paused) ChangeState(GameState.Playing);
    }

    //Spawns player at the passed position
    //This logic should be moved to a player manager script
    private void SpawnPlayer(Transform position) {
        //Ollie is leaving this open for now.
        //Intention is that the Save/File system saves checkpoints and passed those checkpoints here upon respawn
    }

    public void QuitGame() {
        Debug.Log("Successfully quit to desktop!");
        Application.Quit();
    }

    private void GameOver() {
        ChangeState(GameState.Lose);
    }

    public void ChangeState(GameState newState) {
        if (currentGameState == newState) return;

        currentGameState = newState;
        ApplyState(newState);
        OnStateChanged?.Invoke(newState);
    }

    public void ApplyState(GameState state) {
        bool frozen = state != GameState.Playing;
        Cursor.lockState = frozen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = frozen;
        Time.timeScale = frozen ? 0f : 1f;
        AudioListener.pause = state == GameState.Paused;
    }

    //Completely restarts game to new game file
    public void RestartGame() {
        //TODO: restart logic
        Time.timeScale = 1f;

        //TEMPORARY. MUST BE REPLACED
        SceneManager.LoadScene("Monster_UnitTest");
    }
}
