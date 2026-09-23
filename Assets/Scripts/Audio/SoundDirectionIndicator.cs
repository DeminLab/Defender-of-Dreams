using UnityEngine;
using UnityEngine.UI;

namespace DefenderOfDreams.Audio
{
    public class SoundDirectionIndicator : MonoBehaviour
    {
        [Header("4.9.3 — visual sound direction")]
        [SerializeField] private RectTransform arrow;
        [SerializeField] private CanvasGroup arrowGroup;
        [SerializeField] private float maxOffset = 120f;
        [SerializeField] private float fadeSpeed = 3f;

        private float _timer;
        private float _duration;
        private Vector2 _dir;

        private void OnEnable()
        {
            AudioService.DirectionalCue += OnCue;
            if (arrowGroup != null)
                arrowGroup.alpha = 0f;
        }

        private void OnDisable()
        {
            AudioService.DirectionalCue -= OnCue;
        }

        private void OnCue(Vector2 worldPos, float pan, float duration)
        {
            var cam = Camera.main;
            if (cam == null)
                return;
            Vector2 source = worldPos;
            Vector2 self = cam.transform.position;
            _dir = (source - self).normalized;
            if (_dir.sqrMagnitude < 0.001f)
                _dir = Vector2.right;
            _duration = duration;
            _timer = duration;
        }

        private void Update()
        {
            if (_timer <= 0f)
            {
                if (arrowGroup != null && arrowGroup.alpha > 0f)
                    arrowGroup.alpha = Mathf.Max(0f, arrowGroup.alpha - fadeSpeed * Time.unscaledDeltaTime);
                return;
            }

            _timer -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_timer / Mathf.Max(0.01f, _duration));

            if (arrow != null)
            {
                arrow.anchoredPosition = _dir * maxOffset;
                float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
                arrow.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }

            if (arrowGroup != null)
                arrowGroup.alpha = Mathf.Lerp(0.4f, 1f, t);
        }
    }
}
