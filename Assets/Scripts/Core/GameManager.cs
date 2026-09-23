using UnityEngine;

namespace DefenderOfDreams.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Autosave")]
        [SerializeField, Min(5f)] private float autosaveInterval = 60f;

        public bool IsPaused { get; private set; }

        private float _autosaveTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (SaveService.Instance != null && SaveService.Instance.HasSave)
                SaveService.Instance.Load();

            GameEvents.RaiseGameLoaded();
        }

        private void Update()
        {
            if (IsPaused)
                return;

            // SaveService is the single owner of autosaving.
            // Keeping the timer here previously caused duplicate writes.
        }

        public void SaveGame() => SaveService.Instance?.Save();

        public void LoadGame() => SaveService.Instance?.Load();

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void TogglePause() => SetPaused(!IsPaused);

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
