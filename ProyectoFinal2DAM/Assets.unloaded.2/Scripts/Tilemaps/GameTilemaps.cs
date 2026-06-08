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
    public const string GroundTilemapName = "TM_Ground";
    public const string LaddersTilemapName = "TM_Ladders";
    public const string DecorationTilemapName = "TM_Decoration";

    public static GameTilemaps Instance { get; private set; }

    [Header("Referencias (vacío = busca hijos por nombre)")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap laddersTilemap;
    [SerializeField] private Tilemap decorationTilemap;

    private Grid _grid;

    public Grid Grid => _grid;
    public Tilemap Ground => groundTilemap;
    public Tilemap Ladders => laddersTilemap;
    public Tilemap Decoration => decorationTilemap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: hay más de un GameTilemaps. Solo debe haber uno activo.", this);
        }

        Instance = this;
        _grid = GetComponent<Grid>();

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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

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

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return _grid != null ? _grid.WorldToCell(worldPosition) : Vector3Int.zero;
    }

    public Vector3 CellCenterWorld(Vector3Int cell)
    {
        return _grid != null ? _grid.GetCellCenterWorld(cell) : Vector3.zero;
    }

    public bool HasGroundAt(Vector3 worldPosition)
    {
        return groundTilemap != null && groundTilemap.HasTile(groundTilemap.WorldToCell(worldPosition));
    }

    public bool HasLadderAt(Vector3 worldPosition)
    {
        return laddersTilemap != null && laddersTilemap.HasTile(laddersTilemap.WorldToCell(worldPosition));
    }

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
