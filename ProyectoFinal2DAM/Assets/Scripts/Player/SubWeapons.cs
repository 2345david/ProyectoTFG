using UnityEngine;
using UnityEngine.InputSystem; // Sirve para leer el teclado con el sistema nuevo de Unity.

/// <summary>
/// Este script deja que el jugador dispare flechas.
/// Cuando aprietas la tecla, comprueba que tengas flechas y que no esté en "espera",
/// crea la flecha mirando hacia donde mira el jugador y suena el disparo.
/// </summary>
public class SubWeapons : MonoBehaviour
{
    public int ArrowCost = 1;           // Cuántas flechas (munición) gasta cada disparo.
    public GameObject ArrowPrefab;      // El "molde" de la flecha que vamos a crear al disparar.

    public float shootCooldown = 1f;    // Cuánto hay que esperar entre un disparo y otro (en segundos).
    private float currentCooldown = 0f; // Tiempo que falta para poder volver a disparar.

    // Esto se repite muchas veces por segundo. Va restando el tiempo de espera y revisa si quieres disparar.
    void Update()
    {
        currentCooldown -= Time.deltaTime;  // Le quitamos un poquito de tiempo a la espera en cada vuelta.
        UseSubWeapon();
    }

    // Mira si el jugador apretó la tecla de disparo (flecha arriba) y, si es así, intenta disparar.
    public void UseSubWeapon()
    {
        // Buscamos el teclado. Si no hay ninguno conectado, no hacemos nada.
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Si en este momento se apretó la flecha de arriba, intentamos disparar.
        if (keyboard.upArrowKey.wasPressedThisFrame)
            TryFireArrow();
    }

    // Intenta disparar una flecha. Solo lo logra si hay munición suficiente y ya pasó el tiempo de espera.
    // Devuelve "true" (sí) si disparó, o "false" (no) si no pudo.
    public bool TryFireArrow()
    {
        // Si no existe el inventario de flechas, no hay suficientes, o todavía estamos esperando: no disparamos.
        if (SubItems.Instance == null || ArrowCost > SubItems.Instance.subItemsAmount || currentCooldown > 0f)
            return false;

        // Le restamos al inventario las flechas que cuesta el disparo.
        SubItems.Instance.SubItem(-ArrowCost);

        GameObject sub;

        // Creamos la flecha y la lanzamos según hacia dónde está mirando el jugador.
        if (transform.localScale.x < 0)
        {
            // El jugador mira a la izquierda: giramos la flecha, la volteamos y la empujamos hacia la izquierda.
            sub = Instantiate(ArrowPrefab, transform.position, Quaternion.Euler(0, 0, 50));
            sub.GetComponent<SpriteRenderer>().flipX = true;
            sub.GetComponent<Rigidbody2D>().AddForce(new Vector2(-600f, 0f), ForceMode2D.Force);
        }
        else
        {
            // El jugador mira a la derecha: empujamos la flecha hacia la derecha.
            sub = Instantiate(ArrowPrefab, transform.position, Quaternion.Euler(0, 0, -50));
            sub.GetComponent<Rigidbody2D>().AddForce(new Vector2(600f, 0f), ForceMode2D.Force);
        }

        // Si hay sonido de flecha preparado, lo reproducimos.
        if (AudioManager.instance != null && AudioManager.instance.arrow != null)
            AudioManager.instance.PlayAudio(AudioManager.instance.arrow);

        currentCooldown = shootCooldown;    // Empezamos de nuevo la espera para el próximo disparo.
        return true;
    }
}