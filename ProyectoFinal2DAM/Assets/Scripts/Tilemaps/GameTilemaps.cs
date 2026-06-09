using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Registro central de los Tilemaps del nivel. Pinta solo en TM_Ground, TM_Ladders y TM_Decoration;
/// el resto de scripts pueden consultar celdas por mundo con <see cref="HasGroundAt"/>, etc.
/// Coloca este componente en el mismo GameObject que tiene <see cref="Grid"/>.
/// </summary>
/// <remarks>
/// <b>Rampas / diagonales:</b> si al pintar en diagonal ves "escalones", casi siempre es porque el tile usa
/// <b>Collider Type = Grid</b> (caja por celda). Usa <b>Sprite</b> y recorta la forma en el Sprite Editor
/// (Custom Physics Shape), o usa <b>2D Sprite Shape</b> para pendientes largas.
/// Menú: <c>Tools / Proyecto Final / Rampas / …</c>
/// </remarks>
[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class GameTilemaps : MonoBehaviour
{
    // Nombres convenidos de los GameObjects de cada Tilemap (usados para buscarlos por nombre).
    public const string GroundTilemapName = "TM_Ground";
    public const string LaddersTilemapName = "TM_Ladders";
    public const string DecorationTilemapName = "TM_Decoration";

    // Acceso global de solo lectura a la instancia activa (singleton ligero).
    public static GameTilemaps Instance { get; private set; }

    [Header("Referencias (vacío = busca hijos por nombre)")]
    // Tilemaps asignables en el Inspector; si quedan vacíos se autocompletan en Awake().
    [SerializeField] private Tilemap groundTilemap;      // Suelo / colisiones.
    [SerializeField] private Tilemap laddersTilemap;     // Escaleras (trigger).
    [SerializeField] private Tilemap decorationTilemap;  // Decoración (sin colisión).

    // Grid del que cuelgan los Tilemaps; cachéado en Awake().
    private Grid _grid;

    // Propiedades públicas de solo lectura para que otros scripts accedan a los componentes.
    public Grid Grid => _grid;
    public Tilemap Ground => groundTilemap;
    public Tilemap Ladders => laddersTilemap;
    public Tilemap Decoration => decorationTilemap;

    /// <summary>
    /// Registra la instancia, cachea el Grid y resuelve las referencias de Tilemaps que
    /// no se hayan asignado en el Inspector buscándolas entre los hijos por nombre.
    /// </summary>
    private void Awake()
    {
        // Aviso si hubiera más de un GameTilemaps activo en la escena.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: hay más de un GameTilemaps. Solo debe haber uno activo.", this);
        }

        Instance = this;
        _grid = GetComponent<Grid>();

        // Autocompletado de referencias: solo si el campo está vacío.
        if (groundTilemap == null)
        {
            groundTilemap = FindTilemapByName(GroundTilemapName);
        }

        if (laddersTilemap == null)
        {
            laddersTilemap = FindTilemapByName(LaddersTilemapName);
        }

        if (decorationTilemap == null)
        {
            decorationTilemap = FindTilemapByName(DecorationTilemapName);
        }
    }

    /// <summary>Libera la referencia estática cuando se destruye esta instancia.</summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Busca recursivamente entre los hijos (incluidos inactivos) un GameObject con el
    /// nombre indicado que tenga un componente <see cref="Tilemap"/>.
    /// </summary>
    /// <returns>El Tilemap encontrado o null si no existe.</returns>
    private Tilemap FindTilemapByName(string objectName)
    {
        var transforms = GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (t.name == objectName)
            {
                var tm = t.GetComponent<Tilemap>();
                if (tm != null)
                {
                    return tm;
                }
            }
        }

        return null;
    }

    /// <summary>Convierte una posición de mundo a coordenada de celda del Grid.</summary>
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return _grid != null ? _grid.WorldToCell(worldPosition) : Vector3Int.zero;
    }

    /// <summary>Devuelve el centro en mundo de la celda indicada.</summary>
    public Vector3 CellCenterWorld(Vector3Int cell)
    {
        return _grid != null ? _grid.GetCellCenterWorld(cell) : Vector3.zero;
    }

    /// <summary>Indica si hay un tile de suelo en la posición de mundo dada.</summary>
    public bool HasGroundAt(Vector3 worldPosition)
    {
        return groundTilemap != null && groundTilemap.HasTile(groundTilemap.WorldToCell(worldPosition));
    }

    /// <summary>Indica si hay un tile de escalera en la posición de mundo dada.</summary>
    public bool HasLadderAt(Vector3 worldPosition)
    {
        return laddersTilemap != null && laddersTilemap.HasTile(laddersTilemap.WorldToCell(worldPosition));
    }

    /// <summary>Indica si hay un tile de decoración en la posición de mundo dada.</summary>
    public bool HasDecorationAt(Vector3 worldPosition)
    {
        return decorationTilemap != null && decorationTilemap.HasTile(decorationTilemap.WorldToCell(worldPosition));
    }

    /// <summary>LayerMask solo con la capa física "Ground" (para overlaps / casts genéricos).</summary>
    public static LayerMask GetGroundLayerMask()
    {
        var layer = LayerMask.NameToLayer("Ground");
        return layer >= 0 ? 1 << layer : default;
    }

    /// <summary>LayerMask de la capa "Ladders" (triggers de escalera).</summary>
    public static LayerMask GetLaddersLayerMask()
    {
        var layer = LayerMask.NameToLayer("Ladders");
        return layer >= 0 ? 1 << layer : default;
    }
}
