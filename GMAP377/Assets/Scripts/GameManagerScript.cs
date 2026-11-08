using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    private static GameManagerScript Instance;
    [SerializeField] private GameObject player;
    public Vector3 spawnPoint;
    public Quaternion spawnRotation;
    public List<string> inventory;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += ChangeScene;
    }

    void Update()
    {
        if (InputSystem.actions["Inventory"].WasPressedThisFrame())
        {
            foreach (string item in inventory)
            {
                Debug.Log(item);
            }
        }
    }

    private static void ChangeScene(Scene from, Scene to)
    {

        Instantiate(Instance.player, Instance.spawnPoint, Instance.spawnRotation);
        foreach (string item in Instance.inventory)
        {
            Destroy(GameObject.Find(item));
        }
    }
}
