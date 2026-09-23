using UnityEngine;

namespace DefenderOfDreams.Visuals
{
    public class UnitBob : MonoBehaviour
    {
        public bool isPlayer;

        private Vector3 _baseScale;
        private float _phase;
        private float _squash;

        private void Start()
        {
            _baseScale = transform.localScale;
            _phase = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            if (isPlayer)
            {
                _squash = Mathf.Lerp(_squash, 0f, Time.deltaTime * 8f);
            }
            else
            {
                float chase = 0f;
                var rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                    chase = Mathf.Clamp01(rb.linearVelocity.magnitude / 4f);
                _squash = Mathf.Lerp(_squash, chase * 0.18f, Time.deltaTime * 6f);
            }

            float bob = Mathf.Sin(Time.time * 4f + _phase) * 0.04f;
            transform.localScale = new Vector3(
                _baseScale.x * (1f + _squash),
                _baseScale.y * (1f - _squash * 0.6f) + bob,
                _baseScale.z * (1f + _squash * 0.5f));
        }

        public void Punch()
        {
            _squash = 0.35f;
        }
    }
}
