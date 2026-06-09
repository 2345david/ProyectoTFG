using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Una zona que va creando enemigos (estilo metroidvania). Puede usar varios tipos de enemigo
/// (unos salen más que otros según su "peso"), crea la primera tanda al empezar o al entrar el
/// jugador, y vuelve a llenar la zona de enemigos cuando deja de verse por la cámara.
/// </summary>
[DisallowMultipleComponent]
public class MetroidvaniaSpawnZone : MonoBehaviour
{
    // Cómo se reparten los enemigos por la zona: al azar o en cuadrícula ordenada.
    public enum DistributionMode
    {
        RandomUniform,
        Grid
    }

    // El ritmo al que van saliendo los enemigos: todos de golpe, de uno en uno, o por grupitos.
    public enum SpawnPace
    {
        Instant,
        OneByOne,
        Bursts
    }

    // Qué hace que salga la primera tanda de enemigos.
    public enum InitialSpawnTrigger
    {
        /// <summary>Salen al empezar la escena (puede esperar un poquito antes).</summary>
        OnSceneStart,

        /// <summary>Salen la primera vez que el jugador entra en la zona de este objeto.</summary>
        OnPlayerFirstTriggerEnter
    }

    // Un tipo de enemigo junto con su "peso" (cuanto más peso, más veces aparece).
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

    // Los enemigos vivos que ha creado esta zona en la tanda actual.
    private readonly List<GameObject> _spawned = new List<GameObject>();
    // La tarea que está creando enemigos ahora mismo (la guardamos para poder pararla).
    private Coroutine _spawnRoutine;
    // Verdadero cuando ya se hizo la primera tanda (a partir de ahí se permite rellenar la zona).
    private bool _initialWaveScheduledOrDone;
    // Cuánto tiempo lleva la zona sin verse por la cámara.
    private float _hiddenTimer;
    // Cuándo se rellenó la zona por última vez (para no rellenar demasiado seguido).
    private float _lastRespawnTime = -999f;
    // Si la zona se veía o no en el frame anterior (para notar el momento en que deja de verse).
    private bool _wasVisible = true;

    /// <summary>Verdadero mientras se están creando enemigos en este momento.</summary>
    public bool IsSpawning { get; private set; }

    // Al despertar, buscamos solos al jugador y a la cámara si no se asignaron en el Inspector.
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

        // Si no hay cámara puesta, usamos la cámara principal.
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }

    // Al empezar la escena, si está configurado así, lanza la primera tanda (con o sin espera).
    private void Start()
    {
        // Si la primera tanda NO debe salir al empezar la escena, no hacemos nada aquí.
        if (initialSpawnTrigger != InitialSpawnTrigger.OnSceneStart)
        {
            return;
        }

        // Si hay una espera configurada, esperamos; si no, lanzamos ya la tanda.
        if (delayAfterSceneStart > 0f)
        {
            StartCoroutine(SceneStartDelayedSpawn());
        }
        else
        {
            BeginInitialWave();
        }
    }

    /// <summary>Espera el tiempo configurado y luego lanza la primera tanda de enemigos.</summary>
    private IEnumerator SceneStartDelayedSpawn()
    {
        yield return new WaitForSeconds(delayAfterSceneStart);
        BeginInitialWave();
    }

    // Lanza la primera tanda cuando el jugador entra por primera vez en la zona.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Esto solo sirve si está configurado para salir al entrar el jugador.
        if (initialSpawnTrigger != InitialSpawnTrigger.OnPlayerFirstTriggerEnter)
        {
            return;
        }

        // Si lo que entró no es el jugador, no hacemos nada.
        if (!other.CompareTag("Player"))
        {
            return;
        }

        BeginInitialWave();
    }

    /// <summary>
    /// Se ejecuta cada frame para rellenar la zona: si la zona lleva suficiente tiempo sin verse
    /// por la cámara (y se cumplen las condiciones), crea una tanda nueva de enemigos.
    /// </summary>
    private void Update()
    {
        // Si esta función está desactivada o aún no hubo primera tanda, no hacemos nada.
        if (!respawnWhenZoneNotVisible || !_initialWaveScheduledOrDone)
        {
            return;
        }

        // Si justo ahora estamos creando enemigos, no comprobamos nada.
        if (IsSpawning)
        {
            return;
        }

        // Miramos si la zona se ve ahora mismo.
        var visible = IsZoneVisibleToPlayerCamera();
        if (visible)
        {
            // Se ve: ponemos a cero el contador de "tiempo oculto" y lo recordamos.
            _hiddenTimer = 0f;
            _wasVisible = true;
            return;
        }

        // Justo cuando pasa de verse a no verse, ponemos el contador a cero para empezar a contar.
        if (_wasVisible)
        {
            _hiddenTimer = 0f;
            _wasVisible = false;
        }

        // Sumamos el tiempo que lleva oculta; si todavía no es suficiente, esperamos más.
        _hiddenTimer += Time.deltaTime;
        if (_hiddenTimer < secondsHiddenBeforeRespawn)
        {
            return;
        }

        // No rellenamos si hace muy poco que ya rellenamos (tiempo de espera entre tandas).
        if (Time.time - _lastRespawnTime < respawnCooldown)
        {
            return;
        }

        // Si está marcado que no haya enemigos vivos para rellenar, esperamos a que mueran todos.
        if (requireAllDefeatedBeforeRespawn && !AllTrackedEnemiesDefeated())
        {
            return;
        }

        // Se cumple todo: ponemos los contadores a cero y creamos la tanda nueva.
        _hiddenTimer = 0f;
        _lastRespawnTime = Time.time;
        StartRespawnWave();
    }

    /// <summary>
    /// Cuando se apaga este componente, detiene la creación de enemigos que estuviera en marcha.
    /// </summary>
    private void OnDisable()
    {
        // Paramos cualquier creación de enemigos en curso para no dejarla a medias.
        StopSpawnRoutine();
        IsSpawning = false;
    }

    /// <summary>Obliga a crear una tanda nueva (si <paramref name="ignoreCooldown"/> es false, respeta el tiempo de espera).</summary>
    public void ForceRespawnWave(bool ignoreCooldown = true)
    {
        if (!ignoreCooldown && Time.time - _lastRespawnTime < respawnCooldown)
        {
            return;
        }

        _lastRespawnTime = Time.time;
        StartRespawnWave();
    }

    /// <summary>Borra todos los enemigos que ha creado esta zona.</summary>
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

    /// <summary>
    /// Lanza la primera tanda de enemigos (solo una vez). A partir de aquí ya se permite rellenar la zona.
    /// </summary>
    private void BeginInitialWave()
    {
        // Si la primera tanda ya se hizo, no la repetimos.
        if (_initialWaveScheduledOrDone)
        {
            return;
        }

        // Comprobamos que haya tipos de enemigo válidos antes de crear nada.
        if (!ValidateEnemyTable())
        {
            return;
        }

        // La marcamos como hecha (esto permite rellenar después) y la lanzamos.
        _initialWaveScheduledOrDone = true;
        StartWaveInternal(isRespawn: false);
    }

    /// <summary>
    /// Rellena la zona: para cualquier creación anterior, borra los enemigos de ahora y crea una tanda nueva.
    /// </summary>
    private void StartRespawnWave()
    {
        // Comprobamos que haya tipos de enemigo válidos antes de seguir.
        if (!ValidateEnemyTable())
        {
            return;
        }

        // Paramos la creación anterior y borramos los enemigos que había antes de crear los nuevos.
        StopSpawnRoutine();
        ClearSpawnedEnemies();
        StartWaveInternal(isRespawn: true);
    }

    /// <summary>
    /// Parte común que usan todas las tandas: decide cuántos enemigos crear, dónde, y arranca su creación.
    /// </summary>
    /// <param name="isRespawn">True si es un relleno (usa su propio ritmo); false para la primera tanda.</param>
    private void StartWaveInternal(bool isRespawn)
    {
        // Elegimos al azar cuántos enemigos crear, dentro del mínimo y máximo configurados.
        var count = Random.Range(
            Mathf.Min(spawnCountMin, spawnCountMax),
            Mathf.Max(spawnCountMin, spawnCountMax) + 1);
        // Calculamos en qué sitios van a aparecer.
        var positions = BuildPositions(count);
        if (positions.Count == 0)
        {
            return;
        }

        // De quién "cuelgan" los enemigos: el objeto configurado o, si no hay, este mismo.
        var parent = spawnParent != null ? spawnParent : transform;
        // Ritmo y número de configuración (0 = primera tanda, 1 = relleno) según el tipo de tanda.
        var pace = isRespawn ? respawnSpawnPace : initialSpawnPace;
        var id = isRespawn ? 1 : 0;
        // Arrancamos la tarea que va creando los enemigos al ritmo indicado.
        _spawnRoutine = StartCoroutine(SpawnWaveCoroutine(positions, parent, pace, id));
    }

    /// <summary>
    /// Tarea que va creando los enemigos en sus sitios al ritmo elegido (todos de golpe, de uno en uno
    /// o por grupitos), con sus tiempos de espera. Usa los ajustes de la primera tanda o los del relleno.
    /// </summary>
    /// <param name="positions">Los sitios donde van a aparecer los enemigos.</param>
    /// <param name="parent">El objeto del que "cuelgan" los enemigos creados.</param>
    /// <param name="pace">El ritmo al que aparecen.</param>
    /// <param name="configId">0 = ajustes de la primera tanda, 1 = ajustes del relleno.</param>
    private IEnumerator SpawnWaveCoroutine(
        IReadOnlyList<Vector2> positions,
        Transform parent,
        SpawnPace pace,
        int configId)
    {
        // Avisamos de que estamos creando enemigos, para que no se intente rellenar a la vez.
        IsSpawning = true;
        // Elegimos los tiempos según sea la primera tanda (0) o un relleno (1).
        var initialDelay = configId == 0 ? initialPaceInitialDelay : respawnPaceInitialDelay;
        var betweenOne = configId == 0 ? initialSecondsBetweenSpawns : respawnSecondsBetweenSpawns;
        var burst = Mathf.Max(1, configId == 0 ? initialBurstSize : respawnBurstSize);
        var betweenBurst = configId == 0 ? initialSecondsBetweenBursts : respawnSecondsBetweenBursts;
        var jitter = configId == 0 ? initialDelayJitter : respawnDelayJitter;
        var unscaled = configId == 0 ? initialUseUnscaledTime : respawnUseUnscaledTime;

        // Espera opcional antes de empezar a crear enemigos.
        if (initialDelay > 0f)
        {
            yield return JitteredDelay(initialDelay, jitter, unscaled);
        }

        // Ritmo "todos de golpe": creamos todos los enemigos a la vez y terminamos.
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
            // Ritmo "de uno en uno": creamos un enemigo y esperamos antes del siguiente.
            while (index < total)
            {
                SpawnOneEnemyAt(positions[index], parent);
                index++;
                // Esperamos entre enemigos, menos después del último.
                if (index < total && betweenOne > 0f)
                {
                    yield return JitteredDelay(betweenOne, jitter, unscaled);
                }
            }
        }
        else
        {
            // Ritmo "por grupitos": creamos un grupo de enemigos y esperamos antes del siguiente grupo.
            while (index < total)
            {
                var end = Mathf.Min(index + burst, total);
                for (; index < end; index++)
                {
                    SpawnOneEnemyAt(positions[index], parent);
                }

                // Esperamos entre grupos, menos después del último.
                if (index < total && betweenBurst > 0f)
                {
                    yield return JitteredDelay(betweenBurst, jitter, unscaled);
                }
            }
        }

        // Damos por terminada la creación de enemigos.
        FinishSpawnRoutine();
    }

    /// <summary>
    /// Marca que la creación de enemigos ha terminado bien (olvida la tarea y avisa de que ya no se está creando).
    /// </summary>
    private void FinishSpawnRoutine()
    {
        _spawnRoutine = null;
        IsSpawning = false;
    }

    /// <summary>
    /// Detiene antes de tiempo la creación de enemigos que estuviera en marcha (si la hay).
    /// </summary>
    private void StopSpawnRoutine()
    {
        // Si hay una creación en marcha, la paramos y la olvidamos.
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        IsSpawning = false;
    }

    /// <summary>
    /// Crea una espera de la duración indicada más un poquito al azar (jitter), para que no sea siempre igual.
    /// </summary>
    /// <param name="seconds">La espera básica.</param>
    /// <param name="jitter">El máximo de tiempo extra al azar que se le añade.</param>
    /// <param name="unscaled">Si es true, usa el tiempo real (aunque el juego esté en cámara lenta o pausado).</param>
    private static object JitteredDelay(float seconds, float jitter, bool unscaled)
    {
        // Tiempo total = espera básica + un poquito al azar; nunca negativo.
        var t = Mathf.Max(0f, seconds + Random.Range(0f, jitter));
        // Elegimos el tipo de espera según se use el tiempo real o el normal.
        return unscaled ? new WaitForSecondsRealtime(t) : new WaitForSeconds(t);
    }

    /// <summary>
    /// Crea un enemigo en el sitio indicado: elige el tipo según su peso, lo apoya en el suelo si toca y lo apunta en la lista.
    /// </summary>
    /// <param name="horizontal">El sitio (X,Y) antes de bajarlo al suelo.</param>
    /// <param name="parent">El objeto del que "cuelga" el enemigo creado.</param>
    private void SpawnOneEnemyAt(Vector2 horizontal, Transform parent)
    {
        // Elegimos qué tipo de enemigo crear según los pesos.
        var prefab = PickWeightedPrefab();
        if (prefab == null)
        {
            return;
        }

        // Si toca, lo bajamos hasta el suelo; luego lo creamos y lo guardamos en la lista.
        var world = SnapIfNeeded(horizontal);
        var instance = Instantiate(prefab, world, Quaternion.identity, parent);
        _spawned.Add(instance);
    }

    /// <summary>
    /// Elige al azar un tipo de enemigo, pero dando más probabilidad a los que tienen más peso. Devuelve null si no hay pesos válidos.
    /// </summary>
    private GameObject PickWeightedPrefab()
    {
        // Sumamos los pesos de todos los tipos que estén bien puestos (no vacíos).
        float sum = 0f;
        for (var i = 0; i < enemyTypes.Count; i++)
        {
            if (enemyTypes[i].prefab != null)
            {
                sum += enemyTypes[i].weight;
            }
        }

        // Si la suma de pesos no es mayor que cero, no podemos elegir.
        if (sum <= 0f)
        {
            return null;
        }

        // Sacamos un número al azar y vamos restando pesos hasta dar con el tipo elegido.
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

        // Por si los decimales fallan justo al final, devolvemos el último tipo.
        return enemyTypes[enemyTypes.Count - 1].prefab;
    }

    /// <summary>
    /// Comprueba que la lista de tipos de enemigo sirva (que exista y tenga al menos uno bien puesto).
    /// Si no, avisa en la consola y devuelve false.
    /// </summary>
    private bool ValidateEnemyTable()
    {
        // La lista tiene que existir y no estar vacía.
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogWarning($"{name}: añade al menos un prefab en Enemy Types.", this);
            return false;
        }

        // Con encontrar un solo tipo bien puesto ya nos vale.
        for (var i = 0; i < enemyTypes.Count; i++)
        {
            if (enemyTypes[i].prefab != null)
            {
                return true;
            }
        }

        // Si todos están vacíos, no se puede crear ningún enemigo.
        Debug.LogWarning($"{name}: todos los prefabs en Enemy Types son nulos.", this);
        return false;
    }

    /// <summary>
    /// Dice si ya han muerto todos los enemigos de la tanda actual (la lista se queda vacía tras quitar los borrados).
    /// </summary>
    private bool AllTrackedEnemiesDefeated()
    {
        // Quitamos de la lista los enemigos ya muertos antes de mirar si queda alguno.
        PurgeDestroyedFromList();
        return _spawned.Count == 0;
    }

    /// <summary>
    /// Limpia de la lista los enemigos que ya fueron borrados (las casillas que quedaron vacías).
    /// </summary>
    private void PurgeDestroyedFromList()
    {
        // Recorremos del final al principio para poder borrar sin liarnos con las posiciones.
        for (var i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] == null)
            {
                _spawned.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Dice si la zona se ve ahora mismo. Con una cámara 2D normal, mira si la pantalla y la zona se tocan;
    /// si no hay esa cámara, se conforma con mirar si el jugador está cerca de la zona.
    /// </summary>
    private bool IsZoneVisibleToPlayerCamera()
    {
        // La caja que ocupa la zona en el mundo (con un poco de grosor para comparar bien).
        var zoneBounds = GetSpawnZoneWorldBounds2D();

        // Caso normal: cámara 2D. Comparamos lo que enseña la pantalla (un poco agrandado) con la zona.
        if (gameplayCamera != null && gameplayCamera.orthographic)
        {
            var camBounds = GetOrthographicCameraWorldBounds(gameplayCamera);
            // Agrandamos un poco la pantalla para no rellenar justo en el borde del encuadre.
            camBounds.Expand(cameraBoundsPadding);
            return camBounds.Intersects(zoneBounds);
        }

        // Si no hay esa cámara: decimos que se "ve" si el jugador está cerca de la zona.
        if (playerTransform != null)
        {
            var closest = zoneBounds.ClosestPoint(playerTransform.position);
            return Vector2.Distance(closest, playerTransform.position) < fallbackPlayerNearZoneDistance;
        }

        // Sin cámara ni jugador, decimos que se ve (así no rellena, por seguridad).
        return true;
    }

    /// <summary>
    /// Calcula la caja (en el mundo) de lo que enseña una cámara 2D, a partir de su tamaño y su forma de pantalla.
    /// </summary>
    private Bounds GetOrthographicCameraWorldBounds(Camera cam)
    {
        // El alto visible es el doble del tamaño de la cámara; el ancho depende de la forma de la pantalla.
        var height = cam.orthographicSize * 2f;
        var width = height * cam.aspect;
        var c = cam.transform.position;
        // Le damos grosor (50) en profundidad para que la comparación de cajas funcione bien en 2D.
        return new Bounds(c, new Vector3(width, height, 50f));
    }

    /// <summary>
    /// Calcula la caja (en el mundo) que ocupa el rectángulo de la zona, teniendo en cuenta dónde está y cómo está girado/escalado.
    /// También le da grosor en profundidad para poder compararla bien con la cámara.
    /// </summary>
    private Bounds GetSpawnZoneWorldBounds2D()
    {
        // La mitad del ancho y del alto del rectángulo.
        var hw = regionSize.x * 0.5f;
        var hh = regionSize.y * 0.5f;
        // Empezamos en una esquina y vamos metiendo las otras tres para que la caja cubra todo el rectángulo.
        var b = new Bounds(transform.TransformPoint(new Vector3(-hw, -hh, 0f)), Vector3.zero);
        b.Encapsulate(transform.TransformPoint(new Vector3(hw, -hh, 0f)));
        b.Encapsulate(transform.TransformPoint(new Vector3(-hw, hh, 0f)));
        b.Encapsulate(transform.TransformPoint(new Vector3(hw, hh, 0f)));
        // Le damos grosor en profundidad para que la comparación de cajas con la cámara sea fiable en 2D.
        var zRef = gameplayCamera != null ? gameplayCamera.transform.position.z : b.center.z;
        var c = b.center;
        c.z = zRef;
        b.center = c;
        b.extents = new Vector3(Mathf.Max(b.extents.x, 0.05f), Mathf.Max(b.extents.y, 0.05f), 80f);
        return b;
    }

    /// <summary>
    /// Prepara la lista de sitios donde aparecerán los enemigos, según se quiera en cuadrícula ordenada o al azar.
    /// </summary>
    private List<Vector2> BuildPositions(int count)
    {
        var list = new List<Vector2>(count);
        // Sin cantidad válida devolvemos una lista vacía.
        if (count <= 0)
        {
            return list;
        }

        // Distribución en rejilla: posiciones regulares dentro de la región.
        if (distribution == DistributionMode.Grid)
        {
            FillGrid(list, count);
            return list;
        }

        // Distribución uniforme aleatoria (por defecto).
        FillRandom(list, count);
        return list;
    }

    /// <summary>
    /// Coloca los sitios en una cuadrícula lo más cuadrada posible que cubre toda la zona.
    /// </summary>
    private void FillGrid(List<Vector2> list, int count)
    {
        // Calculamos columnas y filas para repartir los puntos de forma ordenada.
        var cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        var rows = Mathf.CeilToInt(count / (float)cols);
        var halfW = regionSize.x * 0.5f;
        var halfH = regionSize.y * 0.5f;
        // La separación entre celdas (0 si solo hay una columna/fila, para centrar el punto).
        var stepX = cols > 1 ? regionSize.x / (cols - 1) : 0f;
        var stepY = rows > 1 ? regionSize.y / (rows - 1) : 0f;

        var index = 0;
        // Recorremos la cuadrícula fila a fila hasta colocar todos los puntos.
        for (var r = 0; r < rows && index < count; r++)
        {
            for (var c = 0; c < cols && index < count; c++)
            {
                // Calculamos dónde va la celda y lo pasamos a coordenadas del mundo.
                var localX = cols > 1 ? -halfW + c * stepX : 0f;
                var localY = rows > 1 ? -halfH + r * stepY : 0f;
                list.Add(LocalToWorld(new Vector2(localX, localY)));
                index++;
            }
        }
    }

    /// <summary>
    /// Coloca los sitios al azar dentro de la zona, intentando que no queden demasiado pegados entre sí.
    /// </summary>
    private void FillRandom(List<Vector2> list, int count)
    {
        // Solo cuidamos la separación mínima si su valor vale la pena.
        var useSeparation = minSeparation > 0.01f;
        for (var i = 0; i < count; i++)
        {
            Vector2 chosen = default;
            var ok = false;
            // Probamos varias veces a buscar un punto que quede lo bastante lejos de los ya puestos.
            for (var attempt = 0; attempt < maxAttemptsPerPoint; attempt++)
            {
                chosen = RandomPointInRegion();
                if (!useSeparation || IsFarEnough(chosen, list))
                {
                    ok = true;
                    break;
                }
            }

            // Si tras varios intentos no lo logramos, aceptamos un punto cualquiera para no quedarnos cortos.
            if (!ok)
            {
                chosen = RandomPointInRegion();
            }

            list.Add(chosen);
        }
    }

    /// <summary>
    /// Comprueba si un punto nuevo está lo bastante lejos de todos los puntos ya elegidos.
    /// </summary>
    private bool IsFarEnough(Vector2 candidate, List<Vector2> existing)
    {
        // Comparamos las distancias "al cuadrado" porque así el cálculo es más rápido.
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

    /// <summary>
    /// Saca un punto al azar dentro del rectángulo de la zona y lo devuelve en coordenadas del mundo.
    /// </summary>
    private Vector2 RandomPointInRegion()
    {
        var halfW = regionSize.x * 0.5f;
        var halfH = regionSize.y * 0.5f;
        // Un punto al azar dentro de los límites de la zona.
        var local = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
        return LocalToWorld(local);
    }

    /// <summary>
    /// Pasa un punto medido desde el objeto a coordenadas del mundo en 2D (quitando la profundidad).
    /// </summary>
    private Vector2 LocalToWorld(Vector2 local)
    {
        var w = transform.TransformPoint(new Vector3(local.x, local.y, 0f));
        return new Vector2(w.x, w.y);
    }

    /// <summary>
    /// Baja un punto hasta el suelo lanzando un rayo hacia abajo (si está activado). Si encuentra suelo, devuelve el punto
    /// justo encima; si no, deja el punto como estaba.
    /// </summary>
    private Vector2 SnapIfNeeded(Vector2 horizontal)
    {
        // Si no queremos apoyar en el suelo, devolvemos el punto tal cual.
        if (!snapToGround)
        {
            return horizontal;
        }

        // Lanzamos un rayo desde un poco más arriba del punto, hacia abajo, buscando el suelo.
        var origin = new Vector2(horizontal.x, horizontal.y + raycastFromYOffset);
        var hit = Physics2D.Raycast(origin, Vector2.down, raycastMaxDistance, groundLayers);
        if (hit.collider != null)
        {
            // Colocamos al enemigo justo encima del suelo, dejando un huequito.
            return new Vector2(horizontal.x, hit.point.y + groundSurfaceYOffset);
        }

        // Si no hay suelo, dejamos el punto como estaba.
        return horizontal;
    }

    /// <summary>
    /// Dibuja ayudas visuales en el editor al seleccionar el objeto: el rectángulo de la zona y, jugando, un cubo
    /// de color (azul si la zona se ve, rojo si no) para ver más fácil cómo funciona el relleno.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Dibujamos el rectángulo de la zona en verde.
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.2f, 0.85f, 0.35f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(regionSize.x, regionSize.y, 0f));

        // El cubo de color (que muestra si se ve o no) solo tiene sentido mientras se juega.
        if (!Application.isPlaying)
        {
            return;
        }

        // Color del cubo según si la zona se ve ahora: azul = se ve, rojo = no se ve.
        Gizmos.color = IsZoneVisibleToPlayerCamera()
            ? new Color(0.2f, 0.6f, 1f, 0.35f)
            : new Color(1f, 0.3f, 0.2f, 0.35f);
        var b = GetSpawnZoneWorldBounds2D();
        Gizmos.DrawCube(b.center, b.size);
    }
}
