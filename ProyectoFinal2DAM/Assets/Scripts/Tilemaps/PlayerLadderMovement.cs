using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Escaleras / cuerdas: mientras el jugador solape la capa <b>Ladders</b> (Tilemap TM_Ladders con trigger),
/// W/S o flechas arriba/abajo suben o bajan; gravedad se anula en la escalera.
/// Añade este script al mismo GameObject que <see cref="PlayerController"/> (orden de ejecución posterior).
/// </summary>
[DefaultExecutionOrder(50)]
public class PlayerLadderMovement : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float slideSpeed = 1.5f;
    [SerializeField] private Transform climbCheck;
    [SerializeField] private float checkRadius = 0.35f;
    [SerializeField] private LayerMask ladderLayers;
    [SerializeField] private bool autoPickLadderLayer = true;

    private Rigidbody2D _rb;
    private float _defaultGravity;
    private bool _wasOnLadder;

    public bool IsTouchingLadder { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _defaultGravity = _rb.gravityScale;

        if (climbCheck == null)
        {
            climbCheck = transform;
        }

        if (autoPickLadderLayer)
        {
            var m = GameTilemaps.GetLaddersLayerMask();
            if (m.value != 0)
            {
                ladderLayers = m;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null)
        {
            return;
        }

        IsTouchingLadder = Physics2D.OverlapCircle(climbCheck.position, checkRadius, ladderLayers) != null;

        if (IsTouchingLadder)
        {
            _rb.gravityScale = 0f;

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -slideSpeed);
                _wasOnLadder = true;
                return;
            }

            float vertical = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                vertical = 1f;
            }
            else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                vertical = -1f;
            }

            var vy = vertical != 0f ? vertical * climbSpeed : -slideSpeed;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, vy);
            _wasOnLadder = true;
            return;
        }

        if (_wasOnLadder)
        {
            _rb.gravityScale = _defaultGravity;
        }

        _wasOnLadder = false;
    }
}
