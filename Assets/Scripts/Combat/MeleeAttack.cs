using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [Header("Blade of Soul (4.3.2)")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float range = 1.6f;
        [SerializeField] private float arcDegrees = 100f;
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private LayerMask hittableLayers = ~0;
        [SerializeField] private Transform aimPivot;

        private float _cooldownTimer;
        private Vector2 _lastAim = Vector2.right;

        public bool InputLocked { get; set; }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
            if (InputLocked)
                return;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && _cooldownTimer <= 0f)
            {
                TryAttack();
            }
        }

        public void SetAim(Vector2 worldPoint)
        {
            Vector2 dir = ((Vector2)transform.position - worldPoint).normalized;
            if (dir.sqrMagnitude > 0.001f)
                _lastAim = -dir;
        }

        private void TryAttack()
        {
            _cooldownTimer = cooldown;
            Vector2 origin = transform.position;
            Vector2 aim = ResolveAimDirection();

            if (aimPivot != null)
            {
                float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, hittableLayers);
            foreach (var hit in hits)
            {
                if (hit.transform == transform)
                    continue;
                Vector2 toTarget = (Vector2)hit.transform.position - origin;
                if (toTarget.sqrMagnitude < 0.0001f)
                    continue;
                float angle = Vector2.Angle(aim, toTarget.normalized);
                if (angle > arcDegrees * 0.5f)
                    continue;
                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                    damageable.TakeDamage(damage, toTarget.normalized);
            }
        }

        private Vector2 ResolveAimDirection()
        {
            var mouse = Mouse.current;
            var cam = Camera.main;
            if (mouse != null && cam != null)
            {
                Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
                Vector2 dir = ((Vector2)world - (Vector2)transform.position).normalized;
                if (dir.sqrMagnitude > 0.001f)
                {
                    _lastAim = dir;
                    return dir;
                }
            }

            return _lastAim;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
