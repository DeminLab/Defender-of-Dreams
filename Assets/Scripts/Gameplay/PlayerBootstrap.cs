using DefenderOfDreams.Abilities;
using DefenderOfDreams.Combat;
using DefenderOfDreams.Player;
using UnityEngine;

namespace DefenderOfDreams.Gameplay
{
    public class PlayerBootstrap : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private MeleeAttack attack;
        [SerializeField] private AbilityCaster caster;
        [SerializeField] private Dodge dodge;
        [SerializeField] private Transform cameraTarget;

        private void Start()
        {
            gameObject.tag = "Player";

            if (health == null)
                health = GetComponent<Health>();
            if (attack == null)
                attack = GetComponent<MeleeAttack>();
            if (caster == null)
                caster = GetComponent<AbilityCaster>();
            if (dodge == null)
                dodge = GetComponent<Dodge>();

            if (cameraTarget == null)
                cameraTarget = transform;

            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<CameraFollow>();
                if (follow != null)
                    follow.SetTarget(cameraTarget);
            }

            if (health != null)
            {
                health.Died += OnDied;
                health.RestoreFull();
            }
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= OnDied;
        }

        private void OnDied()
        {
            if (attack != null)
                attack.InputLocked = true;
            if (caster != null)
                caster.InputLocked = true;
            if (dodge != null)
                dodge.InputLocked = true;
            Time.timeScale = 0.25f;
            Invoke(nameof(RestoreTime), 2f);
        }

        private void RestoreTime()
        {
            Time.timeScale = 1f;
        }
    }
}
