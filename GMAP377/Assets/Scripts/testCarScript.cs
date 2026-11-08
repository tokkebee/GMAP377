using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class testCarScript : MonoBehaviour
{
    [SerializeField]private List<string> partsRepaired;
    [SerializeField]private List<string> partsBroken;
    private GameManagerScript gameManager;

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

    public void Repair()
    {
        foreach (string carPart in gameManager.inventory)
        {
            if (partsBroken.Contains(carPart))
            {
                partsRepaired.Add(carPart);
                Debug.Log(carPart);
            }
        }
        foreach (string carPart in partsRepaired)
        {
            partsBroken.Remove(carPart);
            gameManager.inventory.Remove(carPart);
        }
    }
}
