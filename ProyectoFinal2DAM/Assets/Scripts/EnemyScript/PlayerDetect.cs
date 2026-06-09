using UnityEngine;


/// <summary>
/// Es como un "radar" que avisa cuando el jugador se acerca. Se pone en un objeto hijo con
/// una zona invisible (trigger). Cuando el jugador entra o sale de esa zona, le avisa al
/// script de disparo (EnemyProjectile) del objeto padre para que decida si dispara o no.
/// </summary>
public class PlayerDetect : MonoBehaviour
{
    // El script de disparo que está en el objeto padre.
    private EnemyProjectile parentProjectile;

    // Al empezar, buscamos y guardamos el script de disparo del padre.
    private void Start()
    {
        parentProjectile = GetComponentInParent<EnemyProjectile>();
    }

    // El jugador entra en el área de detección: avisamos al disparador.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            parentProjectile.OnPlayerEnter();
        }
    }

    // El jugador sale del área de detección: avisamos al disparador.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            parentProjectile.OnPlayerExit();
        }
    }
}