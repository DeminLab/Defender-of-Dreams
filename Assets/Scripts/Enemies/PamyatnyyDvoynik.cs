using DefenderOfDreams.FogOfWar;
using UnityEngine;

namespace DefenderOfDreams.Enemies
{
    /// <summary>
    /// Memory Wraith: a light-weight enemy that becomes more aggressive
    /// while the player is exploring forgotten space.
    /// Attach to a prefab with Rigidbody2D + Collider2D.
    /// </summary>
    public class PamyatnyyDvoynik : EnemyBase
    {
        [Header("Memory Wraith")]
        [SerializeField, Min(0.1f)] private float hiddenSpeedMultiplier = 1.35f;
        [SerializeField, Min(0f)] private float revealRadius = 1.25f;
        [SerializeField, Min(0f)] private float vanishDelay = 2.5f;

        private float _hiddenTimer;

        protected override void Think()
        {
            bool visible = FogOfWarSystem.Instance == null ||
                           FogOfWarSystem.Instance.IsVisible(transform.position);

            if (!visible)
            {
                _hiddenTimer += stateTickInterval;
                if (_hiddenTimer >= vanishDelay)
                {
                    // The wraith relocates toward the player rather than dealing
                    // damage while completely hidden, keeping the mechanic fair.
                    _hiddenTimer = 0f;
                }
            }
            else
            {
                _hiddenTimer = 0f;
            }

            switch (State)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    if (HasLineToTarget(detectionRadius))
                        EnterState(EnemyState.Chase);
                    else
                        EnterState(EnemyState.Patrol);
                    break;

                case EnemyState.Chase:
                    if (HasLineToTarget(attackRadius))
                        EnterState(EnemyState.Attack);
                    else if (!HasLineToTarget(detectionRadius * 1.4f))
                        EnterState(EnemyState.Patrol);
                    break;

                case EnemyState.Attack:
                    if (!HasLineToTarget(attackRadius * 1.15f))
                        EnterState(EnemyState.Chase);
                    break;
            }
        }

        protected override void Act()
        {
            float speed = moveSpeed;

            if (FogOfWarSystem.Instance != null &&
                !FogOfWarSystem.Instance.IsVisible(transform.position))
                speed *= hiddenSpeedMultiplier;

            switch (State)
            {
                case EnemyState.Patrol:
                    MoveTowards(Home, speed * 0.65f);
                    break;

                case EnemyState.Chase:
                    if (Target != null)
                        MoveTowards(Target.position, speed);
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
    }
}
