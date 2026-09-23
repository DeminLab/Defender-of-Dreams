using UnityEngine;

namespace DefenderOfDreams.Abilities
{
    public interface IAbility
    {
        string Id { get; }
        string DisplayName { get; }
        int ResourceCost { get; }
        float Cooldown { get; }
        bool CanCast(GameObject caster);
        bool Cast(GameObject caster);
        float CooldownRemaining { get; }
    }
}
