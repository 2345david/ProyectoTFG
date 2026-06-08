using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Instancia un prefab de enemigo repartido por un rectángulo en el mapa (2D).
/// Puede soltarlos todos a la vez o ir apareciendo poco a poco (intervalos / ráfagas).
/// Para metroidvania (varios tipos, respawn al salir de cámara, primera visita), usa <see cref="MetroidvaniaSpawnZone"/>.
/// </summary>
public class EnemyMapSpawner : MonoBehaviour
{
    public enum DistributionMode
    {
        RandomUniform,
        Grid
    }

    public enum SpawnTiming
    {
        /// <summary>Todos en el mismo frame (comportamiento clásico).</summary>
        Instant,

        /// <summary>Un enemigo cada <see cref="secondsBetweenSpawns"/>.</summary>
        OneByOne,

        /// <summary><see cref="enemiesPerBurst"/> enemigos y luego espera <see cref="secondsBetweenBursts"/>.</summary>
        Bursts
    }

    [Header("Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Cantidad y modo")]
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private DistributionMode distribution = DistributionMode.RandomUniform;

    [Header("Ritmo de aparición")]
    [SerializeField] private SpawnTiming spawnTiming = SpawnTiming.OneByOne;
    [Tooltip("Segundos de espera antes del primer enemigo.")]
    [SerializeField] private float initialDelay = 1f;
    [Tooltip("Modo OneByOne: segundos entre cada enemigo.")]
    [SerializeField] private float secondsBetweenSpawns = 1.25f;
    [Tooltip("Modo Bursts: cuántos enemigos salen en cada ráfaga.")]
    [SerializeField] private int enemiesPerBurst = 2;
    [Tooltip("Modo Bursts: segundos entre ráfagas.")]
    [SerializeField] private float secondsBetweenBursts = 3f;
    [Tooltip("Aleatorio extra 0…jitter añadido a cada espera (más orgánico).")]
    [SerializeField] private float delayJitter = 0.15f;
    [Tooltip("Si está activo, los temporizadores usan tiempo real (ignoran Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime;

    [Header("Área (relativa a este Transform)")]
    [SerializeField] private Vector2 regionSize = new Vector2(24f, 6f);

    [Header("Suelo (opcional, 2D)")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float raycastFromYOffset = 8f;
    [SerializeField] private float raycastMaxDistance = 30f;
    [SerializeField] private float groundSurfaceYOffset = 0.05f;

    [Header("Separación")]
    [SerializeField] private float minSeparation = 0.6f;
    [SerializeField] private int maxAttemptsPerEnemy = 40;

    [Header("Jerarquía")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool clearPreviousOnSpawn = true;

    private readonly List<GameObject> _spawned = new List<GameObject>();
    private Coroutine _spawnRoutine;

    /// <summary>True mientras la corrutina de spawn gradual está en curso.</summary>
    public bool IsSpawningGradually { get; private set; }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }

    private void OnDisable()
    {
        StopSpawning();
    }

#if UNITY_EDITOR
    [ContextMenu("Generar enemigos ahora")]
    private void EditorSpawnNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("EnemyMapSpawner: usa Play Mode o llama SpawnEnemies() desde código.");
            return;
        }

        SpawnEnemies();
    }

    [ContextMenu("Limpiar enemigos generados")]
    private void EditorClearNow()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ClearSpawned();
    }
#endif

    /// <summary>Detiene un spawn gradual en curso (no borra enemigos ya creados).</summary>
    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        IsSpawningGradually = false;
    }

    /// <summary>Limpia instancias generadas por este spawner y cancela spawn en curso.</summary>
    public void ClearSpawned()
    {
        StopSpawning();

        for (var i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null)
            {
                Destroy(_spawned[i]);
            }
        }

        _spawned.Clear();
    }

    /// <summary>Inicia la generación según <see cref="spawnTiming"/> (instantánea o en corrutina).</summary>
    public void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name}: asigna un prefab de enemigo en Enemy Map Spawner.", this);
            return;
        }

        StopSpawning();

        if (clearPreviousOnSpawn)
        {
            for (var i = _spawned.Count - 1; i >= 0; i--)
            {
                if (_spawned[i] != null)
                {
                    Destroy(_spawned[i]);
                }
            }

            _spawned.Clear();
        }

        var positions = BuildPositions();
        if (positions.Count == 0)
        {
            return;
        }

        var parent = spawnParent != null ? spawnParent : transform;

        if (spawnTiming == SpawnTiming.Instant)
        {
            foreach (var p in positions)
            {
                SpawnOneAt(p, parent);
            }

            return;
        }

        _spawnRoutine = StartCoroutine(SpawnGradually(positions, parent));
    }

    private IEnumerator SpawnGradually(IReadOnlyList<Vector2> positions, Transform parent)
    {
        IsSpawningGradually = true;

        if (initialDelay > 0f)
        {
            yield return JitteredDelay(initialDelay);
        }

        var index = 0;
        var total = positions.Count;

        if (spawnTiming == SpawnTiming.OneByOne)
        {
            while (index < total)
            {
                SpawnOneAt(positions[index], parent);
                index++;
                if (index < total && secondsBetweenSpawns > 0f)
                {
                    yield return JitteredDelay(secondsBetweenSpawns);
                }
            }
        }
        else // Bursts
        {
            var burst = Mathf.Max(1, enemiesPerBurst);
            while (index < total)
            {
                var end = Mathf.Min(index + burst, total);
                for (; index < end; index++)
                {
                    SpawnOneAt(positions[index], parent);
                }

                if (index < total && secondsBetweenBursts > 0f)
                {
                    yield return JitteredDelay(secondsBetweenBursts);
                }
            }
        }

        _spawnRoutine = null;
        IsSpawningGradually = false;
    }

    /// <summary>Unity 6: <see cref="WaitForSecondsRealtime"/> no hereda de <see cref="YieldInstruction"/>.</summary>
    private object JitteredDelay(float seconds)
    {
        var t = Mathf.Max(0f, seconds + Random.Range(0f, delayJitter));
        return useUnscaledTime ? new WaitForSecondsRealtime(t) : new WaitForSeconds(t);
    }

    private void SpawnOneAt(Vector2 horizontal, Transform parent)
    {
        var world = SnapIfNeeded(horizontal);
        var instance = Instantiate(enemyPrefab, world, Quaternion.identity, parent);
        _spawned.Add(instance);
    }

    private List<Vector2> BuildPositions()
    {
        var list = new List<Vector2>(spawnCount);
        if (spawnCount <= 0)
        {
            return list;
        }

        if (distribution == DistributionMode.Grid)
        {
            FillGrid(list);
            return list;
        }

        FillRandom(list);
        return list;
    }

    private void FillGrid(List<Vector2> list)
    {
        var cols = Mathf.CeilToInt(Mathf.Sqrt(spawnCount));
        var rows = Mathf.CeilToInt(spawnCount / (float)cols);
        var halfW = regionSize.x * 0.5f;
        var halfH = regionSize.y * 0.5f;

        var stepX = cols > 1 ? regionSize.x / (cols - 1) : 0f;
        var stepY = rows > 1 ? regionSize.y / (rows - 1) : 0f;

        var index = 0;
        for (var r = 0; r < rows && index < spawnCount; r++)
        {
            for (var c = 0; c < cols && index < spawnCount; c++)
            {
                var localX = cols > 1 ? -halfW + c * stepX : 0f;
                var localY = rows > 1 ? -halfH + r * stepY : 0f;
                list.Add(LocalToWorld(new Vector2(localX, localY)));
                index++;
            }
        }
    }

    private void FillRandom(List<Vector2> list)
    {
        var useSeparation = minSeparation > 0.01f;

        for (var i = 0; i < spawnCount; i++)
        {
            Vector2 chosen = default;
            var ok = false;

            for (var attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
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
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.85f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(regionSize.x, regionSize.y, 0f));
    }
}
