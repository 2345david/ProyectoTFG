using UnityEngine;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Controla una "sala" o habitación del nivel. Cuando el jugador entra o sale,
    /// avisa al juego y cambia la cámara para que enfoque bien esa sala.
    /// Necesita una caja invisible (BoxCollider2D) que detecte al jugador.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomController : MonoBehaviour
    {
        [Header("Room Info")]
        public string roomName;    // Nombre de la sala (sirve para identificarla fácilmente).
        public bool isCheckpoint;  // ¿Esta sala es un punto de control (sitio donde se guarda)?

        // Esto crea unos "avisos" (eventos) que otros scripts pueden escuchar
        // para enterarse de cuándo el jugador entra o sale de la sala.
        public delegate void RoomEvent();
        public event RoomEvent OnEnter; // Aviso que se lanza cuando el jugador ENTRA.
        public event RoomEvent OnExit;  // Aviso que se lanza cuando el jugador SALE.

        [Header("Camera Configuration")]
        // La cámara de esta sala. La guardamos de forma general para que funcione
        // aunque el proyecto use distintos tipos de cámara.
        public MonoBehaviour virtualCamera;

        // Al crear la sala, nos aseguramos de que su caja sea "atravesable" (trigger),
        // para detectar al jugador sin frenarlo.
        private void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        // Se ejecuta cuando algo entra en la sala. Si es el jugador, llamamos a OnPlayerEnter.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnter();
            }
        }

        // Qué pasa cuando el jugador entra: avisamos a los demás, registramos la sala
        // en la cámara y subimos la "importancia" de esta cámara para que ella mande.
        private void OnPlayerEnter()
        {
            Debug.Log($"Entered Room: {roomName}");
            // Lanzamos el aviso de "el jugador ha entrado".
            OnEnter?.Invoke();

            // Si existe el controlador de cámaras del juego, le decimos cuál es esta sala.
            if (CameraController.instance != null)
            {
                CameraController.instance.RegisterRoom(this.transform);
            }

            // Subimos la importancia (Priority) de esta cámara a 10 para que sea la que se vea.
            if (virtualCamera != null)
            {
                var prop = virtualCamera.GetType().GetProperty("Priority");
                if (prop != null) prop.SetValue(virtualCamera, 10);
            }
        }

        // Qué pasa cuando el jugador sale: avisamos a los demás, quitamos el registro
        // de la sala en la cámara y bajamos la importancia de esta cámara.
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Lanzamos el aviso de "el jugador ha salido".
                OnExit?.Invoke();
                // Si existe el controlador de cámaras, le decimos que ya no estamos en esta sala.
                if (CameraController.instance != null)
                {
                    CameraController.instance.UnregisterRoom(this.transform);
                }

                // Bajamos la importancia (Priority) de esta cámara a 0 para que deje de mandar.
                if (virtualCamera != null)
                {
                    var prop = virtualCamera.GetType().GetProperty("Priority");
                    if (prop != null) prop.SetValue(virtualCamera, 0);
                }
            }
        }
}
}
