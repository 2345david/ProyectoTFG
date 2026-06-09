using UnityEngine;

/// <summary>
/// Poción de maná que se puede recoger. Cuando el jugador la toca, se guarda
/// una poción de maná en su inventario (mochila) para usarla más tarde.
/// También escribe mensajes en la consola para ayudar a encontrar errores.
/// </summary>
public class ManaPotion : MonoBehaviour
{
    // Cuánto maná dará esta poción cuando se use (se ajusta desde el editor de Unity).
    public float manaToGive = 50f;

    // Se ejecuta cuando algo entra en la zona de contacto; si es el jugador, recoge la poción.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mensaje de ayuda en la consola: dice qué objeto tocó la poción y cuál es su etiqueta.
        Debug.Log($"[ManaPotion] Triggered by: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        // Aceptamos al jugador si tiene la etiqueta "Player" o si su nombre contiene "Player".
        if (collision.CompareTag("Player") || collision.gameObject.name.Contains("Player"))
        {
            // Solo seguimos si la mochila del jugador existe.
            if (PlayerInventory.Instance != null)
            {
                // Guardamos esta poción de maná en la mochila.
                PlayerInventory.Instance.AddManaPotion(manaToGive);
                Debug.Log("[ManaPotion] Added to inventory.");

                // Hacemos sonar el efecto de poción, si existe.
                if (AudioManager.instance != null && AudioManager.instance.potion != null)
                    AudioManager.instance.PlayAudio(AudioManager.instance.potion);

                // Ya recogida, quitamos la poción de la pantalla.
                Destroy(gameObject);
            }
            else
            {
                // Avisamos en la consola si la mochila todavía no existe (algo va mal).
                Debug.LogError("[ManaPotion] PlayerInventory.Instance is null!");
            }
        }
    }
}
