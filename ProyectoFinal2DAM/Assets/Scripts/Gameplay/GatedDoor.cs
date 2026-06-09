using UnityEngine;
using Metroidvania.Core;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Es una puerta cerrada con llave. Solo se abre si el jugador tiene una habilidad
    /// especial (por ejemplo, un doble salto). Si no la tiene, la puerta sigue cerrada.
    /// </summary>
    public class GatedDoor : MonoBehaviour
    {
        public Ability requiredAbility;   // La habilidad que el jugador necesita para abrir la puerta.
        public bool isLocked = true;      // ¿Está cerrada? true = cerrada, false = abierta.

        [Header("Visuals")]
        public GameObject lockVisual;     // El dibujo del candado (se esconde cuando la puerta se abre).

        // Se ejecuta al empezar. Comprueba si la puerta ya debería estar abierta.
        private void Start()
        {
            UpdateDoorState();
        }

        // Revisa si el jugador ya tiene la habilidad. Si la tiene, abre la puerta
        // y esconde el candado.
        public void UpdateDoorState()
        {
            // Preguntamos si el jugador tiene la habilidad necesaria.
            if (ProgressionManager.Instance.HasAbility(requiredAbility))
            {
                isLocked = false;
                if (lockVisual != null) lockVisual.SetActive(false);
            }
        }

        // Se ejecuta cuando algo choca con la puerta. Si está cerrada y choca el jugador,
        // mostramos un aviso diciendo qué habilidad le falta.
        private void OnCollisionEnter2D(Collision2D other)
        {
            // Solo avisamos si la puerta sigue cerrada y quien choca es el jugador.
            if (isLocked && other.collider.CompareTag("Player"))
            {
                Debug.Log($"Door locked. Requires {requiredAbility}");
            }
        }
    }
}
