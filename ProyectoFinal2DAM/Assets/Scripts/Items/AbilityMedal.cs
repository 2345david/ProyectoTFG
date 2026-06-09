using UnityEngine;

/// <summary>
/// Medalla que se puede recoger. Cuando el jugador la toca, le da para siempre
/// una nueva habilidad (doble salto o dash, que es un impulso rápido hacia un lado).
/// </summary>
public class AbilityMedal : MonoBehaviour
{
    // Lista de habilidades que esta medalla puede dar: doble salto o dash (impulso rápido).
    public enum AbilityType { DoubleJump, Dash }

    // Qué habilidad da exactamente esta medalla (se elige desde el editor de Unity).
    public AbilityType abilityToUnlock;

    // Start especial que puede esperar (corrutina): si el jugador ya tenía esta habilidad
    // (por ejemplo al continuar una partida guardada), la medalla sobra y se elimina.
    private System.Collections.IEnumerator Start()
    {
        // Esperamos un instante (un frame) para dar tiempo a que la partida termine de cargarse
        // y las habilidades del jugador ya estén puestas.
        yield return null;

        if (PlayerController.instance != null)
        {
            // Si el jugador ya tiene esta habilidad, esta medalla no hace falta: se elimina.
            if (abilityToUnlock == AbilityType.DoubleJump && PlayerController.instance.hasDoubleJump)
            {
                Destroy(gameObject);
            }
            else if (abilityToUnlock == AbilityType.Dash && PlayerController.instance.hasDash)
            {
                Destroy(gameObject);
            }
        }
    }

    // Se ejecuta cuando algo entra en la zona de contacto. Si es el jugador: le da la habilidad,
    // guarda la partida, suena un efecto y la medalla desaparece.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo nos interesa el objeto con la etiqueta "Player" (el jugador).
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Le encendemos al jugador la habilidad que corresponda.
                if (abilityToUnlock == AbilityType.DoubleJump)
                {
                    player.hasDoubleJump = true;
                }
                else if (abilityToUnlock == AbilityType.Dash)
                {
                    player.hasDash = true;
                }

                // Guardamos la partida para que la habilidad no se pierda al continuar más tarde.
                if (CheckpointManager.instance != null)
                {
                    CheckpointManager.instance.SaveGame();
                }

                // Hacemos sonar el efecto de subir de nivel, si existe.
                if (AudioManager.instance != null && AudioManager.instance.LvlUp != null)
                {
                    AudioManager.instance.PlayAudio(AudioManager.instance.LvlUp);
                }

                // La medalla ya hizo su trabajo: se quita de la pantalla.
                Destroy(gameObject);
            }
        }
    }
}
