using System.Collections.Generic;
using UnityEngine;

namespace DefenderOfDreams.Core
{
    public static class Localization
    {
        private static readonly Dictionary<string, string> Table = new Dictionary<string, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Table.Count > 0)
                return;
            LoadDefaults();
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            return Table.TryGetValue(key, out var value) ? value : key;
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;
            Table[key] = value;
        }

        public static void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            var wrapper = JsonUtility.FromJson<LocalizationJson>(json);
            if (wrapper?.entries == null)
                return;
            foreach (var e in wrapper.entries)
                if (!string.IsNullOrEmpty(e.key))
                    Table[e.key] = e.value;
        }

        private static void LoadDefaults()
        {
            Set("hud.health", "Здоровье");
            Set("hud.anchors", "Якоря");
            Set("hud.resource", "Сила");
            Set("ability.memory", "Воспоминание");
            Set("ability.echo", "Эхо");
            Set("ability.sleep", "Сон");
            Set("ability.forget", "Забыть");
            Set("death.player", "Неро растворяется в Изнанке...");
        }

        [System.Serializable]
        private class LocalizationJson
        {
            public List<Entry> entries = new List<Entry>();
        }

        [System.Serializable]
        private class Entry
        {
            public string key;
            public string value;
        }
    }
}
