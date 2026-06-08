#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Genera el prefab GameObject con BoxCollider2D (trigger) + MetroidvaniaSpawnZone ya configurado.
/// </summary>
public static class MetroidvaniaSpawnZonePrefabCreator
{
    private const string PrefabPath = "Assets/Prefab/MetroidvaniaSpawnZone.prefab";
    private const string DefaultEnemyPrefabPath = "Assets/Prefab/Enemy.prefab";

    [MenuItem("Tools/Proyecto Final/Crear prefab MetroidvaniaSpawnZone")]
    public static void CreateOrUpdatePrefab()
    {
        if (File.Exists(PrefabPath))
        {
            if (!EditorUtility.DisplayDialog(
                    "MetroidvaniaSpawnZone",
                    $"Ya existe el prefab en:\n{PrefabPath}\n¿Sobrescribirlo?",
                    "Sí",
                    "Cancelar"))
            {
                return;
            }
        }

        var directory = Path.GetDirectoryName(PrefabPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var root = BuildSpawnZoneGameObject();
        try
        {
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
            Debug.Log($"MetroidvaniaSpawnZone: prefab guardado en {PrefabPath}", asset);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [MenuItem("Tools/Proyecto Final/Colocar MetroidvaniaSpawnZone en escena activa")]
    public static void PlaceInActiveScene()
    {
        var root = BuildSpawnZoneGameObject();
        Undo.RegisterCreatedObjectUndo(root, "MetroidvaniaSpawnZone");
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }

        Debug.Log(
            "MetroidvaniaSpawnZone colocado en la escena. Asigna más prefabs en Enemy Types si quieres variedad.",
            root);
    }

    private static GameObject BuildSpawnZoneGameObject()
    {
        var root = new GameObject("MetroidvaniaSpawnZone");
        root.layer = LayerMask.NameToLayer("Default");

        var col = root.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(22f, 14f);
        col.offset = Vector2.zero;

        var zone = root.AddComponent<MetroidvaniaSpawnZone>();
        var so = new SerializedObject(zone);

        so.FindProperty("spawnCountMin").intValue = 3;
        so.FindProperty("spawnCountMax").intValue = 6;
        so.FindProperty("regionSize").vector2Value = new Vector2(18f, 8f);
        so.FindProperty("initialSpawnTrigger").enumValueIndex =
            (int)MetroidvaniaSpawnZone.InitialSpawnTrigger.OnPlayerFirstTriggerEnter;
        so.FindProperty("delayAfterSceneStart").floatValue = 0f;
        so.FindProperty("initialSpawnPace").enumValueIndex = (int)MetroidvaniaSpawnZone.SpawnPace.Bursts;
        so.FindProperty("respawnSpawnPace").enumValueIndex = (int)MetroidvaniaSpawnZone.SpawnPace.Instant;

        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultEnemyPrefabPath);
        var listProp = so.FindProperty("enemyTypes");
        listProp.ClearArray();
        if (enemyPrefab != null)
        {
            listProp.arraySize = 1;
            var el = listProp.GetArrayElementAtIndex(0);
            el.FindPropertyRelative("prefab").objectReferenceValue = enemyPrefab;
            el.FindPropertyRelative("weight").floatValue = 1f;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        return root;
    }
}
#endif
