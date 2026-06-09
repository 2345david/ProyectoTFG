// Todo el archivo es código de Editor: solo se compila dentro del editor de Unity.
#if UNITY_EDITOR
using UnityEditor;                      // API del editor (MenuItem, PrefabUtility, SerializedObject...).
using UnityEditor.SceneManagement;      // MarkSceneDirty para señalar cambios en la escena.
using UnityEngine;
using UnityEngine.Tilemaps;             // Grid, Tilemap, TilemapRenderer, TilemapCollider2D.

/// <summary>
/// Herramienta de Editor que monta de un golpe la jerarquía de tilemaps del nivel:
/// un <c>Grid</c> raíz con <c>TM_Ground</c> (suelo sólido fusionado por CompositeCollider2D),
/// <c>TM_Ladders</c> (escaleras como trigger) y <c>TM_Decoration</c> (solo arte, sin colisión).
/// Puede crearla en la escena activa o guardarla como prefab. Menú: <c>Tools/Proyecto Final/…</c>.
/// </summary>
public static class TilemapGameSystemSetup
{
    // Ruta de destino del prefab generado (correcta tras la reorganización; no modificar).
    private const string PrefabPath = "Assets/Prefabs/WorldTilemaps.prefab";

    /// <summary>
    /// Crea la jerarquía completa de tilemaps en la escena activa, la selecciona, la registra
    /// para deshacer y marca la escena como modificada. Imprime ayuda de uso en consola.
    /// </summary>
    [MenuItem("Tools/Proyecto Final/Crear sistema Tilemap (Ground / Escaleras / Decoración)")]
    public static void CreateInScene()
    {
        // Construye el objeto raíz con sus tres tilemaps hijos.
        var root = BuildWorldTilemapsObject();
        Undo.RegisterCreatedObjectUndo(root, "World Tilemaps");
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log(
            "Tilemap: pinta en TM_Ground (suelo), TM_Ladders (escalera, trigger) y TM_Decoration (sin colisión). " +
            "Añade 'Player Ladder Movement' al jugador y revisa que whatIsGround incluya la capa Ground. " +
            "Rampas: Tools → Proyecto Final → Rampas (evitar collider Grid en tiles en diagonal).",
            root);
    }

    /// <summary>
    /// Construye la jerarquía de tilemaps y la guarda como prefab en <see cref="PrefabPath"/>,
    /// destruyendo después el objeto temporal de la escena.
    /// </summary>
    [MenuItem("Tools/Proyecto Final/Guardar prefab WorldTilemaps")]
    public static void SavePrefab()
    {
        var root = BuildWorldTilemapsObject();
        try
        {
            // Asegura la carpeta de destino, guarda el prefab y lo resalta en el Project.
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            EditorGUIUtility.PingObject(asset);
            Debug.Log($"Prefab guardado: {PrefabPath}", asset);
        }
        finally
        {
            // El objeto temporal de escena ya no se necesita (el prefab queda en disco).
            Object.DestroyImmediate(root);
        }
    }

    /// <summary>
    /// Crea el GameObject raíz "WorldTilemaps" con Grid, Rigidbody2D estático y
    /// CompositeCollider2D, le añade el registro <see cref="GameTilemaps"/> y cuelga
    /// los tres tilemaps hijos (suelo, escaleras y decoración).
    /// </summary>
    private static GameObject BuildWorldTilemapsObject()
    {
        // Raíz en la capa "Ground" (si existe en el proyecto).
        var root = new GameObject("WorldTilemaps");
        var groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
        {
            root.layer = groundLayer;
        }

        // Grid con celdas de 1x1 unidad.
        var grid = root.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        // Rigidbody2D estático: necesario para que el CompositeCollider2D funcione como suelo fijo.
        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        // Composite que fusiona los colliders del suelo, y registro central de tilemaps.
        root.AddComponent<CompositeCollider2D>();
        root.AddComponent<GameTilemaps>();

        // Crea los tres tilemaps como hijos del Grid.
        CreateGroundTilemap(root.transform);
        CreateLaddersTilemap(root.transform);
        CreateDecorationTilemap(root.transform);

        return root;
    }

    /// <summary>
    /// Crea la capa del suelo (TM_Ground): es la parte sólida con la que se choca al caminar.
    /// </summary>
    private static void CreateGroundTilemap(Transform parent)
    {
        var go = new GameObject(GameTilemaps.GroundTilemapName);
        go.transform.SetParent(parent, false);
        ApplyLayer(go, "Ground");

        // La capa de azulejos, colocados en el centro de cada casilla.
        var tm = go.AddComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        // Se dibuja al fondo (el número 0 indica que va detrás de las demás capas).
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 0;

        // Le damos forma física sólida (no atravesable) y la juntamos con la del objeto principal.
        var col = go.AddComponent<TilemapCollider2D>();
        SetUsedByComposite(col, true);
        col.isTrigger = false;
    }

    /// <summary>
    /// Crea la capa de escaleras (TM_Ladders): se puede atravesar, pero avisa cuando el jugador la toca.
    /// </summary>
    private static void CreateLaddersTilemap(Transform parent)
    {
        var go = new GameObject(GameTilemaps.LaddersTilemapName);
        go.transform.SetParent(parent, false);
        ApplyLayer(go, "Ladders");

        var tm = go.AddComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        // Se dibuja por delante del suelo (número 2).
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 2;

        // Forma "detectora" (trigger): no frena al jugador, solo avisa de que está en la escalera.
        var col = go.AddComponent<TilemapCollider2D>();
        SetUsedByComposite(col, false);
        col.isTrigger = true;
    }

    /// <summary>
    /// Crea la capa de decoración (TM_Decoration): solo adorno bonito, no choca con nada.
    /// </summary>
    private static void CreateDecorationTilemap(Transform parent)
    {
        var go = new GameObject(GameTilemaps.DecorationTilemapName);
        go.transform.SetParent(parent, false);
        ApplyLayer(go, "Foreground");

        var tm = go.AddComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        // Se dibuja delante de todo lo demás (número 8).
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 8;
    }

    /// <summary>Pone un objeto en la capa que se le indique, si esa capa existe en el proyecto.</summary>
    private static void ApplyLayer(GameObject go, string layerName)
    {
        var id = LayerMask.NameToLayer(layerName);
        if (id >= 0)
        {
            go.layer = id;
        }
    }

    /// <summary>
    /// Enciende o apaga la opción "Used By Composite" de una capa (es la que dice si su forma
    /// física se junta con la del objeto principal). Se hace por un camino especial porque
    /// esa opción no se puede tocar de la forma normal.
    /// </summary>
    private static void SetUsedByComposite(TilemapCollider2D col, bool used)
    {
        var so = new SerializedObject(col);
        var prop = so.FindProperty("m_UsedByComposite");
        if (prop != null)
        {
            prop.boolValue = used;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
