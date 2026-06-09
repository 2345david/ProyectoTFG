using UnityEngine;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Es el arma del jugador. Cuando toca algo que puede recibir daño (un enemigo),
    /// le quita vida.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        // Se ejecuta cuando el arma toca a algo. Aquí decidimos si ese algo recibe daño.
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Preguntamos al objeto tocado si "puede recibir daño".
            // Si puede, nos devuelve algo válido; si no, nos devuelve "nada" (null).
            IDamageable damageable = collision.GetComponent<IDamageable>();

            // Solo seguimos si el objeto puede recibir daño.
            if (damageable != null)
            {
                float damage = 1f; // Daño por defecto, por si no encontramos el dato del jugador.

                // Si el jugador tiene su sistema de ataque listo, usamos su valor de daño real.
                if (PlayerAttack.instance != null)
                {
                    damage = PlayerAttack.instance.damage;
                }

                // Le quitamos vida al objeto. También le pasamos la posición del arma
                // para que pueda salir empujado (el "knockback" o golpe que empuja).
                damageable.TakeDamage(damage, transform.position);
            }
        }
    }
}
