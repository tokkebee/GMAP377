using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class testCarScript : InteractBase
{
    [SerializeField]private List<string> partsRepaired;
    public List<string> partsBroken;
    private PlayerManager playerManager;
    private GameManager stateManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
        stateManager = FindAnyObjectByType<GameManager>();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void Click()
    {
        foreach (string carPart in playerManager.inventory)
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
            playerManager.inventory.Remove(carPart);
        }
        if (partsBroken.Count == 0) stateManager.ChangeState(GameManager.GameState.Win);
    }
}
