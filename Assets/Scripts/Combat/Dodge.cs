using System.Collections;
using UnityEngine;

namespace DefenderOfDreams.Combat
{
    public class Dodge : MonoBehaviour
    {
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float cooldown = 0.7f;
        [SerializeField] private Health health;
        [SerializeField] private DefenderOfDreams.Player.PlayerController controller;

        private float _cooldownTimer;
        private bool _dashing;
        private Vector2 _lastMove = Vector2.right;
        private Rigidbody2D _rb;

        public bool InputLocked { get; set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
            if (InputLocked || _dashing)
                return;

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null)
                return;

            if (controller != null && controller.Velocity.sqrMagnitude > 0.01f)
                _lastMove = controller.Velocity.normalized;

            if (kb.spaceKey.wasPressedThisFrame && _cooldownTimer <= 0f)
                StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            _dashing = true;
            _cooldownTimer = cooldown;
            if (controller != null)
                controller.InputLocked = true;
            if (health != null)
                health.SetInvulnerability(dashDuration + 0.05f);

            float t = 0f;
            while (t < dashDuration)
            {
                t += Time.deltaTime;
                if (_rb != null)
                    _rb.linearVelocity = _lastMove * dashSpeed;
                yield return null;
            }

            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
            if (controller != null)
                controller.InputLocked = false;
            _dashing = false;
        }
    }
}
