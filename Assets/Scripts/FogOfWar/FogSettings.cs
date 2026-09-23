using System;
using UnityEngine;

namespace DefenderOfDreams.FogOfWar
{
    public enum FogCellState : byte
    {
        Hidden = 0,
        Revealed = 1,
        Permanent = 2,
        LostForever = 3
    }

    [Serializable]
    public class FogSettings
    {
        [Header("Field of Awareness (4.1.1)")]
        [Tooltip("Base reveal radius in world units")]
        public float awarenessRadius = 8f;

        [Header("Forgetting (4.1.4)")]
        [Tooltip("Seconds a revealed area stays visible before re-fogging")]
        public float forgetTimer = 30f;

        [Header("Grid")]
        [Tooltip("World units per fog cell")]
        public float cellSize = 0.5f;

        [Header("Visual (4.1.2 / 4.1.3)")]
        [Range(0f, 1f)] public float fogMaxOpacity = 0.97f;
        public float revealBlendSpeed = 4f;
        public float ditherIntensity = 0.35f;
        public float ditherSpeed = 2f;
        public Color fogColor = new Color(0.03f, 0.03f, 0.05f, 1f);
        public Color fogAccentColor = new Color(0.12f, 0.1f, 0.18f, 1f);
        public bool simplifiedFallback;
    }

    public enum MemoryZoneEffectType
    {
        VisualOnly,
        ChangeVisibility,
        ChangePassability,
        ChangeEnemyState
    }

    [Serializable]
    public class MemoryZoneModifier
    {
        public string zoneId;
        public MemoryZoneEffectType effectType;
        public string targetId;
        public float floatValue;
        public string stringValue;
        public bool boolValue;
    }
}
