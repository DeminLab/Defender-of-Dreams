using System.Text;
using DefenderOfDreams.Core;
using DefenderOfDreams.Memories;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DefenderOfDreams.UI
{
    public class MemoryLogUI : MonoBehaviour
    {
        [Header("4.10.3 — Memory journal")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text listText;
        [SerializeField] private Text emptyText;
        [SerializeField] private Text percentText;

        private bool _open;

        private void OnEnable()
        {
            MemoryLog.Collected += OnCollected;
            MemoryLog.Cleared += Refresh;
            MemoryLog.Rebuilt += Refresh;
        }

        private void OnDisable()
        {
            MemoryLog.Collected -= OnCollected;
            MemoryLog.Cleared -= Refresh;
            MemoryLog.Rebuilt -= Refresh;
        }

        private void Start()
        {
            if (titleText != null)
                titleText.text = Localization.Get("journal.title");
            if (emptyText != null)
                emptyText.text = Localization.Get("journal.empty");
            SetOpen(false);
            Refresh();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;
            if (kb.tabKey.wasPressedThisFrame || kb.mKey.wasPressedThisFrame)
                Toggle();
        }

        public void Toggle() => SetOpen(!_open);

        public void SetOpen(bool open)
        {
            _open = open;
            if (panel != null)
                panel.SetActive(open);
            if (open)
                Refresh();
        }

        private void OnCollected(MemoryEntry _) => Refresh();

        private void Refresh()
        {
            var all = MemoryLog.All;
            bool empty = all == null || all.Count == 0;

            if (emptyText != null)
                emptyText.gameObject.SetActive(empty);
            if (listText != null)
                listText.gameObject.SetActive(!empty);

            if (percentText != null)
            {
                int total = 3;
                int have = all != null ? all.Count : 0;
                percentText.text = $"{Localization.Get("journal.found")} {have}/{total} ({(total > 0 ? have * 100 / total : 0)}%)";
            }

            if (empty || listText == null)
                return;

            var sb = new StringBuilder();
            foreach (var e in all)
            {
                sb.AppendLine($"<b>{e.title}</b>");
                sb.AppendLine(e.text);
                sb.AppendLine();
            }
            listText.text = sb.ToString().TrimEnd();
        }
    }
}
