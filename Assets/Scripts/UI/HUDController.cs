using UnityEngine;
using UnityEngine.UI;

namespace DefenderOfDreams.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health (4.10.1)")]
        [SerializeField] private Image healthFill;

        [Header("Ability resource")]
        [SerializeField] private Text abilityResourceText;

        [Header("Anchors")]
        [SerializeField] private Text anchorsText;

        [Header("Forgetting timer")]
        [SerializeField] private Image forgetTimerFill;

        [Header("Dialog line")]
        [SerializeField] private Text dialogText;
        [SerializeField] private GameObject dialogPanel;

        [Header("Pause")]
        [SerializeField] private GameObject pausePanel;

        private void Start()
        {
            if (pausePanel != null && Core.GameManager.Instance != null)
                pausePanel.SetActive(Core.GameManager.Instance.IsPaused);
        }

        private void OnEnable()
        {
            Core.GameEvents.HealthChanged += OnHealth;
            Core.GameEvents.AnchorsChanged += OnAnchors;
            Core.GameEvents.AbilityResourceChanged += OnResource;
            Core.GameEvents.ActiveZoneForgettingChanged += OnForget;
            Core.GameEvents.DialogLineShown += OnDialog;
            Core.GameEvents.PlayerDied += OnDeath;
        }

        private void OnDisable()
        {
            Core.GameEvents.HealthChanged -= OnHealth;
            Core.GameEvents.AnchorsChanged -= OnAnchors;
            Core.GameEvents.AbilityResourceChanged -= OnResource;
            Core.GameEvents.ActiveZoneForgettingChanged -= OnForget;
            Core.GameEvents.DialogLineShown -= OnDialog;
            Core.GameEvents.PlayerDied -= OnDeath;
        }

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null)
                return;

            if (kb.escapeKey.wasPressedThisFrame)
                TogglePause();

            if (dialogPanel != null && dialogPanel.activeSelf
                && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                && Dialogs.DialogRunner.Instance != null)
            {
                Dialogs.DialogRunner.Instance.Advance();
            }

            if (Dialogs.DialogRunner.Instance != null && !Dialogs.DialogRunner.Instance.IsActive
                && dialogPanel != null && dialogPanel.activeSelf)
            {
                dialogPanel.SetActive(false);
            }
        }

        private void TogglePause()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null)
                return;
            gm.TogglePause();
            if (pausePanel != null)
                pausePanel.SetActive(gm.IsPaused);
        }

        private void OnHealth(int current, int max)
        {
            if (healthFill != null)
                healthFill.fillAmount = max > 0 ? current / (float)max : 0f;
        }

        private void OnAnchors(int available)
        {
            if (anchorsText == null)
                return;
            var fog = FogOfWar.FogOfWarSystem.Instance;
            int cap = fog != null ? fog.AnchorCapacity : 3;
            anchorsText.text = $"{available}/{cap}";
        }

        private void OnResource(int current, int max)
        {
            if (abilityResourceText != null)
                abilityResourceText.text = $"{current}/{max}";
        }

        private void OnForget(float normalized)
        {
            if (forgetTimerFill != null)
                forgetTimerFill.fillAmount = normalized;
        }

        private void OnDialog(string line)
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(true);
            if (dialogText != null)
                dialogText.text = line;
        }

        private void OnDeath()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(true);
            if (dialogText != null)
                dialogText.text = "Неро растворяется в Изнанке...";
        }
    }
}
