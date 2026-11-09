using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class testCarScript : InteractBase
{
    [SerializeField]private List<string> partsRepaired;
    public List<string> partsBroken;
    private GameManagerScript gameManager;
    private GameManager stateManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameManager = FindAnyObjectByType<GameManagerScript>();
        stateManager = FindAnyObjectByType<GameManager>();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void Click()
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
        if (partsBroken.Count == 0) stateManager.ChangeState(GameManager.GameState.Win);
    }
}
