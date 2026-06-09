// Todo el archivo es código de Editor: solo se compila dentro del editor de Unity.
#if UNITY_EDITOR
using System.IO;                        // Manejo de rutas y carpetas (File, Directory, Path).
using UnityEditor;                      // API del editor (MenuItem, PrefabUtility, SerializedObject...).
using UnityEditor.SceneManagement;      // MarkSceneDirty para señalar cambios en la escena.
using UnityEngine;

/// <summary>
/// Herramienta de Editor que genera/coloca un GameObject "MetroidvaniaSpawnZone" con un
/// BoxCollider2D (trigger) y el componente <c>MetroidvaniaSpawnZone</c> ya preconfigurado.
/// Disponible en el menú <c>Tools/Proyecto Final/…</c>.
/// </summary>
public static class MetroidvaniaSpawnZonePrefabCreator
{
    // Ruta de destino del prefab y prefab de enemigo por defecto (ya correctas tras la reorganización).
    private const string PrefabPath = "Assets/Prefabs/MetroidvaniaSpawnZone.prefab";
    private const string DefaultEnemyPrefabPath = "Assets/Prefabs/Enemy.prefab";

    /// <summary>
    /// Crea (o sobrescribe, previa confirmación) el prefab de la zona de spawn en <see cref="PrefabPath"/>.
    /// </summary>
    [MenuItem("Tools/Proyecto Final/Crear prefab MetroidvaniaSpawnZone")]
    public static void CreateOrUpdatePrefab()
    {
        // Si ya existe el prefab, pide confirmación antes de sobrescribirlo.
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

        // Asegura que la carpeta de destino exista.
        var directory = Path.GetDirectoryName(PrefabPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Construye el GameObject temporal, lo guarda como prefab y lo selecciona/resalta.
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
            // El objeto temporal de escena ya no se necesita (el prefab queda en disco).
            Object.DestroyImmediate(root);
        }
    }

    /// <summary>
    /// Instancia la zona de spawn directamente en la escena activa (sin crear prefab),
    /// la registra para deshacer y la encuadra en la vista de escena.
    /// </summary>
    [MenuItem("Tools/Proyecto Final/Colocar MetroidvaniaSpawnZone en escena activa")]
    public static void PlaceInActiveScene()
    {
        var root = BuildSpawnZoneGameObject();
        Undo.RegisterCreatedObjectUndo(root, "MetroidvaniaSpawnZone");
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        // Centra la cámara de la vista de escena sobre el objeto recién creado.
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }

        Debug.Log(
            "MetroidvaniaSpawnZone colocado en la escena. Asigna más prefabs en Enemy Types si quieres variedad.",
            root);
    }

    /// <summary>
    /// Arma el objeto "MetroidvaniaSpawnZone" con su área detectora y le pone todos sus
    /// ajustes con valores por defecto sensatos.
    /// </summary>
    private static GameObject BuildSpawnZoneGameObject()
    {
        // Objeto principal, puesto en la capa "Default".
        var root = new GameObject("MetroidvaniaSpawnZone");
        root.layer = LayerMask.NameToLayer("Default");

        // Área detectora (trigger): marca el espacio dentro del cual aparecerán los enemigos.
        var col = root.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(22f, 14f);
        col.offset = Vector2.zero;

        // El script que maneja la aparición de enemigos; lo ajustamos por un camino especial
        // que permite tocar también sus ajustes ocultos.
        var zone = root.AddComponent<MetroidvaniaSpawnZone>();
        var so = new SerializedObject(zone);

        // Valores por defecto: cuántos enemigos salen (mínimo y máximo), tamaño de la zona,
        // cuándo empiezan a salir y a qué ritmo lo hacen.
        so.FindProperty("spawnCountMin").intValue = 3;
        so.FindProperty("spawnCountMax").intValue = 6;
        so.FindProperty("regionSize").vector2Value = new Vector2(18f, 8f);
        so.FindProperty("initialSpawnTrigger").enumValueIndex =
            (int)MetroidvaniaSpawnZone.InitialSpawnTrigger.OnPlayerFirstTriggerEnter;
        so.FindProperty("delayAfterSceneStart").floatValue = 0f;
        so.FindProperty("initialSpawnPace").enumValueIndex = (int)MetroidvaniaSpawnZone.SpawnPace.Bursts;
        so.FindProperty("respawnSpawnPace").enumValueIndex = (int)MetroidvaniaSpawnZone.SpawnPace.Instant;

        // Si existe el enemigo por defecto, lo ponemos como único tipo de enemigo de la zona.
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

        // Guardamos los ajustes en el objeto (sin apuntarlo en deshacer, porque aún se está creando).
        so.ApplyModifiedPropertiesWithoutUndo();
        return root;
    }
}
#endif
