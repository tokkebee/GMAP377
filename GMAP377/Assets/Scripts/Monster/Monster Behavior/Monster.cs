using System.Collections.Generic;
using Pathfinding.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterVision))]
public class Monster : MonoBehaviour {
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] GameObject player; //temporary. will be replaced by more sophisticated detection system later
    [SerializeField] private MonsterVision vision;
    [SerializeField] private float searchDuration = 4f;
    //assumed second "treasure" (from tutorial) [SerializeField] GameObject treasure2;

    private NavMeshAgent agent;
    private BehaviourTree tree;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<MonsterVision>();

        //highest to lowest priority: chase --> investigate --> patrol

        //priority 30: chase when player is visible
        Sequence chase = new Sequence("Chase", 30);
        chase.AddChild(new Leaf("CanChase", new Condition(() => vision.shouldChase && vision.canSeePlayer)));
        chase.AddChild(new Leaf("MoveToPlayer", new MoveToPositionStrategy(agent, () => player.transform.position)));

        //todo: attack behavior when agent reaches stopping distance

        //priority 20: investigate
        Sequence investigate = new Sequence("Investigate", 20);
        investigate.AddChild(new Leaf("ShouldChase", new Condition(() => vision.shouldChase && !vision.canSeePlayer && vision.hasLastKnownPosition)));
        investigate.AddChild(new Leaf("MoveToLastKnownPosition", new MoveToPositionStrategy(agent, () => vision.lastKnownPosition)));
        investigate.AddChild(new Leaf("SearchArea", new SearchStrategy(searchDuration)));
        investigate.AddChild(new Leaf("ClearAlert", new ActionStrategy(() => vision.ClearAlert())));

        //priority 10: patrol when no active alert
        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints), 10);

        PrioritySelector behaviors = new PrioritySelector("MonsterBehaviors");

        behaviors.AddChild(chase);
        behaviors.AddChild(investigate);
        behaviors.AddChild(patrol);

        tree = new BehaviourTree("Monster");
        tree.AddChild(behaviors);

        // //TEMPORARY PLAYER DETECTION SYSTEM
        // Leaf isPlayerPresent = new Leaf("IsPlayerPresent", new Condition(() => player.activeSelf));
        // Leaf moveToPlayer = new Leaf("MoveToPlayer", new ActionStrategy(() => agent.SetDestination(player.transform.position)));

        // Sequence goToPlayer = new Sequence("GoToPlayer", 10);
        // goToPlayer.AddChild(new Leaf("isPlayerPresent", new Condition(() => player.activeSelf)));
        // goToPlayer.AddChild(new Leaf("moveToPlayer", new ActionStrategy(() => agent.SetDestination(player.transform.position))));
        // tree.AddChild(goToPlayer);
    }

    void Update() {
        tree.Process();
    }

}