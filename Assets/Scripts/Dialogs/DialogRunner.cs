using System;
using DefenderOfDreams.Core;
using DefenderOfDreams.Memories;
using DefenderOfDreams.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Dialogs
{
    public class DialogRunner : MonoBehaviour
    {
        [SerializeField] private DialogAsset activeDialog;
        [SerializeField] private int lineIndex = -1;
        [SerializeField] private PlayerController player;
        [SerializeField] private MonoBehaviour[] disableWhileTalking;

        public static DialogRunner Instance { get; private set; }
        public bool IsActive => activeDialog != null && lineIndex >= 0 && lineIndex < activeDialog.lines.Count;
        public string CurrentSpeaker { get; private set; } = "";
        public string CurrentText { get; private set; } = "";
        public event Action LineChanged;
        public event Action DialogEnded;

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
            if (!IsActive)
                return;

            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                Advance();
        }

        public bool CanStart(DialogAsset dialog)
        {
            if (dialog == null)
                return false;
            if (dialog.once && GameFlags.Has(dialog.dialogId))
                return false;
            return true;
        }

        public void StartDialog(DialogAsset dialog)
        {
            if (!CanStart(dialog))
                return;
            activeDialog = dialog;
            lineIndex = -1;
            SetPlayerControl(false);
            Advance();
        }

        public void Advance()
        {
            if (activeDialog == null)
                return;

            while (true)
            {
                lineIndex++;
                if (lineIndex >= activeDialog.lines.Count)
                {
                    EndDialog();
                    return;
                }

                var line = activeDialog.lines[lineIndex];
                if (!string.IsNullOrEmpty(line.requiredFlag) && !GameFlags.Has(line.requiredFlag))
                    continue;
                if (line.requiredMemoryId >= 0 && !MemoryLog.Has(line.requiredMemoryId))
                    continue;
                if (!string.IsNullOrEmpty(line.setFlag))
                    GameFlags.Set(line.setFlag, true);

                CurrentSpeaker = line.speaker;
                CurrentText = line.text;
                GameEvents.RaiseDialogLineShown(CurrentSpeaker, CurrentText);
                LineChanged?.Invoke();
                return;
            }
        }

        private void EndDialog()
        {
            if (activeDialog != null)
            {
                GameFlags.Set(activeDialog.dialogId, true);
                if (!string.IsNullOrEmpty(activeDialog.completionFlag))
                    GameFlags.Set(activeDialog.completionFlag, true);
            }

            activeDialog = null;
            lineIndex = -1;
            CurrentSpeaker = "";
            CurrentText = "";
            SetPlayerControl(true);
            DialogEnded?.Invoke();
        }

        private void SetPlayerControl(bool enabled)
        {
            if (player != null)
                player.InputLocked = !enabled;
            if (disableWhileTalking == null)
                return;
            foreach (var mb in disableWhileTalking)
            {
                if (mb != null)
                    mb.enabled = enabled;
            }
        }
    }
}
