using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : InteractBase
{
    public string travelScene;
    public Vector3 spawnPoint;

    public Quaternion spawnRotation;

    private GameManagerScript gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameManager = FindAnyObjectByType<GameManagerScript>();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void Click()
    {
        gameManager.spawnPoint = spawnPoint;
        gameManager.spawnRotation = spawnRotation;
        SceneManager.LoadScene(travelScene, LoadSceneMode.Single);
    }
}
