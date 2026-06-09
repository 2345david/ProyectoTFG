using UnityEngine;

/// <summary>
/// Es un peligro del escenario (pinchos, lava, un vacío, etc.). Si el jugador lo toca, muere.
/// Funciona tanto si lo atraviesa como si choca contra él.
/// </summary>
public class Hazard : MonoBehaviour
{
    // Se ejecuta cuando algo ATRAVIESA este peligro (zona que se puede cruzar).
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos que quien lo tocó es el jugador.
        if (collision.CompareTag("Player"))
        {
            // Buscamos la "vida" del jugador y, si la encontramos, lo matamos.
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
        }
    }

    // Se ejecuta cuando algo CHOCA contra este peligro (objeto sólido, no se puede atravesar).
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos que quien chocó es el jugador.
        if (collision.gameObject.CompareTag("Player"))
        {
            // Buscamos la "vida" del jugador y, si la encontramos, lo matamos.
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
        }
    }
}
