using System;
using UnityEngine;

namespace DefenderOfDreams.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> HealthChanged;
        public static event Action<int> AnchorsChanged;
        public static event Action<int, int> AbilityResourceChanged;
        public static event Action<string> FlagSet;
        public static event Action<string> MemoryCollected;
        public static event Action<int, string> MemoryCollectedDetailed;
        public static event Action<float> ActiveZoneForgettingChanged;
        public static event Action<string, float, float> ZoneForgetDetailed;
        public static event Action PlayerDied;
        public static event Action GameSaved;
        public static event Action GameLoaded;
        public static event Action<string> DialogLineShown;
        public static event Action<string, string> DialogLineDetailed;

        public static void RaiseHealthChanged(int current, int max) => HealthChanged?.Invoke(current, max);
        public static void RaiseAnchorsChanged(int available) => AnchorsChanged?.Invoke(available);
        public static void RaiseAbilityResourceChanged(int current, int max) => AbilityResourceChanged?.Invoke(current, max);
        public static void RaiseFlagSet(string flagId) => FlagSet?.Invoke(flagId);

        public static void RaiseMemoryCollected(string memoryId) => MemoryCollected?.Invoke(memoryId);

        public static void RaiseMemoryCollected(int memoryId, string title)
        {
            MemoryCollected?.Invoke(memoryId.ToString());
            MemoryCollectedDetailed?.Invoke(memoryId, title);
        }

        public static void RaiseActiveZoneForgettingChanged(float normalized) =>
            ActiveZoneForgettingChanged?.Invoke(normalized);

        public static void RaiseActiveZoneForgettingChanged(string zoneId, float value, float max)
        {
            ZoneForgetDetailed?.Invoke(zoneId, value, max);
            ActiveZoneForgettingChanged?.Invoke(max > 0f ? Mathf.Clamp01(value / max) : 0f);
        }

        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaiseGameSaved() => GameSaved?.Invoke();
        public static void RaiseGameLoaded() => GameLoaded?.Invoke();
        public static void RaiseDialogLineShown(string line) => DialogLineShown?.Invoke(line);

        public static void RaiseDialogLineShown(string speaker, string text)
        {
            DialogLineShown?.Invoke(text);
            DialogLineDetailed?.Invoke(speaker, text);
        }
    }
}
