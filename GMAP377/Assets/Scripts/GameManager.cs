using System;
using UnityEngine;
using UnityEngine.SceneManagement;

//This script handles game states.
//All other systems listen to OnStateChanged event

public enum GameState {
        MainMenu, //TODO: implement logic for "Main Menu" state
        Paused, //TODO: implement logic for "Paused" state
        Playing,
        Win,
        Lose
    }

public class GameManager : MonoBehaviour {
    [SerializeField] private GameState currentGameState;
    public static event Action<GameState> OnStateChanged;

    private void OnEnable() {
        PlayerHealth.OnPlayerDeath += GameOver;
    }

    private void OnDisable() {
        PlayerHealth.OnPlayerDeath -= GameOver;
    }

    //Spawns player at the passed position
    private void SpawnPlayer(Transform position) {
        //Ollie is leaving this open for now.
        //Intention is that the Save/File system saves checkpoints and passed those checkpoints here upon respawn
    }

    //Completely restarts game to new game file
    public void RestartGame() {
        //TODO: restart logic

        //TEMPORARY. MUST BE REPLACED
        SceneManager.LoadScene("Monster_UnitTest");
    }

    public void QuitGame() {
        //Debug.Log("Successfully quit to desktop!");
        Application.Quit();
    }

    private void GameOver() {
        ChangeState(GameState.Lose);
    }

    public void ChangeState(GameState newState) {
        if (currentGameState == newState) return;

        currentGameState = newState;

        OnStateChanged?.Invoke(newState);
    }
}
