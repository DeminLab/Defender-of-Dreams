using System;
using DefenderOfDreams.Core;
using UnityEngine;

namespace DefenderOfDreams.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invulnDuration = 1.1f;
        [SerializeField] private bool isPlayer;

        private int _current;
        private float _invulnTimer;
        private bool _dead;

        public int Current => _current;
        public int Max => maxHealth;
        public bool IsAlive => !_dead;
        public bool IsInvulnerable => _invulnTimer > 0f;
        public float InvulnNormalized => maxHealth <= 0 ? 0f : _current / (float)maxHealth;

        public event Action<int, int> Changed;
        public event Action Died;

        private void Awake()
        {
            _current = maxHealth;
        }

        private void Start()
        {
            Raise();
        }

        private void Update()
        {
            if (_invulnTimer > 0f)
                _invulnTimer -= Time.deltaTime;
        }

        public void TakeDamage(int amount, Vector2 hitDirection)
        {
            if (_dead || IsInvulnerable || amount <= 0)
                return;

            _current = Mathf.Max(0, _current - amount);
            _invulnTimer = invulnDuration;
            Raise();

            if (_current <= 0)
            {
                _dead = true;
                Died?.Invoke();
                if (isPlayer)
                    GameEvents.RaisePlayerDied();
            }
        }

        public void Heal(int amount)
        {
            if (_dead || amount <= 0)
                return;
            _current = Mathf.Min(maxHealth, _current + amount);
            Raise();
        }

        public void RestoreFull()
        {
            _dead = false;
            _current = maxHealth;
            _invulnTimer = 0f;
            Raise();
        }

        public void Configure(int max, bool player = false)
        {
            maxHealth = Mathf.Max(1, max);
            isPlayer = player;
            RestoreFull();
        }

        public void ForceSetHealth(int value)
        {
            _current = Mathf.Clamp(value, 0, maxHealth);
            if (_current > 0)
                _dead = false;
            Raise();
        }

        private void SetInvulnForced(float duration)
        {
            _invulnTimer = Mathf.Max(_invulnTimer, duration);
        }

        private void Raise()
        {
            Changed?.Invoke(_current, maxHealth);
            if (isPlayer)
                GameEvents.RaiseHealthChanged(_current, maxHealth);
        }
    }
}
