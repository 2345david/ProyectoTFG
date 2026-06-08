using UnityEngine;

/// <summary>
/// Instancia un prefab de enemigos de habitación (ej. "EnemigosHabitacion1") y lo regenera
/// cuando el jugador permanece fuera del <see cref="leaveRadius"/> el tiempo indicado.
/// Tras un respawn hace falta volver a entrar en el radio antes de que pueda dispararse otro.
/// </summary>
public class RoomEnemyRespawner : MonoBehaviour
{
    [Header("Prefab de la sala")]
    [Tooltip("Prefab raíz con todos los enemigos de esa habitación (instancia única que se destruye y recrea).")]
    [SerializeField] private GameObject enemiesRoomPrefab;

    [Header("Zona (2D)")]
    [Tooltip("Centro del radio; si está vacío, usa la posición de este objeto.")]
    [SerializeField] private Transform roomCenter;

    [SerializeField] private float leaveRadius = 12f;
    [Tooltip("Segundos seguidos fuera del radio antes de respawnear.")]
    [SerializeField] private float secondsOutsideBeforeRespawn = 0.4f;

    [Header("Dónde aparece el prefab")]
    [Tooltip("Posición/rotación de la instancia; si está vacío, usa este transform.")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Offset local respecto al anchor (spawnPoint o este objeto) si se desea ajustar la posición.")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [SerializeField] private Transform spawnParent;

    [Header("Jugador")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";

    private GameObject _roomInstance;
    private float _outsideTimer;
    private bool _armedForNextExit = true;

    private void Awake()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
            {
                playerTransform = p.transform;
            }
        }
    }

    private void Start()
    {
        SpawnRoomInstance();
    }

    private void Update()
    {
        if (enemiesRoomPrefab == null || playerTransform == null)
        {
            return;
        }

        var center = roomCenter != null ? roomCenter.position : transform.position;
        var playerPos = playerTransform.position;
        var distSq = (new Vector2(playerPos.x, playerPos.y) - new Vector2(center.x, center.y)).sqrMagnitude;
        var r = leaveRadius;
        var inside = distSq <= r * r;

        if (inside)
        {
            _outsideTimer = 0f;
            _armedForNextExit = true;
            return;
        }

        _outsideTimer += Time.deltaTime;
        if (!_armedForNextExit || _outsideTimer < secondsOutsideBeforeRespawn)
        {
            return;
        }

        RespawnRoom();
        _outsideTimer = 0f;
        _armedForNextExit = false;
    }

    /// <summary>Fuerza respawn inmediato (útil desde otros scripts o eventos).</summary>
    public void RespawnRoom()
    {
        if (enemiesRoomPrefab == null)
        {
            Debug.LogWarning($"{name}: asigna el prefab de enemigos de la habitación.", this);
            return;
        }

        if (_roomInstance != null)
        {
            Destroy(_roomInstance);
            _roomInstance = null;
        }

        SpawnRoomInstance();
    }

    private void SpawnRoomInstance()
    {
        if (enemiesRoomPrefab == null)
        {
            return;
        }

        var anchor = spawnPoint != null ? spawnPoint : transform;
        var parent = spawnParent;
        
        // Calculamos la posición con el offset aplicado localmente al anchor.
        Vector3 finalPosition = anchor.position + anchor.TransformDirection(spawnOffset);

        _roomInstance = Instantiate(
            enemiesRoomPrefab,
            finalPosition,
            anchor.rotation,
            parent);
    }

    private void OnDrawGizmosSelected()
    {
        var center = roomCenter != null ? roomCenter.position : transform.position;
        Gizmos.color = new Color(0.95f, 0.55f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(center, leaveRadius);
    }
}
