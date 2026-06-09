using EnemyScript;          // Nos deja usar el componente Enemy (de ahí sacamos la velocidad)
using UnityEngine;

/// <summary>
/// Es la llama (disparo) que lanza el jefe. Nada más aparecer, mira dónde está el jugador
/// y sale volando en línea recta hacia él, siempre a la misma velocidad.
/// </summary>
public class FlameScript : MonoBehaviour
{ 
    private float moveSpeed;          // A qué velocidad se mueve la llama (se saca del componente Enemy)
    Rigidbody2D rb;                   // Pieza de física (Rigidbody2D) que mueve la llama
    Vector2 moveDirection;            // Hacia dónde y a qué velocidad va la llama
    PlayerController target;          // El jugador, que es a quien apunta el disparo

    // Al crearse la llama: calcula la dirección hacia el jugador y la lanza en línea recta.
    void Start()
    {
        // Coge la velocidad del componente Enemy y guarda la pieza de física para moverla
        moveSpeed = GetComponent<Enemy>().speed;
        rb = GetComponent<Rigidbody2D>();
        // El objetivo es el jugador
        target = PlayerController.instance;

        // Calcula la dirección que va de la llama al jugador y la multiplica por la velocidad
        // (.normalized deja la dirección con "tamaño 1" para que la velocidad sea siempre la misma)
        moveDirection = (target.transform.position - transform.position).normalized * moveSpeed;
        // Le da esa velocidad a la física para que la llama avance sola en línea recta
        rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y);
    }

    // Update se ejecuta en cada cuadro. Está vacío a propósito: la llama ya se mueve sola con la física.
    void Update()
    {
        
    }
}
