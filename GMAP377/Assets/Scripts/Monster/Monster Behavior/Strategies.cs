using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;

namespace Pathfinding.BehaviourTrees {
    public interface IStrategy {
        Node.Status Process();
        void Reset() {
            //
        }
    }

    public class ActionStrategy : IStrategy {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething) {
            this.doSomething = doSomething;
        }

        public Node.Status Process() {
            doSomething();
            return Node.Status.Success;
        }
    }

    public class Condition : IStrategy {
        readonly Func<bool> predicate;

        public Condition(Func<bool> predicate) {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class PatrolStrategy : IStrategy {
        readonly Transform entity;
        readonly UnityEngine.AI.NavMeshAgent agent;
        readonly List<Transform> patrolPoints;
        readonly float patrolSpeed;
        int currentIndex;
        bool isPathCalculated;

        public PatrolStrategy(Transform entity, UnityEngine.AI.NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f) {
            this.entity = entity;
            this.agent = agent;
            this.patrolPoints = patrolPoints;
            this.patrolSpeed = patrolSpeed;
        }

        public Node.Status Process() {
            if (patrolPoints == null || patrolPoints.Count == 0)
                return Node.Status.Failure;
            
            int attempts = 0;
            while (attempts < patrolPoints.Count && patrolPoints[currentIndex] == null) {
                currentIndex = (currentIndex + 1) % patrolPoints.Count;
                attempts++;
            }

            if (attempts == patrolPoints.Count) {
                return Node.Status.Failure;
            }

            Transform target = patrolPoints[currentIndex];

            agent.speed = patrolSpeed;

            if (!agent.hasPath || (agent.destination - target.position).sqrMagnitude > 0.25f) {
                agent.SetDestination(target.position);
            }

            if (agent.pathPending) {
                return Node.Status.Running;
            }

            if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                return Node.Status.Failure;
            }

            if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.1f) {
                currentIndex = (currentIndex + 1) % patrolPoints.Count;
                return Node.Status.Running;
            }

            return Node.Status.Running;
        }

        // //old Process method stops at final waypoint
        // public Node.Status Process() {
        //     if (currentIndex == patrolPoints.Count) return Node.Status.Success;

        //     var target = patrolPoints[currentIndex];
        //     agent.SetDestination(target.position);
        //     entity.LookAt(target);

        //     if (isPathCalculated && agent.remainingDistance < 0.1f) {
        //         currentIndex++;
        //         isPathCalculated = false;
        //     }

        //     if (agent.pathPending) {
        //         isPathCalculated = true;
        //     }

        //     return Node.Status.Running;
        // }

        public void Reset() => currentIndex = 0;
    }

    public class MoveToPositionStrategy : IStrategy {
        private readonly UnityEngine.AI.NavMeshAgent agent;
        private readonly Func<Vector3> getDestination;

        public MoveToPositionStrategy(UnityEngine.AI.NavMeshAgent agent, Func<Vector3> getDestination) {
            this.agent = agent;
            this.getDestination = getDestination;
        }

        public Node.Status Process() {
            if (!agent.isOnNavMesh)
                return Node.Status.Failure;
            
            Vector3 destination = getDestination();

            if (!agent.hasPath || (agent.destination - destination).sqrMagnitude > 0.25f) {
                agent.SetDestination(destination);
            }

            if (agent.pathPending) {
                return Node.Status.Running;
            }

            if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                return Node.Status.Failure;
            }

            if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.1f) {
                return Node.Status.Success;
            }

            return Node.Status.Running; //haha literally
        }
    }

    public class SearchStrategy : IStrategy {
        private readonly float searchDuration;
        private float elapsed;

        public SearchStrategy(float searchDuration) {
            this.searchDuration = searchDuration;
        }

        public Node.Status Process() {
            elapsed += Time.deltaTime;

            return elapsed >= searchDuration ? Node.Status.Success : Node.Status.Running;
        }

        public void Reset() {
            elapsed = 0f;
        }
    }
}