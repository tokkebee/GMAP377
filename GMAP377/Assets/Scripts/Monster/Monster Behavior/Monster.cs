using System.Collections.Generic;
using Pathfinding.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Monster : MonoBehaviour {
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] GameObject player; //temporary. will be replaced by more sophisticated detection system later
    //assumed second "treasure" (from tutorial) [SerializeField] GameObject treasure2;

    UnityEngine.AI.NavMeshAgent agent;
    BehaviourTree tree;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();

        tree = new BehaviourTree("Monster");
        //tree.AddChild(new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints)));

        //TEMPORARY PLAYER DETECTION SYSTEM
        Leaf isPlayerPresent = new Leaf("IsPlayerPresent", new Condition(() => player.activeSelf));
        Leaf moveToPlayer = new Leaf("MoveToPlayer", new ActionStrategy(() => agent.SetDestination(player.transform.position)));

        Sequence goToPlayer = new Sequence("GoToPlayer", 10);
        goToPlayer.AddChild(new Leaf("isPlayerPresent", new Condition(() => player.activeSelf)));
        goToPlayer.AddChild(new Leaf("moveToPlayer", new ActionStrategy(() => agent.SetDestination(player.transform.position))));
        tree.AddChild(goToPlayer);

        // //part of the tutorial
        // Sequence goToTreasure2 = new Sequence("GoToTreasure2", 20);
        // gotToTreasure2.AddChild(new Leaf("IsTreasure2Present", new Condition(() => treasure2.activeSelf)));
        // goToTreasure2.AddChild(new Leaf("MoveToTreasure2", new ActionStrategy(() => agent.SetDestination(treasure2.transform.position))));

        // PrioritySelector goToTreasures = new PrioritySelector("GoToTreasures");
        // goToTreasures.AddChild(goToTreasure2);
        // goToTreasures.AddChild(goToPlayer);
        
    }

    void Update() {
        tree.Process();
    }

}