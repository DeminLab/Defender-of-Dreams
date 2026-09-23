using DefenderOfDreams.Core;
using DefenderOfDreams.FogOfWar;
using UnityEngine;

namespace DefenderOfDreams.Memories
{
    public class MemoryPickup : MonoBehaviour
    {
        [SerializeField] private int memoryId;
        [SerializeField] private string title = "Воспоминание";
        [TextArea]
        [SerializeField] private string text = "";
        [SerializeField] private bool destroyOnCollect = true;

        [Header("4.5.2 — Zone effect")]
        [SerializeField] private bool applyZoneEffect = true;
        [SerializeField] private MemoryZoneEffectType zoneEffect = MemoryZoneEffectType.ChangeVisibility;
        [SerializeField] private float effectRadius = 4f;
        [SerializeField] private GameObject effectTarget;
        [SerializeField] private string effectZoneId = "";
        [SerializeField] private bool revealPermanently = true;

        [Header("4.5.4 — Mutual exclusion")]
        [SerializeField] private int exclusiveWithMemoryId = -1;
        [SerializeField] private string exclusiveFlag = "";

        private bool _collected;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void Start()
        {
            if (exclusiveWithMemoryId >= 0 && MemoryLog.Has(exclusiveWithMemoryId))
            {
                Suppress();
                return;
            }

            if (!string.IsNullOrEmpty(exclusiveFlag) && GameFlags.Has(exclusiveFlag))
            {
                Suppress();
                return;
            }

            if (memoryId >= 0 && MemoryLog.Has(memoryId))
                Suppress();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.CompareTag("Player"))
                return;
            Collect();
        }

        public void Collect()
        {
            if (_collected)
                return;
            if (!MemoryLog.Collect(memoryId, title, text))
                return;

            _collected = true;
            GameFlags.Set($"memory.{memoryId}.collected", true);

            if (applyZoneEffect)
                ApplyZoneEffect();

            if (!string.IsNullOrEmpty(exclusiveFlag))
                GameFlags.Set(exclusiveFlag, true);

            if (destroyOnCollect)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }

        private void Suppress()
        {
            if (destroyOnCollect)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }

        private void ApplyZoneEffect()
        {
            var fog = FogOfWarSystem.Instance;
            Vector2 origin = transform.position;

            switch (zoneEffect)
            {
                case MemoryZoneEffectType.VisualOnly:
                    if (fog != null)
                        fog.RevealCircle(origin, effectRadius);
                    break;

                case MemoryZoneEffectType.ChangeVisibility:
                    if (fog != null)
                    {
                        if (revealPermanently)
                            fog.PermanentCircle(origin, effectRadius);
                        else
                            fog.RevealCircle(origin, effectRadius);
                    }
                    break;

                case MemoryZoneEffectType.ChangePassability:
                    if (effectTarget != null)
                        effectTarget.SetActive(false);
                    break;

                case MemoryZoneEffectType.ChangeEnemyState:
                    if (effectTarget != null)
                    {
                        var enemy = effectTarget.GetComponent<Enemies.EnemyBase>();
                        if (enemy != null && enemy.IsAlive)
                            enemy.SeizeControls(true);
                        effectTarget.SetActive(false);
                    }
                    break;
            }

            if (fog != null && !string.IsNullOrEmpty(effectZoneId))
                fog.MarkZoneActive(effectZoneId);

            GameEvents.RaiseFlagSet($"memory.{memoryId}.zone");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            if (applyZoneEffect)
            {
                Gizmos.color = new Color(1f, 0.85f, 0.4f, 0.35f);
                Gizmos.DrawWireSphere(transform.position, effectRadius);
            }
        }
    }
}
