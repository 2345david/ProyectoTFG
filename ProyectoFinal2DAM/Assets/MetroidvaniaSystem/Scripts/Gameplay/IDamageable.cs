using UnityEngine;

namespace Metroidvania.Gameplay
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector2 hitDirection);
    }
}
