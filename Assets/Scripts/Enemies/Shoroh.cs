using System;
using DefenderOfDreams.Combat;
using UnityEngine;

namespace DefenderOfDreams.Enemies
{
    public class Shoroh : EnemyBase
    {
        [Header("Shoroh (wisp)")]
        [SerializeField] private float wanderRadius = 4f;
        [SerializeField] private float retargetInterval = 2f;

        private float _retargetTimer;

        protected override void Start()
        {
            base.Start();
            PickPatrolTarget();
        }

        protected override void Think()
        {
            switch (State)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    if (HasLineToTarget(detectionRadius))
                    {
                        EnterState(EnemyState.Chase);
                        return;
                    }

                    _retargetTimer -= stateTickInterval;
                    if (_retargetTimer <= 0f || ((Vector2)PatrolTarget - Home).sqrMagnitude > wanderRadius * wanderRadius)
                    {
                        PickPatrolTarget();
                        _retargetTimer = retargetInterval;
                        EnterState(EnemyState.Patrol);
                    }
                    break;

                case EnemyState.Chase:
                    if (!HasLineToTarget(detectionRadius * 1.3f))
                    {
                        EnterState(EnemyState.Patrol);
                        return;
                    }

                    if (HasLineToTarget(attackRadius))
                        EnterState(EnemyState.Attack);
                    break;

                case EnemyState.Attack:
                    if (!HasLineToTarget(attackRadius * 1.2f))
                        EnterState(EnemyState.Chase);
                    break;
            }
        }

        protected override void Act()
        {
            switch (State)
            {
                case EnemyState.Patrol:
                    MoveTowards(PatrolTarget, moveSpeed);
                    break;
                case EnemyState.Chase:
                    if (Target != null)
                        MoveTowards(Target.position, chaseSpeed);
                    break;
                case EnemyState.Attack:
                    StopMoving();
                    TryAttackPlayer();
                    break;
                default:
                    StopMoving();
                    break;
            }
        }

        private void PickPatrolTarget()
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            PatrolTarget = Home + offset;
            _retargetTimer = retargetInterval;
        }
    }
}
