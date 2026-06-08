using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zona de spawn estilo metroidvania: varios prefabs ponderados, primera oleada al cargar o al entrar el jugador,
/// y respawn cuando la sala deja de ser visible por la cámara (el jugador no "ve" esa zona).
/// </summary>
[DisallowMultipleComponent]
public class MetroidvaniaSpawnZone : MonoBehaviour
{
    public enum DistributionMode
    {
        RandomUniform,
        Grid
    }

    public enum SpawnPace
    {
        Instant,
        OneByOne,
        Bursts
    }

    public enum InitialSpawnTrigger
    {
        /// <summary>Oleada al iniciar la escena (tras un delay opcional).</summary>
        OnSceneStart,

        /// <summary>La primera vez que el jugador entra en un Collider2D trigger de este mismo GameObject.</summary>
        OnPlayerFirstTriggerEnter
    }

    [System.Serializable]
    public struct WeightedEnemyPrefab
    {
        public GameObject prefab;
        [Min(0.01f)] public float weight;
    }

    [Header("Tipos de enemigo (ponderado)")]
    [SerializeField] private List<WeightedEnemyPrefab> enemyTypes = new List<WeightedEnemyPrefab>();

    [Header("Oleada: cantidad y posiciones")]
    [SerializeField] private int spawnCountMin = 3;
    [SerializeField] private int spawnCountMax = 5;
    [SerializeField] private DistributionMode distribution = DistributionMode.RandomUniform;
    [Tooltip("Rectángulo de aparición en espacio local del objeto.")]
    [SerializeField] private Vector2 regionSize = new Vector2(18f, 8f);

    [Header("Suelo 2D")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float raycastFromYOffset = 10f;
    [SerializeField] private float raycastMaxDistance = 40f;
    [SerializeField] private float groundSurfaceYOffset = 0.05f;

    [Header("Separación entre spawns")]
    [SerializeField] private float minSeparation = 0.75f;
    [SerializeField] private int maxAttemptsPerPoint = 40;

    [Header("Primera oleada (inicio del juego / primera visita)")]
    [SerializeField] private InitialSpawnTrigger initialSpawnTrigger = InitialSpawnTrigger.OnSceneStart;
    [SerializeField] private float delayAfterSceneStart;
    [SerializeField] private SpawnPace initialSpawnPace = SpawnPace.Bursts;
    [SerializeField] private float initialPaceInitialDelay = 0.25f;
    [SerializeField] private float initialSecondsBetweenSpawns = 0.35f;
    [SerializeField] private int initialBurstSize = 2;
    [SerializeField] private float initialSecondsBetweenBursts = 0.6f;
    [SerializeField] private float initialDelayJitter;
    [SerializeField] private bool initialUseUnscaledTime;

    [Header("Mientras juegas: respawn al no ver la zona")]
    [SerializeField] private bool respawnWhenZoneNotVisible = true;
    [Tooltip("Segundos seguidos con la zona fuera de cámara antes de respawn.")]
    [SerializeField] private float secondsHiddenBeforeRespawn = 1.75f;
    [SerializeField] private float respawnCooldown = 4f;
    [Tooltip("Si está activo, no hace respawn completo hasta que no quede ningún enemigo vivo de esta oleada.")]
    [SerializeField] private bool requireAllDefeatedBeforeRespawn;
    [SerializeField] private SpawnPace respawnSpawnPace = SpawnPace.Instant;
    [SerializeField] private float respawnPaceInitialDelay;
    [SerializeField] private float respawnSecondsBetweenSpawns = 0.2f;
    [SerializeField] private int respawnBurstSize = 3;
    [SerializeField] private float respawnSecondsBetweenBursts = 0.4f;
    [SerializeField] private float respawnDelayJitter;
    [SerializeField] private bool respawnUseUnscaledTime;

    [Header("Visibilidad (cámara ortográfica 2D)")]
    [SerializeField] private Camera gameplayCamera;
    [Tooltip("Infla los bounds de la cámara al comprobar intersección (evita respawn al borde del encuadre).")]
    [SerializeField] private float cameraBoundsPadding = 1.25f;
    [Tooltip("Si no hay cámara ortográfica: distancia del jugador al rectángulo de la zona por debajo de la cual se considera 'mirando'.")]
    [SerializeField] private float fallbackPlayerNearZoneDistance = 14f;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform spawnParent;

    private readonly List<GameObject> _spawned = new List<GameObject>();
    private Coroutine _spawnRoutine;
    private bool _initialWaveScheduledOrDone;
    private float _hiddenTimer;
    private float _lastRespawnTime = -999f;
    private bool _wasVisible = true;

    /// <summary>True mientras una corrutina de spawn está en curso.</summary>
    public bool IsSpawning { get; private set; }

    private void Awake()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerTransform = p.transform;
            }
        }

        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (initialSpawnTrigger != InitialSpawnTrigger.OnSceneStart)
        {
            return;
        }

        if (delayAfterSceneStart > 0f)
        {
            StartCoroutine(SceneStartDelayedSpawn());
        }
        else
        {
            BeginInitialWave();
        }
    }

    private IEnumerator SceneStartDelayedSpawn()
    {
        yield return new WaitForSeconds(delayAfterSceneStart);
        BeginInitialWave();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (initialSpawnTrigger != InitialSpawnTrigger.OnPlayerFirstTriggerEnter)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        BeginInitialWave();
    }

    private void Update()
    {
        if (!respawnWhenZoneNotVisible || !_initialWaveScheduledOrDone)
        {
            return;
        }

        if (IsSpawning)
        {
            return;
        }

        var visible = IsZoneVisibleToPlayerCamera();
        if (visible)
        {
            _hiddenTimer = 0f;
            _wasVisible = true;
            return;
        }

        if (_wasVisible)
        {
            _hiddenTimer = 0f;
            _wasVisible = false;
        }

        _hiddenTimer += Time.deltaTime;
        if (_hiddenTimer < secondsHiddenBeforeRespawn)
        {
            return;
        }

        if (Time.time - _lastRespawnTime < respawnCooldown)
        {
            return;
        }

        if (requireAllDefeatedBeforeRespawn && !AllTrackedEnemiesDefeated())
        {
            return;
        }

        _hiddenTimer = 0f;
        _lastRespawnTime = Time.time;
        StartRespawnWave();
    }

    private void OnDisable()
    {
        StopSpawnRoutine();
        IsSpawning = false;
    }

    /// <summary>Fuerza una oleada nueva (respeta cooldown si <paramref name="ignoreCooldown"/> es false).</summary>
    public void ForceRespawnWave(bool ignoreCooldown = true)
    {
        if (!ignoreCooldown && Time.time - _lastRespawnTime < respawnCooldown)
        {
            return;
        }

        _lastRespawnTime = Time.time;
        StartRespawnWave();
    }

    /// <summary>Destruye todos los enemigos generados por esta zona.</summary>
    public void ClearSpawnedEnemies()
    {
        StopSpawnRoutine();
        PurgeDestroyedFromList();
        for (var i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null)
            {
                Destroy(_spawned[i]);
            }
        }

        _spawned.Clear();
    }

    private void BeginInitialWave()
    {
        if (_initialWaveScheduledOrDone)
        {
            return;
        }

        if (!ValidateEnemyTable())
        {
            return;
        }

        _initialWaveScheduledOrDone = true;
        StartWaveInternal(isRespawn: false);
    }

    private void StartRespawnWave()
    {
        if (!ValidateEnemyTable())
        {
            return;
        }

        StopSpawnRoutine();
        ClearSpawnedEnemies();
        StartWaveInternal(isRespawn: true);
    }

    private void StartWaveInternal(bool isRespawn)
    {
        var count = Random.Range(
            Mathf.Min(spawnCountMin, spawnCountMax),
            Mathf.Max(spawnCountMin, spawnCountMax) + 1);
        var positions = BuildPositions(count);
        if (positions.Count == 0)
        {
            return;
        }

        var parent = spawnParent != null ? spawnParent : transform;
        var pace = isRespawn ? respawnSpawnPace : initialSpawnPace;
        var id = isRespawn ? 1 : 0;
        _spawnRoutine = StartCoroutine(SpawnWaveCoroutine(positions, parent, pace, id));
    }

    private IEnumerator SpawnWaveCoroutine(
        IReadOnlyList<Vector2> positions,
        Transform parent,
        SpawnPace pace,
        int configId)
    {
        IsSpawning = true;
        var initialDelay = configId == 0 ? initialPaceInitialDelay : respawnPaceInitialDelay;
        var betweenOne = configId == 0 ? initialSecondsBetweenSpawns : respawnSecondsBetweenSpawns;
        var burst = Mathf.Max(1, configId == 0 ? initialBurstSize : respawnBurstSize);
        var betweenBurst = configId == 0 ? initialSecondsBetweenBursts : respawnSecondsBetweenBursts;
        var jitter = configId == 0 ? initialDelayJitter : respawnDelayJitter;
        var unscaled = configId == 0 ? initialUseUnscaledTime : respawnUseUnscaledTime;

        if (initialDelay > 0f)
        {
            yield return JitteredDelay(initialDelay, jitter, unscaled);
        }

        if (pace == SpawnPace.Instant)
        {
            foreach (var p in positions)
            {
                SpawnOneEnemyAt(p, parent);
            }

            FinishSpawnRoutine();
            yield break;
        }

        var index = 0;
        var total = positions.Count;
        if (pace == SpawnPace.OneByOne)
        {
            while (index < total)
            {
                SpawnOneEnemyAt(positions[index], parent);
                index++;
                if (index < total && betweenOne > 0f)
                {
                    yield return JitteredDelay(betweenOne, jitter, unscaled);
                }
            }
        }
        else
        {
            while (index < total)
            {
                var end = Mathf.Min(index + burst, total);
                for (; index < end; index++)
                {
                    SpawnOneEnemyAt(positions[index], parent);
                }

                if (index < total && betweenBurst > 0f)
                {
                    yield return JitteredDelay(betweenBurst, jitter, unscaled);
                }
            }
        }

        FinishSpawnRoutine();
    }

    private void FinishSpawnRoutine()
    {
        _spawnRoutine = null;
        IsSpawning = false;
    }

    private void StopSpawnRoutine()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        IsSpawning = false;
    }

    private static object JitteredDelay(float seconds, float jitter, bool unscaled)
    {
        var t = Mathf.Max(0f, seconds + Random.Range(0f, jitter));
        return unscaled ? new WaitForSecondsRealtime(t) : new WaitForSeconds(t);
    }

    private void SpawnOneEnemyAt(Vector2 horizontal, Transform parent)
    {
        var prefab = PickWeightedPrefab();
        if (prefab == null)
        {
            return;
        }

        var world = SnapIfNeeded(horizontal);
        var instance = Instantiate(prefab, world, Quaternion.identity, parent);
        _spawned.Add(instance);
    }

    private GameObject PickWeightedPrefab()
    {
        float sum = 0f;
        for (var i = 0; i < enemyTypes.Count; i++)
        {
            if (enemyTypes[i].prefab != null)
            {
                sum += enemyTypes[i].weight;
            }
        }

        if (sum <= 0f)
        {
            return null;
        }

        var r = Random.Range(0f, sum);
        for (var i = 0; i < enemyTypes.Count; i++)
        {
            var e = enemyTypes[i];
            if (e.prefab == null)
            {
                continue;
            }

            r -= e.weight;
            if (r <= 0f)
            {
                return e.prefab;
            }
        }

        return enemyTypes[enemyTypes.Count - 1].prefab;
    }

    private bool ValidateEnemyTable()
    {
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogWarning($"{name}: añade al menos un prefab en Enemy Types.", this);
            return false;
        }

        for (var i = 0; i < enemyTypes.Count; i++)
        {
            if (enemyTypes[i].prefab != null)
            {
                return true;
            }
        }

        Debug.LogWarning($"{name}: todos los prefabs en Enemy Types son nulos.", this);
        return false;
    }

    private bool AllTrackedEnemiesDefeated()
    {
        PurgeDestroyedFromList();
        return _spawned.Count == 0;
    }

    private void PurgeDestroyedFromList()
    {
        for (var i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] == null)
            {
                _spawned.RemoveAt(i);
            }
        }
    }

    private bool IsZoneVisibleToPlayerCamera()
    {
        var zoneBounds = GetSpawnZoneWorldBounds2D();

        if (gameplayCamera != null && gameplayCamera.orthographic)
        {
            var camBounds = GetOrthographicCameraWorldBounds(gameplayCamera);
            camBounds.Expand(cameraBoundsPadding);
            return camBounds.Intersects(zoneBounds);
        }

        if (playerTransform != null)
        {
            var closest = zoneBounds.ClosestPoint(playerTransform.position);
            return Vector2.Distance(closest, playerTransform.position) < fallbackPlayerNearZoneDistance;
        }

        return true;
    }

    private Bounds GetOrthographicCameraWorldBounds(Camera cam)
    {
        var height = cam.orthographicSize * 2f;
        var width = height * cam.aspect;
        var c = cam.transform.position;
        return new Bounds(c, new Vector3(width, height, 50f));
    }

    private Bounds GetSpawnZoneWorldBounds2D()
    {
        var hw = regionSize.x * 0.5f;
        var hh = regionSize.y * 0.5f;
        var b = new Bounds(transform.TransformPoint(new Vector3(-hw, -hh, 0f)), Vector3.zero);
        b.Encapsulate(transform.TransformPoint(new Vector3(hw, -hh, 0f)));
        b.Encapsulate(transform.TransformPoint(new Vector3(-hw, hh, 0f)));
        b.Encapsulate(transform.TransformPoint(new Vector3(hw, hh, 0f)));
        // Intersección con la cámara: grosor en Z para que 3D Bounds.Intersects sea fiable en 2D.
        var zRef = gameplayCamera != null ? gameplayCamera.transform.position.z : b.center.z;
        var c = b.center;
        c.z = zRef;
        b.center = c;
        b.extents = new Vector3(Mathf.Max(b.extents.x, 0.05f), Mathf.Max(b.extents.y, 0.05f), 80f);
        return b;
    }

    private List<Vector2> BuildPositions(int count)
    {
        var list = new List<Vector2>(count);
        if (count <= 0)
        {
            return list;
        }

        if (distribution == DistributionMode.Grid)
        {
            FillGrid(list, count);
            return list;
        }

        FillRandom(list, count);
        return list;
    }

    private void FillGrid(List<Vector2> list, int count)
    {
        var cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        var rows = Mathf.CeilToInt(count / (float)cols);
        var halfW = regionSize.x * 0.5f;
        var halfH = regionSize.y * 0.5f;
        var stepX = cols > 1 ? regionSize.x / (cols - 1) : 0f;
        var stepY = rows > 1 ? regionSize.y / (rows - 1) : 0f;

        var index = 0;
        for (var r = 0; r < rows && index < count; r++)
        {
            for (var c = 0; c < cols && index < count; c++)
            {
                var localX = cols > 1 ? -halfW + c * stepX : 0f;
                var localY = rows > 1 ? -halfH + r * stepY : 0f;
                list.Add(LocalToWorld(new Vector2(localX, localY)));
                index++;
            }
        }
    }

    private void FillRandom(List<Vector2> list, int count)
    {
        var useSeparation = minSeparation > 0.01f;
        for (var i = 0; i < count; i++)
        {
            Vector2 chosen = default;
            var ok = false;
            for (var attempt = 0; attempt < maxAttemptsPerPoint; attempt++)
            {
                chosen = RandomPointInRegion();
                if (!useSeparation || IsFarEnough(chosen, list))
                {
                    ok = true;
                    break;
                }
            }

            if (!ok)
            {
                chosen = RandomPointInRegion();
            }

            list.Add(chosen);
        }
    }

    private bool IsFarEnough(Vector2 candidate, List<Vector2> existing)
    {
        var minSqr = minSeparation * minSeparation;
        foreach (var p in existing)
        {
            if ((p - candidate).sqrMagnitude < minSqr)
            {
                return false;
            }
        }

        return true;
    }

    private Vector2 RandomPointInRegion()
    {
        var halfW = regionSize.x * 0.5f;
        var halfH = regionSize.y * 0.5f;
        var local = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
        return LocalToWorld(local);
    }

    private Vector2 LocalToWorld(Vector2 local)
    {
        var w = transform.TransformPoint(new Vector3(local.x, local.y, 0f));
        return new Vector2(w.x, w.y);
    }

    private Vector2 SnapIfNeeded(Vector2 horizontal)
    {
        if (!snapToGround)
        {
            return horizontal;
        }

        var origin = new Vector2(horizontal.x, horizontal.y + raycastFromYOffset);
        var hit = Physics2D.Raycast(origin, Vector2.down, raycastMaxDistance, groundLayers);
        if (hit.collider != null)
        {
            return new Vector2(horizontal.x, hit.point.y + groundSurfaceYOffset);
        }

        return horizontal;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.2f, 0.85f, 0.35f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(regionSize.x, regionSize.y, 0f));

        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = IsZoneVisibleToPlayerCamera()
            ? new Color(0.2f, 0.6f, 1f, 0.35f)
            : new Color(1f, 0.3f, 0.2f, 0.35f);
        var b = GetSpawnZoneWorldBounds2D();
        Gizmos.DrawCube(b.center, b.size);
    }
}
