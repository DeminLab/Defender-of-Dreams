using DefenderOfDreams.FogOfWar;
using UnityEngine;

namespace DefenderOfDreams.Abilities
{
    public class AbilityReverie : AbilityBase
    {
        [Tooltip("4.4 — Воспоминание: temporarily reveal the whole map")]

        protected override void OnAbilityAwake()
        {
            abilityId = "reverie";
            displayName = "Воспоминание";
        }

        protected override bool Execute(GameObject caster)
        {
            var fog = FogOfWarSystem.Instance;
            if (fog == null)
                return false;
            fog.RevealCircle(caster.transform.position, Mathf.Max(fog.LevelBounds.size.x, fog.LevelBounds.size.y));
            return true;
        }
    }
}
