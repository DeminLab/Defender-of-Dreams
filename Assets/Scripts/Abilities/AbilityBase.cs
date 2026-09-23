using UnityEngine;

namespace DefenderOfDreams.Abilities
{
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        [SerializeField] protected string abilityId = "ability";
        [SerializeField] protected string displayName = "Ability";
        [SerializeField] protected int resourceCost = 1;
        [SerializeField] protected float cooldown = 5f;
        [SerializeField] protected float actionRadius = 6f;

        protected float CooldownTimer;
        protected int ResourcePool = 3;
        protected int ResourceMax = 3;

        public virtual string Id => abilityId;
        public virtual string DisplayName => displayName;
        public virtual int ResourceCost => resourceCost;
        public virtual float Cooldown => cooldown;
        public float CooldownRemaining => CooldownTimer;

        public int Resource => ResourcePool;
        public int ResourceLimit => ResourceMax;

        protected virtual void Awake()
        {
            OnAbilityAwake();
        }

        protected virtual void OnAbilityAwake()
        {
        }

        protected virtual void Update()
        {
            if (CooldownTimer > 0f)
                CooldownTimer -= Time.deltaTime;
        }

        public virtual bool CanCast(GameObject caster)
        {
            return caster != null && CooldownTimer <= 0f && ResourcePool >= resourceCost;
        }

        public bool Cast(GameObject caster)
        {
            if (!CanCast(caster))
                return false;
            ResourcePool -= resourceCost;
            CooldownTimer = cooldown;
            return Execute(caster);
        }

        protected abstract bool Execute(GameObject caster);

        public void RefillResource()
        {
            ResourcePool = ResourceMax;
        }

        public void SetResource(int current, int max)
        {
            ResourceMax = Mathf.Max(1, max);
            ResourcePool = Mathf.Clamp(current, 0, ResourceMax);
        }
    }
}
