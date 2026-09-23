using DefenderOfDreams.Player;
using UnityEngine;

namespace DefenderOfDreams.Characters
{
    /// <summary>
    /// Lightweight companion controller for Lia.
    /// It intentionally has no combat dependency so narrative and combat
    /// systems can be connected later without coupling the character.
    /// </summary>
    public class LiaCompanion : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followDistance = 2.2f;
        [SerializeField] private float stopDistance = 1.4f;
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float acceleration = 10f;

        private Vector2 _velocity;

        public bool IsFollowing { get; private set; } = true;

        private void Start()
        {
            if (target == null)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                    target = player.transform;
            }
        }

        private void Update()
        {
            if (!IsFollowing || target == null)
                return;

            Vector2 targetPos = target.position;
            Vector2 offset = targetPos - (Vector2)transform.position;
            float distance = offset.magnitude;

            if (distance <= stopDistance)
            {
                _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, acceleration * Time.deltaTime);
            }
            else
            {
                Vector2 desired = offset.normalized * moveSpeed;
                if (distance > followDistance)
                    desired *= Mathf.Clamp01((distance - followDistance) / followDistance + 0.5f);

                _velocity = Vector2.MoveTowards(
                    _velocity,
                    desired,
                    acceleration * Time.deltaTime);
            }

            transform.position += (Vector3)(_velocity * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
        public void SetFollowing(bool following)
        {
            IsFollowing = following;
            if (!following)
                _velocity = Vector2.zero;
        }
    }
}
