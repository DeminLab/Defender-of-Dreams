using System;
using DefenderOfDreams.Core;
using UnityEngine;

namespace DefenderOfDreams.Enemies
{
    public class TeacherBoss : EnemyBase
    {
        [Header("Boss «Учитель» (4.7)")]
        [SerializeField] private float phase2Threshold = 0.5f;
        [SerializeField] private float phase1AttackRadius = 1.6f;
        [SerializeField] private float phase2AttackRadius = 2.0f;
        [SerializeField] private float phase1MoveSpeed = 1.8f;
        [SerializeField] private float phase1ChaseSpeed = 3.2f;
        [SerializeField] private float phase2MoveSpeed = 2.6f;
        [SerializeField] private float phase2ChaseSpeed = 5.0f;
        [SerializeField] private float dashSpeed = 9f;
        [SerializeField] private float dashDuration = 0.35f;
        [SerializeField] private float dashCooldown = 3.5f;
        [SerializeField] private float projectileCooldown = 2.4f;
        [SerializeField] private float projectileSpeed = 5.5f;
        [SerializeField] private int projectileDamage = 1;
        [SerializeField] private float projectileLife = 3f;
        [SerializeField] private string introLineKey = "teacher.intro";
        [SerializeField] private string phase2LineKey = "teacher.phase2";
        [SerializeField] private string defeatLineKey = "teacher.defeat";
        [SerializeField] private Color projectileColor = new Color(0.9f, 0.75f, 0.3f, 0.95f);

        public static event Action<float, float> BossHealthChanged;
        public static event Action BossPhaseChanged;
        public static event Action BossDefeated;

        public bool IsPhase2 => _phase2;
        public int Phase => _phase2 ? 2 : 1;
        public float MaxHealthValue => maxHealth;
        public float CurrentHealthValue => CurrentHealth;

        private bool _phase2;
        private bool _introShown;
        private bool _phase2Shown;
        private float _dashTimer;
        private float _dashRemaining;
        private float _projectileTimer;
        private Vector2 _dashDir;
        private SpriteRenderer _sr;

        protected override void Start()
        {
            base.Start();
            _sr = GetComponent<SpriteRenderer>();
            RaiseHealth();
        }

        protected override void Update()
        {
            base.Update();
            RaiseHealth();

            if (State == EnemyState.Dead)
                return;

            CheckPhase();

            if (_dashRemaining > 0f)
            {
                _dashRemaining -= Time.deltaTime;
                if (Rb != null)
                    Rb.linearVelocity = _dashDir * dashSpeed;
                if (_dashRemaining <= 0f && Rb != null)
                    Rb.linearVelocity = Vector2.zero;
            }

            if (_projectileTimer > 0f)
                _projectileTimer -= Time.deltaTime;
        }

        protected override void EnterState(EnemyState next)
        {
            base.EnterState(next);

            if (next == EnemyState.Chase && !_introShown)
            {
                _introShown = true;
                GameEvents.RaiseDialogLineShown(
                    Localization.Get("speaker.teacher"),
                    Localization.Get(introLineKey));
            }
        }

        protected override void Think()
        {
            float hp01 = maxHealth > 0 ? CurrentHealth / (float)maxHealth : 1f;
            attackRadius = _phase2 ? phase2AttackRadius : phase1AttackRadius;

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
                    else if (!HasLineToTarget(detectionRadius * 1.6f))
                        EnterState(EnemyState.Patrol);
                    break;

                case EnemyState.Attack:
                    if (!HasLineToTarget(attackRadius * 1.3f))
                        EnterState(EnemyState.Chase);
                    break;
            }

            _ = hp01;
        }

        protected override void Act()
        {
            float speedMove = _phase2 ? phase2MoveSpeed : phase1MoveSpeed;
            float speedChase = _phase2 ? phase2ChaseSpeed : phase1ChaseSpeed;

            if (_dashRemaining > 0f)
                return;

            switch (State)
            {
                case EnemyState.Patrol:
                    MoveTowards(Home + (Vector2)(Quaternion.Euler(0, 0, Time.time * 15f) * (Vector3)(Vector2.right * 2f)), speedMove * 0.5f);
                    break;

                case EnemyState.Chase:
                    if (Target != null)
                        MoveTowards(Target.position, speedChase);
                    if (_phase2 && _dashTimer <= 0f && HasLineToTarget(detectionRadius * 0.7f))
                        StartDash();
                    break;

                case EnemyState.Attack:
                    StopMoving();
                    TryAttackPlayer();
                    if (_phase2 && _projectileTimer <= 0f)
                        FireProjectile();
                    break;

                default:
                    StopMoving();
                    break;
            }

            if (_dashTimer > 0f)
                _dashTimer -= Time.deltaTime;
        }

        public override void TakeDamage(int amount, Vector2 hitDirection)
        {
            base.TakeDamage(amount, hitDirection);
            RaiseHealth();
            if (IsAlive)
                CheckPhase();
        }

        protected override void Die()
        {
            BossHealthChanged?.Invoke(0f, maxHealth);
            GameEvents.RaiseDialogLineShown(
                Localization.Get("speaker.teacher"),
                Localization.Get(defeatLineKey));
            BossDefeated?.Invoke();
            GameFlags.Set("boss.teacher.defeated");
            base.Die();
        }

        private void CheckPhase()
        {
            if (_phase2 || maxHealth <= 0)
                return;
            float hp01 = CurrentHealth / (float)maxHealth;
            if (hp01 > phase2Threshold)
                return;

            _phase2 = true;
            moveSpeed = phase2MoveSpeed;
            chaseSpeed = phase2ChaseSpeed;
            attackRadius = phase2AttackRadius;
            attackCooldown = Mathf.Max(0.6f, attackCooldown * 0.7f);
            if (_sr != null)
                _sr.color = new Color(1f, 0.85f, 0.8f, 1f);

            if (!_phase2Shown)
            {
                _phase2Shown = true;
                GameEvents.RaiseDialogLineShown(
                    Localization.Get("speaker.teacher"),
                    Localization.Get(phase2LineKey));
            }

            BossPhaseChanged?.Invoke();
            RaiseHealth();
        }

        private void StartDash()
        {
            if (Target == null)
                return;
            _dashTimer = dashCooldown;
            _dashRemaining = dashDuration;
            _dashDir = ((Vector2)Target.position - (Vector2)transform.position).normalized;
        }

        private void FireProjectile()
        {
            if (Target == null)
                return;
            _projectileTimer = projectileCooldown;

            var go = new GameObject("TeacherProjectile");
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakePixelSprite();
            sr.color = projectileColor;
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * 0.35f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearVelocity = ((Vector2)Target.position - (2f * (Vector2)transform.position)).normalized * projectileSpeed;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var proj = go.AddComponent<TeacherProjectile>();
            proj.Init(projectileDamage, projectileLife);
        }

        private void RaiseHealth() => BossHealthChanged?.Invoke(CurrentHealth, maxHealth);

        private static Sprite _pixelSprite;
        private static Sprite MakePixelSprite()
        {
            if (_pixelSprite != null)
                return _pixelSprite;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color32[16];
            for (int i = 0; i < 16; i++)
                px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply(false);
            tex.filterMode = FilterMode.Point;
            _pixelSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
            return _pixelSprite;
        }
    }

    public class TeacherProjectile : MonoBehaviour
    {
        private int _damage = 1;
        private float _life = 3f;

        public void Init(int damage, float life)
        {
            _damage = damage;
            _life = life;
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var dmg = other.GetComponent<Combat.IDamageable>();
                if (dmg != null)
                {
                    Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
                    dmg.TakeDamage(_damage, dir);
                }
                Destroy(gameObject);
            }
            else if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
