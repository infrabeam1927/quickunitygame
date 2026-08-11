using UnityEngine;

namespace Game.Core
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector2 hitPoint, Vector2 hitDirection);
    }
}
