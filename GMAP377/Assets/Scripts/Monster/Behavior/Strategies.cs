using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Pathfinding.BehaviourTrees {
    public interface IStrategy {
        Node.Status Process();
        void Reset() {
            //
        }
    }

    public class AttackStrategy : IStrategy {
        private readonly NavMeshAgent agent;
        private readonly float attackDuration;
        private readonly float recoveryDuration;
        private readonly float attackCooldown;

        private float attackEndTime;
        private float recoveryEndTime;
        private float nextAttackTime;

        private bool isAttacking;
        private bool isRecovering;

        public static event Action OnAttackStarted;
        public static event Action OnAttackFinished;

        public AttackStrategy(NavMeshAgent agent, float attackDuration, float recoveryDuration, float attackCooldown) {
            this.agent = agent;
            this.attackDuration = attackDuration;
            this.recoveryDuration = recoveryDuration;
            this.attackCooldown = attackCooldown;
        }

        public Node.Status Process() {
            //Start the attack
            if (!isAttacking && !isRecovering) {
                if (Time.time < nextAttackTime)
                    return Node.Status.Failure;
                
                isAttacking = true;

                if (agent.isOnNavMesh) {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }

                attackEndTime = Time.time + attackDuration;
                OnAttackStarted?.Invoke();
                return Node.Status.Running;
            }

            //Continuing the attack
            if (isAttacking) {
                if (Time.time < attackEndTime)
                    return Node.Status.Running;
                
                isAttacking = false;
                isRecovering = true;

                recoveryEndTime = Time.time + recoveryDuration;
                OnAttackFinished?.Invoke();
                return Node.Status.Running;
            }
            
            //Finish the attack
            if (isRecovering) {
                if (Time.time < recoveryEndTime)
                    return Node.Status.Running;

                isRecovering = false;
                nextAttackTime = Time.time + attackCooldown;

                if (agent.isOnNavMesh) {
                    agent.isStopped = false;
                }
                return Node.Status.Success;
            }

            return Node.Status.Failure;
        }

        public void Reset() {
            if (isAttacking || isRecovering) {
                if (agent.isOnNavMesh) {
                    agent.isStopped = false;
                }
            }

            isAttacking = false;
            isRecovering = false;
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
        readonly NavMeshAgent agent;
        readonly List<Transform> patrolPoints;
        readonly float patrolSpeed;
        int currentIndex;
        bool isPathCalculated;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f) {
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

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid) {
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
        private readonly NavMeshAgent agent;
        private readonly Func<Vector3> getDestination;
        private readonly float movementSpeed;

        public MoveToPositionStrategy(NavMeshAgent agent, Func<Vector3> getDestination, float movementSpeed) {
            this.agent = agent;
            this.getDestination = getDestination;
            this.movementSpeed = movementSpeed;
        }

        public Node.Status Process() {
            if (!agent.isOnNavMesh)
                return Node.Status.Failure;

            agent.speed = movementSpeed;
            
            Vector3 destination = getDestination();

            if (!agent.hasPath || (agent.destination - destination).sqrMagnitude > 0.25f) {
                agent.SetDestination(destination);
            }

            if (agent.pathPending) {
                return Node.Status.Running;
            }

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid) {
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