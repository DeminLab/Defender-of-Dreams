using DefenderOfDreams.Combat;
using DefenderOfDreams.FogOfWar;
using DefenderOfDreams.Memories;
using DefenderOfDreams.Save;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Core
{
    public class SaveService : MonoBehaviour
    {
        [SerializeField, Min(5f)] private float autosaveInterval = 60f;
        [SerializeField] private Transform player;
        [SerializeField] private Health playerHealth;

        private float _timer;
        public static SaveService Instance { get; private set; }

        public bool HasSave => SaveSystem.Exists();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;

            if (_timer >= autosaveInterval)
            {
                _timer = 0f;
                Save();
            }

            var kb = Keyboard.current;
            if (kb == null)
                return;

            if (kb.f5Key.wasPressedThisFrame)
                Save();
            else if (kb.f9Key.wasPressedThisFrame)
                Load();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                Save();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                Save();
        }

        public void Save()
        {
            var data = new SaveData();

            if (player != null)
            {
                data.playerX = player.position.x;
                data.playerY = player.position.y;
            }

            if (playerHealth != null)
            {
                data.playerHealth = playerHealth.Current;
                data.playerMaxHealth = playerHealth.Max;
            }

            data.storyFlags = GameFlags.GetExport();
            MemoryLog.Export(data.memoryIds, data.memoryTitles, data.memoryTexts);

            var fog = FogOfWarSystem.Instance;
            if (fog != null)
                fog.WriteToSave(data);

            SaveSystem.Write(data);
            GameEvents.RaiseGameSaved();
        }

        public bool Load()
        {
            var data = SaveSystem.Read();
            if (data == null)
                return false;

            if (player != null)
                player.position = new Vector3(data.playerX, data.playerY, player.position.z);

            if (playerHealth != null)
            {
                int savedMax = Mathf.Max(1, data.playerMaxHealth);
                playerHealth.Configure(savedMax, true);
                playerHealth.ForceSetHealth(Mathf.Clamp(data.playerHealth, 1, savedMax));
            }

            GameFlags.SetImport(data.storyFlags);
            MemoryLog.Rebuild(data.memoryIds, data.memoryTitles, data.memoryTexts);

            var fog = FogOfWarSystem.Instance;
            if (fog != null)
                fog.ReadFromSave(data);

            _timer = 0f;
            GameEvents.RaiseGameLoaded();
            return true;
        }
    }
}
