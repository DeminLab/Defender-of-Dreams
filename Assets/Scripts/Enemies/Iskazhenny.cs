using DefenderOfDreams.Core;
using UnityEngine;

namespace DefenderOfDreams.Enemies
{
    public class Iskazhenny : EnemyBase
    {
        [Header("Iskazhenny (4.6.3)")]
        [SerializeField] private string combatLineKey = "iskazhenny.combat";
        [SerializeField] private string criticalLineKey = "iskazhenny.critical";
        [SerializeField] private string deathLineKey = "iskazhenny.death";
        [SerializeField] private float criticalHealthThreshold = 0.35f;
        [SerializeField] private float wanderRadius = 3f;
        [SerializeField] private float retargetInterval = 2.5f;

        private bool _combatLineShown;
        private bool _criticalLineShown;
        private float _retargetTimer;

        protected override void Start()
        {
            base.Start();
            PickPatrolTarget();
        }

        protected override void EnterState(EnemyState next)
        {
            base.EnterState(next);

            if (next == EnemyState.Chase || next == EnemyState.Attack)
            {
                if (!_combatLineShown)
                {
                    _combatLineShown = true;
                    GameEvents.RaiseDialogLineShown(
                        Localization.Get("speaker.iskazhenny"),
                        Localization.Get(combatLineKey));
                }
            }
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
                        EnterState(EnemyState.Patrol);
                    }
                    break;

                case EnemyState.Chase:
                    if (!HasLineToTarget(detectionRadius * 1.4f))
                    {
                        EnterState(EnemyState.Patrol);
                        return;
                    }

                    if (HasLineToTarget(attackRadius))
                        EnterState(EnemyState.Attack);
                    break;

                case EnemyState.Attack:
                    if (!HasLineToTarget(attackRadius * 1.25f))
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

        public override void TakeDamage(int amount, Vector2 hitDirection)
        {
            int before = CurrentHealth;
            base.TakeDamage(amount, hitDirection);

            if (!_criticalLineShown && IsAlive && before > 0 && maxHealth > 0)
            {
                float hp01 = CurrentHealth / (float)maxHealth;
                if (hp01 <= criticalHealthThreshold)
                {
                    _criticalLineShown = true;
                    GameEvents.RaiseDialogLineShown(
                        Localization.Get("speaker.iskazhenny"),
                        Localization.Get(criticalLineKey));
                }
            }
        }

        protected override void Die()
        {
            GameEvents.RaiseDialogLineShown(
                Localization.Get("speaker.iskazhenny"),
                Localization.Get(deathLineKey));
            base.Die();
        }

        private void PickPatrolTarget()
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            PatrolTarget = Home + offset;
            _retargetTimer = retargetInterval;
        }
    }
}
