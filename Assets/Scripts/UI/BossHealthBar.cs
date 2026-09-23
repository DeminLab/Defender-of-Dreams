using DefenderOfDreams.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace DefenderOfDreams.UI
{
    public class BossHealthBar : MonoBehaviour
    {
        [Header("Boss bar (4.7)")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image fill;
        [SerializeField] private Text nameText;
        [SerializeField] private Text phaseText;

        private void OnEnable()
        {
            TeacherBoss.BossHealthChanged += OnHealth;
            TeacherBoss.BossPhaseChanged += OnPhase;
            TeacherBoss.BossDefeated += OnDefeated;
        }

        private void OnDisable()
        {
            TeacherBoss.BossHealthChanged -= OnHealth;
            TeacherBoss.BossPhaseChanged -= OnPhase;
            TeacherBoss.BossDefeated -= OnDefeated;
        }

        private void Start()
        {
            if (nameText != null)
                nameText.text = Core.Localization.Get("boss.teacher");
            if (root != null)
                root.SetActive(false);
            if (phaseText != null)
                phaseText.text = "I";
        }

        private void OnHealth(float current, float max)
        {
            if (root != null && !root.activeSelf && current > 0f)
                root.SetActive(true);
            if (fill != null)
                fill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        }

        private void OnPhase()
        {
            if (phaseText != null)
                phaseText.text = "II";
        }

        private void OnDefeated()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
