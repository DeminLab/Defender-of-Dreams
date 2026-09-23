using System;
using System.Collections.Generic;
using DefenderOfDreams.Core;
using DefenderOfDreams.Save;
using UnityEngine;

namespace DefenderOfDreams.FogOfWar
{
    public class FogOfWarSystem : MonoBehaviour
    {
        public static FogOfWarSystem Instance { get; private set; }

        [SerializeField] private FogSettings settings = new FogSettings();
        [SerializeField] private Transform player;
        [SerializeField] private Bounds levelBounds = new Bounds(Vector3.zero, new Vector3(64f, 64f, 0f));

        public FogSettings Settings => settings;
        public Bounds LevelBounds => levelBounds;
        public Texture2D MaskTexture => _mask;
        public Vector2 WorldOrigin => new Vector2(levelBounds.min.x, levelBounds.min.y);
        public Vector2 WorldSize => new Vector2(levelBounds.size.x, levelBounds.size.y);
        public int Width => _width;
        public int Height => _height;
        public int AvailableAnchors => Mathf.Max(0, _anchorCapacity - _anchorUsed);
        public int AnchorCapacity => _anchorCapacity;
        public int AnchorsPlaced => _anchorUsed;
        public IReadOnlyList<AnchorRecord> Anchors => _anchors;
        public string ActiveZoneId => _activeZoneId;

        private int _width;
        private int _height;
        private FogCellState[] _states;
        private float[] _timers;
        private float[] _visual;
        private Texture2D _mask;
        private bool _dirty;
        private readonly List<AnchorRecord> _anchors = new List<AnchorRecord>();
        private readonly HashSet<int> _anchorCells = new HashSet<int>();
        private int _anchorCapacity = 1;
        private int _anchorUsed;
        private string _activeZoneId = "";
        private float _zonePressure;

        [Serializable]
        public struct AnchorRecord
        {
            public Vector2 position;
            public float radius;
            public bool destroyed;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializeGrid();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void InitializeGrid()
        {
            _width = Mathf.Max(1, Mathf.CeilToInt(levelBounds.size.x / settings.cellSize));
            _height = Mathf.Max(1, Mathf.CeilToInt(levelBounds.size.y / settings.cellSize));
            int count = _width * _height;
            _states = new FogCellState[count];
            _timers = new float[count];
            _visual = new float[count];
            for (int i = 0; i < count; i++)
            {
                _states[i] = FogCellState.Hidden;
                _visual[i] = 1f;
            }

            _mask = new Texture2D(_width, _height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            PushMask();
        }

        public void SetPlayer(Transform t) => player = t;

        public void SetLevelBounds(Bounds bounds)
        {
            levelBounds = bounds;
            InitializeGrid();
        }

        public void Configure(FogSettings newSettings, int anchorCapacity)
        {
            settings = newSettings;
            _anchorCapacity = Mathf.Max(1, anchorCapacity);
            GameEvents.RaiseAnchorsChanged(AvailableAnchors);
        }

        private void Update()
        {
            if (player == null)
                return;

            TickForget(Time.deltaTime);
            RevealAroundPlayer();
            UpdateVisual(Time.deltaTime);
            RaiseZoneForget();

            if (_dirty)
                PushMask();
        }

        private void RevealAroundPlayer()
        {
            RevealCircle(player.position, settings.awarenessRadius);
        }

        public void RevealCircle(Vector2 worldPos, float radius)
        {
            float cell = settings.cellSize;
            int cx = Mathf.FloorToInt((worldPos.x - levelBounds.min.x) / cell);
            int cy = Mathf.FloorToInt((worldPos.y - levelBounds.min.y) / cell);
            int r = Mathf.CeilToInt(radius / cell);
            float rCells = radius / cell;

            for (int y = cy - r; y <= cy + r; y++)
            {
                if (y < 0 || y >= _height)
                    continue;
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || x >= _width)
                        continue;
                    float dx = x + 0.5f - (worldPos.x - levelBounds.min.x) / cell;
                    float dy = y + 0.5f - (worldPos.y - levelBounds.min.y) / cell;
                    if (dx * dx + dy * dy > rCells * rCells)
                        continue;

                    int idx = y * _width + x;
                    if (_states[idx] == FogCellState.LostForever || _states[idx] == FogCellState.Permanent)
                        continue;
                    if (_states[idx] != FogCellState.Revealed)
                        _dirty = true;
                    _states[idx] = FogCellState.Revealed;
                    _timers[idx] = settings.forgetTimer;
                }
            }
        }

        private void TickForget(float dt)
        {
            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i] != FogCellState.Revealed)
                    continue;
                _timers[i] -= dt;
                if (_timers[i] <= 0f)
                {
                    _states[i] = FogCellState.Hidden;
                    _timers[i] = 0f;
                    _dirty = true;
                }
            }
        }

        private void UpdateVisual(float dt)
        {
            float blend = 1f - Mathf.Exp(-settings.revealBlendSpeed * dt);
            bool changed = false;
            for (int i = 0; i < _visual.Length; i++)
            {
                float target = _states[i] switch
                {
                    FogCellState.Revealed => 0f,
                    FogCellState.Permanent => 0f,
                    FogCellState.LostForever => 1f,
                    _ => 1f
                };
                float next = Mathf.Lerp(_visual[i], target, blend);
                if (!Mathf.Approximately(next, _visual[i]))
                {
                    _visual[i] = next;
                    changed = true;
                }
            }

            if (changed)
                _dirty = true;
        }

        private void PushMask()
        {
            var pixels = new Color32[_visual.Length];
            for (int i = 0; i < _visual.Length; i++)
            {
                byte fog = (byte)Mathf.Clamp(Mathf.RoundToInt(_visual[i] * 255f), 0, 255);
                byte lost = _states[i] == FogCellState.LostForever ? (byte)255 : (byte)0;
                byte permanent = _states[i] == FogCellState.Permanent ? (byte)255 : (byte)0;
                pixels[i] = new Color32(fog, lost, permanent, 255);
            }

            _mask.SetPixels32(pixels);
            _mask.Apply(false);
            _dirty = false;
        }

        public bool TryPlaceAnchor(Vector2 worldPos)
        {
            if (_anchorUsed >= _anchorCapacity)
                return false;

            var record = new AnchorRecord
            {
                position = worldPos,
                radius = settings.awarenessRadius,
                destroyed = false
            };
            _anchors.Add(record);
            _anchorUsed++;
            PermanentCircle(worldPos, record.radius);
            GameEvents.RaiseAnchorsChanged(AvailableAnchors);
            return true;
        }

        public void PermanentCircle(Vector2 worldPos, float radius)
        {
            ForEachCellInCircle(worldPos, radius, idx =>
            {
                if (_states[idx] == FogCellState.LostForever)
                    return;
                _states[idx] = FogCellState.Permanent;
                _anchorCells.Add(idx);
                _dirty = true;
            });
        }

        public void ConsumeForever(Vector2 worldPos, float radius)
        {
            ForEachCellInCircle(worldPos, radius, idx =>
            {
                if (_states[idx] == FogCellState.Permanent)
                    _anchorCells.Remove(idx);
                _states[idx] = FogCellState.LostForever;
                _timers[idx] = 0f;
                _dirty = true;
            });
        }

        public void DestroyAnchorsIn(Vector2 worldPos, float radius)
        {
            float r2 = radius * radius;
            bool any = false;
            for (int i = 0; i < _anchors.Count; i++)
            {
                if (_anchors[i].destroyed)
                    continue;
                var a = _anchors[i];
                if ((a.position - worldPos).sqrMagnitude <= r2)
                {
                    a.destroyed = true;
                    _anchors[i] = a;
                    any = true;
                }
            }

            if (any)
            {
                _anchorUsed = 0;
                foreach (var a in _anchors)
                    if (!a.destroyed)
                        _anchorUsed++;
                GameEvents.RaiseAnchorsChanged(AvailableAnchors);
            }
        }

        public void MarkZoneActive(string zoneId)
        {
            _activeZoneId = zoneId ?? "";
            _zonePressure = 0f;
        }

        public void SetActiveZonePressure(string zoneId, float pressure01)
        {
            if (!string.IsNullOrEmpty(zoneId) && zoneId != _activeZoneId)
                _activeZoneId = zoneId;
            _zonePressure = Mathf.Clamp01(pressure01);
        }

        public void ForgetZone(string zoneId)
        {
            if (player == null)
                return;
            float radius = settings.awarenessRadius * (1f + _zonePressure);
            ForEachCellInCircle(player.position, radius, idx =>
            {
                if (_states[idx] == FogCellState.Permanent)
                    _anchorCells.Remove(idx);
                _states[idx] = FogCellState.LostForever;
                _timers[idx] = 0f;
                _dirty = true;
            });
            _zonePressure = 0f;
            _activeZoneId = zoneId ?? "";
        }

        private void RaiseZoneForget()
        {
            if (player == null)
                return;

            if (!string.IsNullOrEmpty(_activeZoneId))
            {
                GameEvents.RaiseActiveZoneForgettingChanged(_activeZoneId, _zonePressure * 100f, 100f);
                return;
            }

            float minTimer = float.MaxValue;
            int cx = Mathf.FloorToInt((player.position.x - levelBounds.min.x) / settings.cellSize);
            int cy = Mathf.FloorToInt((player.position.y - levelBounds.min.y) / settings.cellSize);
            int r = Mathf.CeilToInt(settings.awarenessRadius / settings.cellSize);
            for (int y = cy - r; y <= cy + r; y++)
            {
                if (y < 0 || y >= _height)
                    continue;
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || x >= _width)
                        continue;
                    int idx = y * _width + x;
                    if (_states[idx] != FogCellState.Revealed)
                        continue;
                    if (_timers[idx] < minTimer)
                        minTimer = _timers[idx];
                }
            }

            float normalized = minTimer == float.MaxValue
                ? 1f
                : Mathf.Clamp01(minTimer / Mathf.Max(0.01f, settings.forgetTimer));
            GameEvents.RaiseActiveZoneForgettingChanged(normalized);
        }

        public FogCellState GetState(Vector2 worldPos)
        {
            int idx = WorldToIndex(worldPos);
            if (idx < 0)
                return FogCellState.Hidden;
            return _states[idx];
        }

        public bool IsVisible(Vector2 worldPos)
        {
            var s = GetState(worldPos);
            return s == FogCellState.Revealed || s == FogCellState.Permanent;
        }

        public void WriteToSave(SaveData data)
        {
            if (_states == null)
                return;
            data.fogWidth = _width;
            data.fogHeight = _height;
            data.fogStates.Clear();
            data.fogTimers.Clear();
            for (int i = 0; i < _states.Length; i++)
            {
                data.fogStates.Add((int)_states[i]);
                data.fogTimers.Add(_timers[i]);
            }

            data.anchorsPlaced = _anchorUsed;
        }

        public void ReadFromSave(SaveData data)
        {
            if (data?.fogStates == null || data.fogStates.Count == 0)
                return;
            if (_states == null)
                InitializeGrid();

            int n = Mathf.Min(_states.Length, data.fogStates.Count);
            for (int i = 0; i < n; i++)
            {
                _states[i] = (FogCellState)data.fogStates[i];
                _timers[i] = i < data.fogTimers.Count ? data.fogTimers[i] : 0f;
                if (_states[i] == FogCellState.Permanent)
                    _anchorCells.Add(i);
            }

            _dirty = true;
            PushMask();
            GameEvents.RaiseAnchorsChanged(AvailableAnchors);
        }

        private int WorldToIndex(Vector2 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - levelBounds.min.x) / settings.cellSize);
            int y = Mathf.FloorToInt((worldPos.y - levelBounds.min.y) / settings.cellSize);
            if (x < 0 || y < 0 || x >= _width || y >= _height)
                return -1;
            return y * _width + x;
        }

        private void ForEachCellInCircle(Vector2 worldPos, float radius, Action<int> action)
        {
            float cell = settings.cellSize;
            int cx = Mathf.FloorToInt((worldPos.x - levelBounds.min.x) / cell);
            int cy = Mathf.FloorToInt((worldPos.y - levelBounds.min.y) / cell);
            int r = Mathf.CeilToInt(radius / cell);
            float rCells = radius / cell;
            for (int y = cy - r; y <= cy + r; y++)
            {
                if (y < 0 || y >= _height)
                    continue;
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || x >= _width)
                        continue;
                    float dx = x + 0.5f - (worldPos.x - levelBounds.min.x) / cell;
                    float dy = y + 0.5f - (worldPos.y - levelBounds.min.y) / cell;
                    if (dx * dx + dy * dy > rCells * rCells)
                        continue;
                    action(y * _width + x);
                }
            }
        }
    }
}
