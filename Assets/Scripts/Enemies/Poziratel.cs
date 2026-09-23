using UnityEngine;

namespace DefenderOfDreams.Enemies
{
    public class Poziratel : EnemyBase
    {
        [Header("Devourer (4.5)")]
        [SerializeField] private float consumeInterval = 3f;
        [SerializeField] private float consumeRadius = 2.5f;
        [SerializeField] private float guardWanderRadius = 2f;

        private float _consumeTimer;

        protected override void Think()
        {
            switch (State)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    if (HasLineToTarget(detectionRadius))
                        EnterState(EnemyState.Chase);
                    else if (State == EnemyState.Idle)
                        EnterState(EnemyState.Patrol);
                    break;

                case EnemyState.Chase:
                    if (HasLineToTarget(attackRadius))
                        EnterState(EnemyState.Attack);
                    else if (!HasLineToTarget(detectionRadius * 1.5f))
                        EnterState(EnemyState.Patrol);
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
                    MoveTowards(Home + (Vector2)(Quaternion.Euler(0, 0, Time.time * 20f) * (Vector3)(Vector2.right * guardWanderRadius)), moveSpeed * 0.6f);
                    break;
                case EnemyState.Chase:
                    if (Target != null)
                        MoveTowards(Target.position, chaseSpeed);
                    break;
                case EnemyState.Attack:
                    StopMoving();
                    TryAttackPlayer();
                    ConsumeTick();
                    break;
                default:
                    StopMoving();
                    break;
            }
        }

        private void ConsumeTick()
        {
            _consumeTimer -= Time.deltaTime;
            if (_consumeTimer > 0f)
                return;
            _consumeTimer = consumeInterval;
            var fog = DefenderOfDreams.FogOfWar.FogOfWarSystem.Instance;
            if (fog != null)
                fog.ConsumeForever(transform.position, consumeRadius);
        }
    }
}
