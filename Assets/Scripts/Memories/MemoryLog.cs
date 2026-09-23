using System;
using System.Collections.Generic;
using DefenderOfDreams.Core;
using UnityEngine;

namespace DefenderOfDreams.Memories
{
    [Serializable]
    public class MemoryEntry
    {
        public int id;
        public string title;
        [TextArea]
        public string text;
    }

    public static class MemoryLog
    {
        private static readonly List<MemoryEntry> Entries = new();
        public static IReadOnlyList<MemoryEntry> All => Entries;
        public static event Action<MemoryEntry> Collected;
        public static event Action Cleared;

        public static bool Has(int id) => Entries.Exists(e => e.id == id);

        public static MemoryEntry Get(int id) => Entries.Find(e => e.id == id);

        public static bool Collect(int id, string title, string text)
        {
            if (Has(id))
                return false;
            var entry = new MemoryEntry { id = id, title = title, text = text };
            Entries.Add(entry);
            GameFlags.CollectMemory(id.ToString());
            GameEvents.RaiseMemoryCollected(id, title);
            Collected?.Invoke(entry);
            return true;
        }

        public static void Clear()
        {
            Entries.Clear();
            Cleared?.Invoke();
        }

        public static void Rebuild(List<int> ids, List<string> titles, List<string> texts)
        {
            Entries.Clear();
            if (ids != null)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    Entries.Add(new MemoryEntry
                    {
                        id = ids[i],
                        title = titles != null && i < titles.Count ? titles[i] : $"Memory {ids[i]}",
                        text = texts != null && i < texts.Count ? texts[i] : ""
                    });
                }
            }

            Cleared?.Invoke();
        }

        public static void Export(List<int> ids, List<string> titles, List<string> texts)
        {
            ids.Clear();
            titles.Clear();
            texts.Clear();
            foreach (var e in Entries)
            {
                ids.Add(e.id);
                titles.Add(e.title);
                texts.Add(e.text);
            }
        }
    }
}
