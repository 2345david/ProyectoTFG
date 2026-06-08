#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Crea Grid + TM_Ground (sólido + composite) + TM_Ladders (trigger) + TM_Decoration (solo arte).
/// </summary>
public static class TilemapGameSystemSetup
{
    private const string PrefabPath = "Assets/Prefab/WorldTilemaps.prefab";

    [MenuItem("Tools/Proyecto Final/Crear sistema Tilemap (Ground / Escaleras / Decoración)")]
    public static void CreateInScene()
    {
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

    [MenuItem("Tools/Proyecto Final/Guardar prefab WorldTilemaps")]
    public static void SavePrefab()
    {
        var root = BuildWorldTilemapsObject();
        try
        {
            System.IO.Directory.CreateDirectory("Assets/Prefab");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            EditorGUIUtility.PingObject(asset);
            Debug.Log($"Prefab guardado: {PrefabPath}", asset);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static GameObject BuildWorldTilemapsObject()
    {
        var root = new GameObject("WorldTilemaps");
        var groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
        {
            root.layer = groundLayer;
        }

        var grid = root.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        root.AddComponent<CompositeCollider2D>();
        root.AddComponent<GameTilemaps>();

        CreateGroundTilemap(root.transform);
        CreateLaddersTilemap(root.transform);
        CreateDecorationTilemap(root.transform);

        return root;
    }

    private static void CreateGroundTilemap(Transform parent)
    {
        var go = new GameObject(GameTilemaps.GroundTilemapName);
        go.transform.SetParent(parent, false);
        ApplyLayer(go, "Ground");

        var tm = go.AddComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 0;

        var col = go.AddComponent<TilemapCollider2D>();
        SetUsedByComposite(col, true);
        col.isTrigger = false;
    }

    private static void CreateLaddersTilemap(Transform parent)
    {
        var go = new GameObject(GameTilemaps.LaddersTilemapName);
        go.transform.SetParent(parent, false);
        ApplyLayer(go, "Ladders");

        var tm = go.AddComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 2;

        var col = go.AddComponent<TilemapCollider2D>();
        SetUsedByComposite(col, false);
        col.isTrigger = true;
    }

    private static void CreateDecorationTilemap(Transform parent)
    {
        var go = new GameObject(GameTilemaps.DecorationTilemapName);
        go.transform.SetParent(parent, false);
        ApplyLayer(go, "Foreground");

        var tm = go.AddComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 8;
    }

    private static void ApplyLayer(GameObject go, string layerName)
    {
        var id = LayerMask.NameToLayer(layerName);
        if (id >= 0)
        {
            go.layer = id;
        }
    }

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
