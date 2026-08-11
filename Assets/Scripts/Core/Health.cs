using System;
using UnityEngine;

namespace Game.Core
{
    // Shared health/damage component used by both the player and enemies.
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        public float MaxHealth => maxHealth;
        public float Current { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<Vector2, Vector2> OnDamaged;    // hitPoint, hitDirection
        public event Action OnDied;

        private void Awake()
        {
            Current = maxHealth;
        }

        public void SetMaxHealth(float value)
        {
            maxHealth = value;
            Current = maxHealth;
        }

        public void TakeDamage(float amount, Vector2 hitPoint, Vector2 hitDirection)
        {
            if (IsDead || amount <= 0f) return;

            Current = Mathf.Max(0f, Current - amount);
            OnHealthChanged?.Invoke(Current, maxHealth);
            OnDamaged?.Invoke(hitPoint, hitDirection);

            if (Current <= 0f)
            {
                IsDead = true;
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            OnHealthChanged?.Invoke(Current, maxHealth);
        }
    }
}
