using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    static PlayerManager Instance;
    [SerializeField] private GameObject player;
    public Vector3 spawnPoint;
    public Quaternion spawnRotation;
    public List<string> inventory;

    void Awake() //Checks if a PlayerManager already exists and destroys itself if it does
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); //PlayerManager persists through scenes
        SceneManager.activeSceneChanged += ChangeScene;
    }

    //void Update()
    //{
    // if (InputSystem.actions["Inventory"].WasPressedThisFrame()) 
    //     foreach (string item in inventory) Debug.Log(item);
    //}

    private static void ChangeScene(Scene arg0, Scene arg1) //Spawns in a new player on scene change
    //if a player already exists, just teleport it to where it's supposed to be
    {
        //Debug.Log("THIS RUNS");
        GameObject player = GameObject.Find("Player");
        if (player == null){
            //Debug.Log("New Player Spawned");
            Instantiate(Instance.player, Instance.spawnPoint, Instance.spawnRotation);
        }
        else
        {
            player.GetComponent<CharacterController>().enabled = false; //this is so the transform can be update in code
            player.transform.position = Instance.spawnPoint;
            player.transform.rotation = Instance.spawnRotation;
            player.GetComponent<CharacterController>().enabled = true;
            Debug.Log("Player moved");
            
        }
        foreach (string item in Instance.inventory) //so like, this should be more complicated than this in a full release
        //but right now it works by deleting the first item with the same name as something in the player's inventory from the map.
        {
            Destroy(GameObject.Find(item));
        }
    }
}
