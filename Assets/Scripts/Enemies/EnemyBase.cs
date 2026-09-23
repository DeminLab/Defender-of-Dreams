using System;
using DefenderOfDreams.Abilities;
using DefenderOfDreams.Combat;
using UnityEngine;

namespace DefenderOfDreams.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Sleep,
        Dead
    }

    public abstract class EnemyBase : MonoBehaviour, IDamageable, ISleepable
    {
        [Header("Common (4.5 data-driven)")]
        [SerializeField] protected int maxHealth = 3;
        [SerializeField] protected float moveSpeed = 2.5f;
        [SerializeField] protected float chaseSpeed = 4f;
        [SerializeField] protected float detectionRadius = 6f;
        [SerializeField] protected float attackRadius = 1.1f;
        [SerializeField] protected int attackDamage = 1;
        [SerializeField] protected float attackCooldown = 1.2f;
        [SerializeField] protected float patrolRadius = 3f;
        [SerializeField] protected float stateTickInterval = 0.2f;

        protected int CurrentHealth;
        protected EnemyState State = EnemyState.Idle;
        protected Transform Target;
        protected float AttackTimer;
        protected float SleepTimer;
        protected float TickTimer;
        protected Vector2 Home;
        protected Vector2 PatrolTarget;
        protected Rigidbody2D Rb;
        protected bool ControlsSeized;

        public event Action<EnemyBase> Died;

        public bool IsAlive => State != EnemyState.Dead;
        public EnemyState CurrentState => State;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
            Rb = GetComponent<Rigidbody2D>();
            Home = transform.position;
            PatrolTarget = Home;
            if (Rb != null)
            {
                Rb.gravityScale = 0f;
                Rb.freezeRotation = true;
            }
        }

        protected virtual void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                Target = player.transform;
        }

        protected virtual void Update()
    {
            if (State == EnemyState.Dead)
                return;

            AttackTimer -= Time.deltaTime;

            if (SleepTimer > 0f)
            {
                SleepTimer -= Time.deltaTime;
                if (State != EnemyState.Sleep)
                    EnterState(EnemyState.Sleep);
                if (SleepTimer <= 0f)
                    EnterState(EnemyState.Idle);
                return;
            }

            if (State == EnemyState.Sleep)
                return;

            TickTimer -= Time.deltaTime;
            if (TickTimer <= 0f)
            {
                TickTimer = stateTickInterval;
                Think();
            }

            Act();
        }

        protected abstract void Think();

        protected abstract void Act();

        protected virtual void EnterState(EnemyState next)
        {
            if (State == EnemyState.Dead)
                return;
            State = next;
            if (Rb != null && next == EnemyState.Sleep)
                Rb.linearVelocity = Vector2.zero;
        }

        protected bool HasLineToTarget(float radius)
        {
            if (Target == null)
                return false;
            return ((Vector2)Target.position - (Vector2)transform.position).sqrMagnitude <= radius * radius;
        }

        protected void MoveTowards(Vector2 point, float speed)
        {
            if (Rb == null || ControlsSeized)
                return;
            Vector2 dir = (point - (Vector2)transform.position);
            if (dir.sqrMagnitude < 0.01f)
            {
                Rb.linearVelocity = Vector2.zero;
                return;
            }
            Rb.linearVelocity = dir.normalized * speed;
        }

        protected void StopMoving()
        {
            if (Rb != null)
                Rb.linearVelocity = Vector2.zero;
        }

        protected virtual void TryAttackPlayer()
        {
            if (AttackTimer > 0f || Target == null)
                return;
            if (!HasLineToTarget(attackRadius))
                return;
            AttackTimer = attackCooldown;
            var damageable = Target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 dir = ((Vector2)Target.position - (Vector2)transform.position).normalized;
                damageable.TakeDamage(attackDamage, dir);
            }
        }

        public virtual void TakeDamage(int amount, Vector2 hitDirection)
        {
            if (!IsAlive || amount <= 0)
                return;
            CurrentHealth -= amount;
            if (State == EnemyState.Idle || State == EnemyState.Patrol)
                EnterState(EnemyState.Chase);
            if (CurrentHealth <= 0)
                Die();
        }

        public void Sleep(float duration)
        {
            if (!IsAlive)
                return;
            SleepTimer = Mathf.Max(SleepTimer, duration);
        }

        public void Forget()
        {
            if (!IsAlive)
                return;
            CurrentHealth = 0;
            Die();
        }

        public void SeizeControls(bool seized)
        {
            ControlsSeized = seized;
            if (seized)
                StopMoving();
        }

        protected virtual void Die()
        {
            EnterState(EnemyState.Dead);
            StopMoving();
            if (Rb != null)
                Rb.simulated = false;
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
            Died?.Invoke(this);
            Destroy(gameObject, 0.05f);
        }
    }
}
