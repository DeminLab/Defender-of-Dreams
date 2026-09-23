using UnityEngine;

namespace DefenderOfDreams.Abilities
{
    public class AbilityForget : AbilityBase
    {
        [Tooltip("4.4 — Забыть: remove one enemy permanently")]
        [SerializeField] private LayerMask enemyLayers = ~0;

        protected override void OnAbilityAwake()
        {
            abilityId = "forget";
            displayName = "Забыть";
        }

        protected override bool Execute(GameObject caster)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, actionRadius, enemyLayers);
            Enemies.EnemyBase best = null;
            float bestDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.transform == caster.transform)
                    continue;
                var enemy = hit.GetComponentInParent<Enemies.EnemyBase>();
                if (enemy == null || !enemy.IsAlive)
                    continue;
                float d = ((Vector2)hit.transform.position - (Vector2)caster.transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = enemy;
                }
            }

            if (best == null)
                return false;
            best.Forget();
            return true;
        }
    }
}
