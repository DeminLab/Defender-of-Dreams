using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement (4.2.2)")]
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float deceleration = 30f;

        private Rigidbody2D _rb;
        private Vector2 _input;
        private Vector2 _velocity;

        public Vector2 Velocity => _velocity;
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

            var kb = Keyboard.current;
            if (kb == null)
            {
                _input = Vector2.zero;
                return;
            }

            float x = 0f;
            float y = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                var stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.15f)
                {
                    x = stick.x;
                    y = stick.y;
                }
            }

            _input = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }

        private void FixedUpdate()
        {
            Vector2 desired = _input * moveSpeed;
            float rate = _input.sqrMagnitude > 0.01f ? acceleration : deceleration;
            _velocity = Vector2.MoveTowards(_velocity, desired, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = _velocity;
        }
    }
}
