using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefenderOfDreams.Save
{
    [Serializable]
    public class SaveData
    {
        public int schemaVersion = 1;
        public float playerX;
        public float playerY;
        public int playerHealth;
        public int playerMaxHealth;
        public List<string> storyFlags = new();
        public List<int> memoryIds = new();
        public List<string> memoryTitles = new();
        public List<string> memoryTexts = new();
        public List<int> fogStates = new();
        public List<float> fogTimers = new();
        public int fogWidth;
        public int fogHeight;
        public int anchorsPlaced;
        public long savedAtUnix;
    }

    public static class SaveSystem
    {
        private const string FallbackKey = "dod_save_v1";

        public static string FilePath =>
            System.IO.Path.Combine(Application.persistentDataPath, "dream_keeper_save.json");

        public static bool Exists()
        {
            try
            {
                if (System.IO.File.Exists(FilePath))
                    return true;
            }
            catch (Exception)
            {
            }

            return PlayerPrefs.HasKey(FallbackKey);
        }

        public static void Write(SaveData data)
        {
            data.savedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string json = JsonUtility.ToJson(data, true);
            try
            {
                System.IO.File.WriteAllText(FilePath, json);
            }
            catch (Exception)
            {
                PlayerPrefs.SetString(FallbackKey, json);
                PlayerPrefs.Save();
            }
        }

        public static SaveData Read()
        {
            string json = null;
            try
            {
                if (System.IO.File.Exists(FilePath))
                    json = System.IO.File.ReadAllText(FilePath);
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(json))
                json = PlayerPrefs.GetString(FallbackKey, "");

            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data != null && data.schemaVersion <= 1)
                    return data;
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static void Delete()
        {
            try
            {
                if (System.IO.File.Exists(FilePath))
                    System.IO.File.Delete(FilePath);
            }
            catch (Exception)
            {
            }

            PlayerPrefs.DeleteKey(FallbackKey);
            PlayerPrefs.Save();
        }
    }
}
