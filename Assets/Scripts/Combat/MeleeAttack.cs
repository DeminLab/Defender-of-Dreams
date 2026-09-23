using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [Header("Blade of Soul")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float range = 1.6f;
        [SerializeField, Range(10f, 180f)] private float arcDegrees = 100f;
        [SerializeField, Min(0.05f)] private float cooldown = 0.35f;
        [SerializeField] private LayerMask hittableLayers = ~0;
        [SerializeField] private Transform aimPivot;

        private float _cooldownTimer;
        private Vector2 _lastAim = Vector2.right;
        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

        public bool InputLocked { get; set; }
        public bool IsAttacking { get; private set; }
        public event Action<Vector2> AttackStarted;

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            if (InputLocked || _cooldownTimer > 0f)
                return;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                TryAttack();
        }

        public void SetAim(Vector2 worldPoint)
        {
            Vector2 dir = (worldPoint - (Vector2)transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
                _lastAim = dir;
        }

        public bool TryAttack()
        {
            if (InputLocked || _cooldownTimer > 0f)
                return false;

            _cooldownTimer = cooldown;
            _hitThisSwing.Clear();
            IsAttacking = true;

            Vector2 aim = ResolveAimDirection();

            if (aimPivot != null)
            {
                float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            AttackStarted?.Invoke(aim);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                range,
                hittableLayers);

            foreach (var hit in hits)
            {
                if (hit == null || hit.transform == transform)
                    continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive || !_hitThisSwing.Add(damageable))
                    continue;

                Vector2 toTarget = (Vector2)hit.transform.position - (Vector2)transform.position;
                if (toTarget.sqrMagnitude < 0.0001f)
                    continue;

                if (Vector2.Angle(aim, toTarget.normalized) <= arcDegrees * 0.5f)
                    damageable.TakeDamage(damage, toTarget.normalized);
            }

            IsAttacking = false;
            return true;
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

            return _lastAim.sqrMagnitude > 0.001f
                ? _lastAim.normalized
                : Vector2.right;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
