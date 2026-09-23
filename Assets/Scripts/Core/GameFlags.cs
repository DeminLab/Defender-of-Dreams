using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefenderOfDreams.Core
{
    public static class GameFlags
    {
        private static readonly HashSet<string> Flags = new();
        private static readonly HashSet<string> Memories = new();
        private static readonly HashSet<int> MemoryIds = new();

        public static IReadOnlyCollection<string> AllFlags => Flags;

        public static bool Has(string flagId) =>
            !string.IsNullOrEmpty(flagId) && Flags.Contains(flagId);

        public static void Set(string flagId, bool value = true)
        {
            if (string.IsNullOrEmpty(flagId))
                return;
            if (value)
            {
                if (Flags.Add(flagId))
                    GameEvents.RaiseFlagSet(flagId);
            }
            else
            {
                Flags.Remove(flagId);
            }
        }

        public static void Clear(string flagId)
        {
            if (!string.IsNullOrEmpty(flagId))
                Flags.Remove(flagId);
        }

        public static bool HasMemory(string memoryId) =>
            !string.IsNullOrEmpty(memoryId) && Memories.Contains(memoryId);

        public static bool CollectMemory(string memoryId)
        {
            if (string.IsNullOrEmpty(memoryId) || !Memories.Add(memoryId))
                return false;
            GameEvents.RaiseMemoryCollected(memoryId);
            return true;
        }

        public static List<string> GetExport() => new(Flags);

        public static void SetImport(List<string> flags)
        {
            Flags.Clear();
            if (flags == null)
                return;
            foreach (var f in flags)
                if (!string.IsNullOrEmpty(f))
                    Flags.Add(f);
        }

        public static List<int> GetCollectedMemoryIds() => new(MemoryIds);

        public static void SetCollectedMemories(List<int> ids)
        {
            MemoryIds.Clear();
            if (ids == null)
                return;
            foreach (var id in ids)
                MemoryIds.Add(id);
        }

        public static float MemoriesPercent(int total)
        {
            if (total <= 0)
                return 0f;
            return Mathf.Min(1f, Memories.Count / (float)total);
        }
    }
}
