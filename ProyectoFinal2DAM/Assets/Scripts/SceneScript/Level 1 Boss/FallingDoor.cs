using UnityEngine;

/// <summary>
/// Es una puerta que baja sola cuando se enciende, hasta una altura concreta.
/// Sirve para encerrar al jugador en la zona del jefe. Se puede volver a poner
/// en su sitio de origen con ResetDoor.
/// </summary>
public class FallingDoor : MonoBehaviour
{
    public float targetY = -5.7f;     // Altura a la que la puerta deja de bajar
    public float fallSpeed = 10f;     // Cómo de rápido baja la puerta
    public bool isFalling = false;    // Vale "sí" mientras la puerta está bajando

    private Rigidbody2D rb;               // Pieza de física (Rigidbody2D) de la puerta
    private Vector3 initialPosition;      // Dónde estaba al principio, para poder devolverla ahí

    // Al despertar: recuerda dónde estaba la puerta y prepara su física para que se mueva por código y no por golpes.
    void Awake()
    {
        // Apunta la posición inicial para poder volver a ella más tarde
        initialPosition = transform.position;
        // Guarda la pieza de física y la pone en modo "la muevo yo a mano" (Kinematic), no por choques
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
        }
    }

    // Vuelve a poner la puerta en su sitio de origen y deja de bajar. La usa el jefe cuando reaparece.
    public void ResetDoor()
    {
        isFalling = false;
        transform.position = initialPosition;
    }

    // Cuando se enciende la puerta, empieza a bajar.
    void OnEnable()
    {
        isFalling = true;
    }

    // En cada cuadro, si la puerta está bajando la mueve un poco hacia abajo y se para al llegar a su altura.
    void Update()
    {
        if (isFalling)
        {
            // Mueve la altura (Y) poco a poco hacia targetY (MoveTowards = avanzar sin pasarse)
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, fallSpeed * Time.deltaTime);
            transform.position = pos;

            // Cuando ya está casi en la altura final, deja de bajar
            if (Mathf.Abs(pos.y - targetY) < 0.001f)
            {
                isFalling = false;
            }
        }
    }

    // Permite cambiar desde otro script la altura a la que la puerta debe parar.
    public void SetTargetY(float y)
    {
        targetY = y;
    }
}
