using UnityEngine;
using Metroidvania.Core;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Es como una "puerta invisible". Cuando el jugador la toca, lo lleva a otra zona
    /// (otra escena) del juego y lo coloca en el lugar correcto.
    /// </summary>
    public class TransitionTrigger : MonoBehaviour
    {
        public string targetScene;     // Nombre de la zona (escena) a la que vamos a llevar al jugador.
        public string targetSpawnTag;  // Etiqueta del sitio exacto donde aparecerá el jugador al llegar.

        // Esto se ejecuta solo cuando algo entra dentro de esta zona invisible.
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Comprobamos que quien entró es el jugador (y no otra cosa).
            // Si es el jugador, pedimos al "WorldManager" que lo lleve a la nueva zona.
            if (other.CompareTag("Player"))
            {
                WorldManager.Instance.TravelToBiome(targetScene, targetSpawnTag);
            }
        }
    }
}
