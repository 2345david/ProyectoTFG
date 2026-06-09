using UnityEngine;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Marca el sitio donde aparece el jugador cuando llega a una zona nueva.
    /// El juego busca este punto por su etiqueta para saber dónde colocarlo.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        public string spawnTag; // Nombre/etiqueta de este punto, para poder encontrarlo.

        // Dibuja una marca de ayuda (solo visible en el editor de Unity, no en el juego)
        // para que veamos fácilmente dónde está este punto de aparición.
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            // Dibuja un círculo en la posición del punto.
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            // Dibuja una línea hacia arriba para indicar hacia dónde "mira" el punto.
            Gizmos.DrawLine(transform.position, transform.position + transform.up);
        }
    }
}
