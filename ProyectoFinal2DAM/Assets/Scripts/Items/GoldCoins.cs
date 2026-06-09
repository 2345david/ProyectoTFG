using UnityEngine;

/// <summary>
/// Moneda de oro que se puede recoger. Cuando el jugador la toca, suma dinero
/// a la cuenta del jugador, suena el efecto de moneda y la moneda desaparece.
/// </summary>
public class GoldCoins : MonoBehaviour
{
    // Cuánto dinero da esta moneda (se ajusta desde el editor de Unity).
    public float cashToGive;

    // Se ejecuta cuando algo entra en la zona de contacto; si es el jugador, le da el dinero.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo nos interesa el objeto con la etiqueta "Player" (el jugador).
        if (collision.CompareTag("Player"))
        {
            // Sumamos el dinero a la cuenta del jugador.
            BankAccount.Instance.Money((float)cashToGive);
            // Hacemos sonar el efecto de moneda y quitamos la moneda de la pantalla.
            AudioManager.instance.PlayAudio(AudioManager.instance.coin);
            Destroy(gameObject);
        }
    }

}
