using DefenderOfDreams.Abilities;
using DefenderOfDreams.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Combat
{
    public class AbilityCaster : MonoBehaviour
    {
        [SerializeField] private AbilityBase primaryAbility;
        [SerializeField] private AbilityBase secondaryAbility;

        public AbilityBase Primary => primaryAbility;
        public AbilityBase Secondary => secondaryAbility;
        public bool InputLocked { get; set; }

        private void Update()
        {
            if (InputLocked)
                return;

            var mouse = Mouse.current;
            var kb = Keyboard.current;

            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
                TryCast(primaryAbility);

            if (kb != null && kb.qKey.wasPressedThisFrame)
                TryCast(secondaryAbility);

            if (primaryAbility != null)
                GameEvents.RaiseAbilityResourceChanged(primaryAbility.Resource, primaryAbility.ResourceLimit);
        }

        public void Configure(AbilityBase primary, AbilityBase secondary)
        {
            primaryAbility = primary;
            secondaryAbility = secondary;
        }

        private void TryCast(AbilityBase ability)
        {
            if (ability == null)
                return;
            if (ability.Cast(gameObject))
            {
                GameEvents.RaiseAbilityResourceChanged(
                    primaryAbility != null ? primaryAbility.Resource : 0,
                    primaryAbility != null ? primaryAbility.ResourceLimit : 1);
            }
        }
    }
}
