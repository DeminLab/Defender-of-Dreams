using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 5.5f;
        [SerializeField, Min(0.1f)] private float acceleration = 40f;
        [SerializeField, Min(0.1f)] private float deceleration = 30f;
        [SerializeField, Range(0f, 0.5f)] private float gamepadDeadzone = 0.15f;

        private Rigidbody2D _rb;
        private Vector2 _input;
        private Vector2 _velocity;
        private Vector2 _lastMoveDirection = Vector2.down;

        public Vector2 Velocity => _velocity;
        public Vector2 InputDirection => _input;
        public Vector2 LastMoveDirection => _lastMoveDirection;
        public bool IsMoving => _velocity.sqrMagnitude > 0.01f;
        public bool InputLocked { get; set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            if (InputLocked)
            {
                _input = Vector2.zero;
                return;
            }

            Vector2 input = Vector2.zero;

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.magnitude >= gamepadDeadzone)
                    input = Vector2.ClampMagnitude(stick, 1f);
            }

            if (input.sqrMagnitude < 0.0001f)
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    float x = 0f;
                    float y = 0f;

                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;

                    input = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
                }
            }

            _input = input;

            if (_input.sqrMagnitude > 0.01f)
                _lastMoveDirection = _input.normalized;
        }

        private void FixedUpdate()
        {
            Vector2 desired = _input * moveSpeed;
            float rate = _input.sqrMagnitude > 0.01f ? acceleration : deceleration;

            _velocity = Vector2.MoveTowards(
                _velocity,
                desired,
                rate * Time.fixedDeltaTime);

            _rb.linearVelocity = _velocity;
        }

        public void StopImmediately()
        {
            _input = Vector2.zero;
            _velocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
        }
    }
}
