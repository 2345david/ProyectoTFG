using UnityEngine;

/// <summary>
/// Hace que una cámara siga al jugador todo el tiempo.
/// La cámara se coloca al lado del jugador a una distancia fija que tú decides.
/// </summary>
public class UICamera : MonoBehaviour
{
    public Transform player;          // El jugador al que la cámara va a seguir
    public float xpos, ypos, zpos;    // Cuánto se separa la cámara del jugador en X, Y y dónde está en Z (profundidad)

    // Al empezar el juego: coloca la cámara junto al jugador.
    void Start()
    {
        // Pone la cámara en la posición del jugador, sumándole la separación elegida
        transform.position = new Vector3(player.position.x + xpos, player.position.y + ypos, zpos);
    }

    // Cada cuadro (cada pequeña actualización del juego): vuelve a poner la cámara junto al jugador para que lo siga.
    void Update()
    {
        // Recoloca la cámara siguiendo al jugador con la misma separación
        transform.position = new Vector3(player.position.x + xpos, player.position.y + ypos, zpos);
    }
}
