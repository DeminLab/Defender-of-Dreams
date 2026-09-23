using DefenderOfDreams.Abilities;
using UnityEngine;

namespace DefenderOfDreams.Abilities
{
    public class AbilitySleep : AbilityBase
    {
        [Tooltip("4.4 — Сон: freeze enemies in radius")]
        [SerializeField] private float duration = 3f;
        [SerializeField] private LayerMask enemyLayers = ~0;

        protected override void OnAbilityAwake()
        {
            abilityId = "sleep";
            displayName = "Сон";
        }

        protected override bool Execute(GameObject caster)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, actionRadius, enemyLayers);
            foreach (var hit in hits)
            {
                var sleeper = hit.GetComponentInParent<ISleepable>();
                sleeper?.Sleep(duration);
            }

            return true;
        }
    }

    public interface ISleepable
    {
        void Sleep(float duration);
    }
}
