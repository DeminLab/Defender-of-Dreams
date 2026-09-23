using DefenderOfDreams.Audio;
using DefenderOfDreams.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DefenderOfDreams.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("4.10.2 — Pause menu")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Settings bindings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Toggle ditherToggle;
        [SerializeField] private Toggle directionVisualToggle;
        [SerializeField] private Text statusText;

        private void OnEnable()
        {
            GameEvents.GameSaved += OnSaved;
            GameEvents.GameLoaded += OnLoaded;
        }

        private void OnDisable()
        {
            GameEvents.GameSaved -= OnSaved;
            GameEvents.GameLoaded -= OnLoaded;
        }

        private void Start()
        {
            BindSettings();
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                var gm = GameManager.Instance;
                if (gm == null)
                    return;
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                    return;
                }
                gm.TogglePause();
                if (pausePanel != null)
                    pausePanel.SetActive(gm.IsPaused);
            }
        }

        public void OnSaveClicked()
        {
            GameManager.Instance?.SaveGame();
            SetStatus(Localization.Get("pause.saved"));
        }

        public void OnLoadClicked()
        {
            if (SaveService.Instance != null && SaveService.Instance.HasSave)
            {
                SaveService.Instance.Load();
                SetStatus(Localization.Get("pause.loaded"));
            }
            else
            {
                SetStatus(Localization.Get("pause.nosave"));
            }
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void OnResumeClicked()
        {
            GameManager.Instance?.SetPaused(false);
            if (pausePanel != null)
                pausePanel.SetActive(false);
            CloseSettings();
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void BindSettings()
        {
            var audio = AudioService.Instance;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(audio != null ? audio.MasterVolume : 1f);
                masterVolumeSlider.onValueChanged.AddListener(v =>
                {
                    if (AudioService.Instance != null)
                        AudioService.Instance.SetMasterVolume(v);
                });
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(audio != null ? audio.SfxVolume : 1f);
                sfxVolumeSlider.onValueChanged.AddListener(v =>
                {
                    if (AudioService.Instance != null)
                        AudioService.Instance.SetSfxVolume(v);
                });
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(audio != null ? audio.MusicVolume : 0.7f);
                musicVolumeSlider.onValueChanged.AddListener(v =>
                {
                    if (AudioService.Instance != null)
                        AudioService.Instance.SetMusicVolume(v);
                });
            }

            if (directionVisualToggle != null)
            {
                directionVisualToggle.SetIsOnWithoutNotify(audio == null || audio.DirectionVisualEnabled);
                directionVisualToggle.onValueChanged.AddListener(v =>
                {
                    if (AudioService.Instance != null)
                        AudioService.Instance.SetDirectionVisual(v);
                });
            }

            if (ditherToggle != null)
            {
                int dither = PlayerPrefs.GetInt("fog.dither", 1);
                ditherToggle.SetIsOnWithoutNotify(dither == 1);
                ditherToggle.onValueChanged.AddListener(v =>
                {
                    PlayerPrefs.SetInt("fog.dither", v ? 1 : 0);
                    PlayerPrefs.Save();
                    var fog = FogOfWar.FogOfWarSystem.Instance;
                    if (fog != null)
                    {
                        var s = fog.Settings;
                        s.ditherEnabled = v;
                        s.simplifiedFallback = !v;
                        fog.Configure(s, fog.AnchorCapacity);
                    }
                });
            }

            EnsureEventSystem();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void OnSaved() => SetStatus(Localization.Get("pause.saved"));
        private void OnLoaded() => SetStatus(Localization.Get("pause.loaded"));

        private void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }
    }
}
