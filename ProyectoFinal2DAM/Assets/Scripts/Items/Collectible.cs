using UnityEngine;

/// <summary>
/// Objeto recogible de flechas. Cuando el jugador lo toca, mete flechas en la
/// mochila (reserva) del jugador y suena el efecto de recogida.
/// </summary>
public class Collectible : MonoBehaviour
{
    // Cuántas flechas da este objeto (se ajusta desde el editor de Unity).
    public int arrowsToGive;

    // Se ejecuta cuando algo entra en la zona de contacto; si es el jugador, recoge las flechas.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo nos interesa el objeto con la etiqueta "Player" (el jugador).
        if (collision.CompareTag("Player"))
        {
            // Metemos las flechas en la mochila, si el contador de flechas existe.
            if (SubItems.Instance != null)
                SubItems.Instance.AddToReserve(arrowsToGive);

            // Hacemos sonar el efecto de recoger flechas y quitamos el objeto de la pantalla.
            AudioManager.instance.PlayAudio(AudioManager.instance.recoger_flechas);
            Destroy(gameObject);
        }
    }

}
