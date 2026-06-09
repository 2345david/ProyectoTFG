using UnityEngine;
using System.Collections.Generic; // Nos deja usar listas (List), que son grupos de cosas

/// <summary>
/// Hace que la cámara siga al jugador, pero sin salirse de la sala donde está.
/// Así la cámara nunca enseña lo que hay fuera de la habitación.
/// Otros scripts (como RoomController) le avisan a qué sala ha entrado el jugador.
/// </summary>
public class CameraController : MonoBehaviour
{
    public Transform player;        // El jugador al que la cámara sigue
    public Transform activeRoom;    // La sala donde está ahora el jugador (la cámara se queda dentro de ella)
    public float smoothTime = 0.2f; // Cuánto tarda la cámara en llegar: más alto = movimiento más suave y lento

    // "instance" deja que otros scripts hablen con esta cámara desde cualquier sitio (solo hay una)
    public static CameraController instance;

    private Camera _cam;                              // Guardamos aquí la cámara para no buscarla una y otra vez
    private Vector3 _currentVelocity = Vector3.zero;  // Dato interno que usa el movimiento suave

    // Lista de las salas en las que el jugador está metido en este momento
    private List<Transform> _detectedRooms = new List<Transform>();

    // Al despertar: guarda esta cámara como la única (instance) y recuerda su componente Camera.
    private void Awake()
    {
        // Si todavía no hay una cámara guardada, esta pasa a ser la oficial
        if (instance == null)
        {
            instance = this;
        }
        // Guarda el componente Camera una sola vez para usarlo después
        _cam = GetComponent<Camera>();
    }

    // Justo después de mover al jugador: recoloca la cámara sin dejar que se salga de la sala.
    private void LateUpdate()
    {
        // Si falta el jugador o la sala, no hay nada que seguir
        if (player == null || activeRoom == null) return;

        // Lee el tamaño y los bordes de la sala (de su caja de colisión, BoxCollider2D)
        Bounds roomBounds = activeRoom.GetComponent<BoxCollider2D>().bounds;

        // Calcula cuánto ve la cámara: la mitad del alto y la mitad del ancho de la pantalla
        float halfHeight = _cam.orthographicSize;
        float halfWidth = halfHeight * _cam.aspect;

        // Calcula hasta dónde puede moverse la cámara sin enseñar lo de fuera de la sala
        float minPosX = roomBounds.min.x + halfWidth;
        float maxPosX = roomBounds.max.x - halfWidth;
        float minPosY = roomBounds.min.y + halfHeight;
        float maxPosY = roomBounds.max.y - halfHeight;

        // Si la sala es más estrecha que lo que ve la cámara, deja la cámara centrada de lado a lado
        if (roomBounds.size.x < halfWidth * 2)
        {
            minPosX = maxPosX = roomBounds.center.x;
        }
        // Si la sala es más baja que lo que ve la cámara, deja la cámara centrada de arriba a abajo
        if (roomBounds.size.y < halfHeight * 2)
        {
            minPosY = maxPosY = roomBounds.center.y;
        }

        // No deja que la cámara pase de los bordes que acabamos de calcular (Clamp = "no te salgas de aquí")
        float targetX = Mathf.Clamp(player.position.x, minPosX, maxPosX);
        float targetY = Mathf.Clamp(player.position.y, minPosY, maxPosY);

        // Mueve la cámara poco a poco hacia ese punto, manteniendo su profundidad (Z)
        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);
    }

    // Apunta una sala nueva en la lista y la marca como la sala actual. La llama RoomController al entrar el jugador.
    public void RegisterRoom(Transform room)
    {
        // No la apunta dos veces si ya estaba en la lista
        if (!_detectedRooms.Contains(room))
        {
            _detectedRooms.Add(room);
        }
        activeRoom = room;
    }

    // Quita una sala de la lista. Si era la sala actual, vuelve a la última sala en la que seguía estando el jugador.
    public void UnregisterRoom(Transform room)
    {
        _detectedRooms.Remove(room);

        // Si salimos de la sala que era la actual, pasamos a la última de la lista
        if (activeRoom == room)
        {
            if (_detectedRooms.Count > 0)
            {
                activeRoom = _detectedRooms[_detectedRooms.Count - 1];
            }
        }
    }
}
