using UnityEngine;
using Metroidvania.Gameplay;

namespace Metroidvania.Gameplay
{
    public class PlayerWeapon : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Try to get the IDamageable interface from the hit object
            IDamageable damageable = collision.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                float damage = 1f; // Fallback
                
                if (PlayerAttack.instance != null)
                {
                    damage = PlayerAttack.instance.damage;
                }
                
                // Pass damage and the weapon's position for knockback calculation
                damageable.TakeDamage(damage, transform.position);
            }
        }
    }
}
