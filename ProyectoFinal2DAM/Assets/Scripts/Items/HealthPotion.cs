using UnityEngine;

/// <summary>
/// Poción de vida que se puede recoger. Cuando el jugador la toca, se guarda
/// una poción de curación en su inventario (mochila). No cura al momento:
/// se guarda para usarla cuando el jugador quiera.
/// </summary>
public class HealthPotion : MonoBehaviour
{
    // Cuánta vida curará esta poción cuando se use (se ajusta desde el editor de Unity).
    public float healAmount = 50f;

    // Se ejecuta cuando algo entra en la zona de contacto; si es el jugador, recoge la poción.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo nos interesa el objeto con la etiqueta "Player" (el jugador).
        if (collision.CompareTag("Player"))
        {
            // Solo recogemos la poción si la mochila del jugador existe.
            if (PlayerInventory.Instance != null)
            {
                // Guardamos esta poción de curación en la mochila.
                PlayerInventory.Instance.AddPotion(healAmount);

                // Hacemos sonar el efecto de poción, si existe.
                if (AudioManager.instance != null && AudioManager.instance.potion != null)
                    AudioManager.instance.PlayAudio(AudioManager.instance.potion);

                // Ya recogida, quitamos la poción de la pantalla.
                Destroy(gameObject);
            }
        }
    }
}
