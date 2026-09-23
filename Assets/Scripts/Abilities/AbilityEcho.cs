using System.Collections.Generic;
using UnityEngine;

namespace DefenderOfDreams.Abilities
{
    public class AbilityEcho : AbilityBase
    {
        [Tooltip("4.4 — Эхо: show recent enemy trails on the map")]
        [SerializeField] private float trailDuration = 6f;
        [SerializeField] private float recordInterval = 0.35f;
        [SerializeField] private float recordLifetime = 14f;
        [SerializeField] private int maxSamplesPerEnemy = 40;
        [SerializeField] private Color trailColor = new Color(0.45f, 0.92f, 1f, 0.85f);
        [SerializeField] private float markerScale = 0.25f;

        private static readonly List<EchoSample> Samples = new List<EchoSample>();
        private readonly Dictionary<Enemies.EnemyBase, float> _perEnemyTimers = new Dictionary<Enemies.EnemyBase, float>();
        private readonly List<Enemies.EnemyBase> dead = new List<Enemies.EnemyBase>();
        private readonly List<SpriteRenderer> _markers = new List<SpriteRenderer>();
        private float _revealTimer;
        private Sprite _markerSprite;

        private struct EchoSample
        {
            public Vector2 pos;
            public float time;
            public string enemyKey;
        }

        protected override void OnAbilityAwake()
        {
            abilityId = "echo";
            displayName = "Эхо";
        }

        protected override void Update()
        {
            base.Update();
            RecordTrails(Time.deltaTime);

            if (_revealTimer > 0f)
            {
                _revealTimer -= Time.deltaTime;
                if (_revealTimer <= 0f)
                    ClearMarkers();
            }
        }

        protected override bool Execute(GameObject caster)
        {
            EnsureMarkerSprite();
            ClearMarkers();

            float now = Time.time;
            for (int i = 0; i < Samples.Count; i++)
            {
                var s = Samples[i];
                if (now - s.time > recordLifetime)
                    continue;

                float age01 = Mathf.Clamp01((now - s.time) / recordLifetime);
                var go = new GameObject($"EchoTrail_{s.enemyKey}_{i}");
                go.transform.position = new Vector3(s.pos.x, s.pos.y, 0f);
                go.transform.localScale = Vector3.one * markerScale * (1f - age01 * 0.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _markerSprite;
                sr.color = trailColor * (1f - age01 * 0.7f);
                sr.sortingOrder = 20;
                _markers.Add(sr);
            }

            _revealTimer = trailDuration;
            return _markers.Count > 0;
        }

        private void RecordTrails(float dt)
        {
            var enemies = Object.FindObjectsByType<Enemies.EnemyBase>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                string key = enemy.GetEntityId().ToString();
                if (!_perEnemyTimers.TryGetValue(enemy, out float t))
                    t = 0f;
                t -= dt;
                if (t > 0f)
                {
                    _perEnemyTimers[enemy] = t;
                    continue;
                }

                _perEnemyTimers[enemy] = recordInterval;
                Samples.Add(new EchoSample
                {
                    pos = enemy.transform.position,
                    time = Time.time,
                    enemyKey = key
                });
            }

            dead.Clear();
            foreach (var kv in _perEnemyTimers)
                if (kv.Key == null || !kv.Key.IsAlive)
                    dead.Add(kv.Key);
            for (int d = 0; d < dead.Count; d++)
                _perEnemyTimers.Remove(dead[d]);
            dead.Clear();

            if (Samples.Count > 0)
            {
                Samples.RemoveAll(s => Time.time - s.time > recordLifetime);
                var counts = new Dictionary<string, int>();
                for (int i = Samples.Count - 1; i >= 0; i--)
                {
                    string id = Samples[i].enemyKey;
                    counts.TryGetValue(id, out int c);
                    c++;
                    counts[id] = c;
                    if (c > maxSamplesPerEnemy)
                        Samples.RemoveAt(i);
                }
            }
        }

        private void EnsureMarkerSprite()
        {
            if (_markerSprite != null)
                return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color32[16];
            for (int i = 0; i < 16; i++)
                pixels[i] = new Color32(255, 255, 255, 180);
            tex.SetPixels32(pixels);
            tex.Apply(false);
            tex.filterMode = FilterMode.Point;
            _markerSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        }

        private void ClearMarkers()
        {
            foreach (var m in _markers)
                if (m != null)
                    Destroy(m.gameObject);
            _markers.Clear();
        }

        private void OnDestroy() => ClearMarkers();
    }
}
