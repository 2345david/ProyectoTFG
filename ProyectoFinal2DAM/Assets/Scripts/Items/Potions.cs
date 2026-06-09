using UnityEngine;

/// <summary>
/// Poción de vida que se puede recoger (versión sencilla).
/// Cuando el jugador la toca, guarda una poción de curación en su inventario
/// (mochila) y suena un efecto. La poción no cura al momento, se guarda para luego.
/// </summary>
public class Potions : MonoBehaviour
{
    // Cuánta vida curará esta poción cuando se use (se ajusta desde el editor de Unity).
    public float healthToGive;

    // Se ejecuta cuando algo entra en la zona de contacto de la poción (su área que detecta toques).
    private void OnTriggerEnter2D(Collider2D collison)
    {
        // Miramos la etiqueta del objeto que entró: solo nos interesa si es el "Player" (jugador).
        if (collison.gameObject.tag == "Player")
        {
            // Guardamos esta poción de curación en el inventario del jugador.
            PlayerInventory.Instance.AddPotion(healthToGive);
            // Hacemos sonar el efecto de poción, si existe.
            if (AudioManager.instance != null && AudioManager.instance.potion != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.potion);
            // Ya recogida, quitamos la poción de la pantalla.
            Destroy(gameObject);
        }
    }
}
