using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Escaleras / cuerdas: mientras el jugador solape la capa <b>Ladders</b> (Tilemap TM_Ladders con trigger),
/// W/S o flechas arriba/abajo suben o bajan; gravedad se anula en la escalera.
/// Añade este script al mismo GameObject que <see cref="PlayerController"/> (orden de ejecución posterior).
/// </summary>
// Orden de ejecución posterior (50) para correr después de PlayerController y poder
// sobrescribir su gravedad/velocidad mientras el jugador está en la escalera.
[DefaultExecutionOrder(50)]
public class PlayerLadderMovement : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 4f;       // Velocidad al subir/bajar pulsando arriba/abajo.
    [SerializeField] private float slideSpeed = 1.5f;     // Velocidad de descenso suave cuando no se pulsa nada.
    [SerializeField] private Transform climbCheck;        // Punto de comprobación de solape con la escalera.
    [SerializeField] private float checkRadius = 0.35f;   // Radio del círculo de detección de escalera.
    [SerializeField] private LayerMask ladderLayers;      // Capa(s) consideradas escalera.
    [SerializeField] private bool autoPickLadderLayer = true; // Si true, toma la capa "Ladders" automáticamente.

    private Rigidbody2D _rb;          // Rigidbody2D del jugador (cacheado).
    private float _defaultGravity;    // Gravedad original, para restaurarla al salir de la escalera.
    private bool _wasOnLadder;        // Indica si en el frame anterior estaba en la escalera.

    // Estado público de solo lectura: true mientras el jugador toca una escalera.
    public bool IsTouchingLadder { get; private set; }

    /// <summary>
    /// Cachea el Rigidbody2D, guarda su gravedad original, fija el punto de comprobación
    /// y, opcionalmente, resuelve la LayerMask de escaleras desde <see cref="GameTilemaps"/>.
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _defaultGravity = _rb.gravityScale;

        // Si no se asignó un punto de comprobación, usa el propio transform.
        if (climbCheck == null)
        {
            climbCheck = transform;
        }

        // Autodetección de la capa de escaleras para no depender del Inspector.
        if (autoPickLadderLayer)
        {
            var m = GameTilemaps.GetLaddersLayerMask();
            if (m.value != 0)
            {
                ladderLayers = m;
            }
        }
    }

    /// <summary>
    /// Lógica de física por frame fijo: detecta la escalera, anula la gravedad mientras se
    /// está sobre ella y controla el movimiento vertical (subir, bajar o deslizar). Al
    /// abandonar la escalera restaura la gravedad original.
    /// </summary>
    private void FixedUpdate()
    {
        if (_rb == null)
        {
            return;
        }

        // ¿El punto de comprobación solapa con alguna escalera?
        IsTouchingLadder = Physics2D.OverlapCircle(climbCheck.position, checkRadius, ladderLayers) != null;

        if (IsTouchingLadder)
        {
            // En la escalera no hay gravedad: el jugador se sostiene.
            _rb.gravityScale = 0f;

            var keyboard = Keyboard.current;
            // Sin teclado disponible: deslizamiento lento hacia abajo por defecto.
            if (keyboard == null)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -slideSpeed);
                _wasOnLadder = true;
                return;
            }

            // Lectura de la intención vertical: W/Arriba sube, S/Abajo baja.
            float vertical = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                vertical = 1f;
            }
            else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                vertical = -1f;
            }

            // Si hay intención de movimiento usa climbSpeed; si no, desliza despacio hacia abajo.
            var vy = vertical != 0f ? vertical * climbSpeed : -slideSpeed;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, vy);
            _wasOnLadder = true;
            return;
        }

        // Acaba de salir de la escalera: restaura la gravedad original una sola vez.
        if (_wasOnLadder)
        {
            _rb.gravityScale = _defaultGravity;
        }

        _wasOnLadder = false;
    }
}
