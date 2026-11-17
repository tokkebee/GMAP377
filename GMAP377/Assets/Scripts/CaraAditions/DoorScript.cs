using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : InteractBase
{
    public string travelScene;
    public Vector3 spawnPoint;
    public Quaternion spawnRotation;
    private PlayerManager playerManager;
    public bool additiveSceneToggle = false;
    private void OnEnable()
    {

        //SceneManager.sceneLoaded += OnSceneLoaded; 
         //Doors find the playermanager to reference
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //playerManager = FindAnyObjectByType<PlayerManager>(); //Doors find the playermanager to reference
    }

    private void OnDisable()
    {
        //SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void Click() //tells the playerManager where it should spawn the player and where that player should be facing when they spawn in
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
        playerManager.spawnPoint = spawnPoint;
        playerManager.spawnRotation = spawnRotation;
        Debug.Log(playerManager.spawnPoint + " = " + spawnPoint);
        if (playerManager.spawnPoint == spawnPoint)
        {
            SceneManager.LoadScene(travelScene, additiveSceneToggle ? LoadSceneMode.Additive : LoadSceneMode.Single);   
        }
    }
}
