using System.Collections.Generic;
using Pathfinding.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

//This script constructs and processes the Monster Behavior tree
//Dependant on Node.cs and Strategies.cs
//Expecting to be or is currently hooked up to the following
    //monster animation systems
        //LegAnimation.cs (placeholder)
    //player manager
    //game manager

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterVision))]
public class Monster : MonoBehaviour
{
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] Transform player; //temporary. will be replaced by more sophisticated detection system later
    [SerializeField] private MonsterVision vision;
    [SerializeField] private float searchDuration = 4f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDuration = 0.7f;
    [SerializeField] private float attackRecoveryDuration = 0.4f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float investigationSpeed = 3f;

    private NavMeshAgent agent;
    private BehaviourTree tree;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<MonsterVision>();

        //Constructing Behavior Tree

        //Creating the main behaviors
        //highest to lowest priority: attack --> chase --> investigate --> patrol
        AttackStrategy attackStrategy = new AttackStrategy(agent, attackDuration, attackRecoveryDuration, attackCooldown);
        Sequence attack = new Sequence("Attack", 40);
        attack.AddChild(new Leaf("CanSeePlayer", new Condition(() => vision.canSeePlayer)));
        attack.AddChild(new Leaf("IsPlayerInAttackRange", new Condition(() => IsPlayerInAttackRange())));
        attack.AddChild(new Leaf("EnteringAttack",
            new ActionStrategy(() =>
                Debug.Log("Monster entering attacking behavior.", this
            ))));
        attack.AddChild(new Leaf("PerformAttack", attackStrategy));

        //priority 30: chase when player is visible
        ReactiveSequence  chase = new ReactiveSequence ("Chase", 30);
        chase.AddChild(new Leaf("ShouldChase", new Condition(() => vision.shouldChase && vision.canSeePlayer)));
        // chase.AddChild(new Leaf("ShouldChase",
        //     new Condition(() =>
        //     {
        //         bool shouldInvestigate =
        //             vision.shouldChase &&
        //             vision.canSeePlayer;

        //         if (vision.shouldChase)
        //             Debug.Log("Monster entering chasing behavior");

        //         return shouldInvestigate;
        //     })));
        // chase.AddChild(new Leaf("CanChase", new Condition(() => vision.shouldChase && vision.canSeePlayer)));
        chase.AddChild(new Leaf("MoveToPlayer", new MoveToPositionStrategy(agent, () => player.transform.position, chaseSpeed)));

        //priority 20: investigate
        //todo. feeling of investigation is lackluster
        Sequence investigate = new Sequence("Investigate", 20);
        investigate.AddChild(new Leaf("ShouldInvestigate",
            new Condition(() =>
            {
                bool shouldInvestigate =
                    vision.shouldChase &&
                    !vision.canSeePlayer &&
                    vision.hasLastKnownPosition;

                if (shouldInvestigate)
                    Debug.Log("Monster entering investigation behavior");

                return shouldInvestigate;
            })));
        investigate.AddChild(new Leaf("ShouldChase", new Condition(() => vision.shouldChase && !vision.canSeePlayer && vision.hasLastKnownPosition)));
        investigate.AddChild(new Leaf("MoveToLastKnownPosition", new MoveToPositionStrategy(agent, () => vision.lastKnownPosition, investigationSpeed)));
        investigate.AddChild(new Leaf("SearchArea", new SearchStrategy(searchDuration)));
        investigate.AddChild(new Leaf("ClearAlert", new ActionStrategy(() => vision.ClearAlert())));

        //priority 10: patrol when no active alert
        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints, patrolSpeed), 10);


        //Creating "Priority Selector", which monster quickly switches behaviors if conditions allow for a higher priority behavior
        PrioritySelector behaviors = new PrioritySelector("MonsterBehaviors");

        behaviors.AddChild(attack);
        behaviors.AddChild(chase);
        behaviors.AddChild(investigate);
        behaviors.AddChild(patrol);

        tree = new BehaviourTree("Monster");
        tree.AddChild(behaviors);
    }

    void Update() {
        tree.Process();
    }

    private bool IsPlayerInAttackRange() {
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

}