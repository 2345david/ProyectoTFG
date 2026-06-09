using UnityEngine;
using UnityEngine.SceneManagement; // Carga de escenas (fallback si no hay WorldManager)
using System.IO;                   // Lectura/escritura del archivo de guardado
using System.Collections.Generic;  // List<>

/// <summary>
/// Es el encargado principal de guardar y cargar la partida. Recoge todo el estado
/// del juego (vida, dinero, posición, jefes derrotados, etc.) y lo guarda en un archivo,
/// o lo vuelve a poner al cargar. Solo existe uno y no se borra al cambiar de escena.
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    // El único CheckpointManager que existe; se puede usar desde cualquier script.
    public static CheckpointManager instance;

    private Vector3 lastCheckpointPosition; // El último sitio donde el jugador volverá a aparecer.
    private bool hasCheckpoint = false;     // ¿Ya se ha registrado algún punto de guardado?
    private List<string> defeatedBosses = new List<string>(); // Nombres de los jefes ya derrotados.

    // En qué "cajón" (ranura) se está guardando o cargando ahora.
    public int currentSlot = SaveSystem.AutoSaveSlot;
    // Qué ranura pidió cargar el menú al entrar al juego; -1 significa partida nueva.
    private int _requestedSlot = SaveSystem.AutoSaveSlot;

    // Apunta que un jefe ha sido derrotado (si no estaba ya) y guarda la partida.
    public void MarkBossDefeated(string bossID)
    {
        if (!defeatedBosses.Contains(bossID))
        {
            defeatedBosses.Add(bossID);
            SaveGame();
        }
    }

    // Responde sí o no: ¿este jefe ya ha sido derrotado?
    public bool IsBossDefeated(string bossID)
    {
        return defeatedBosses.Contains(bossID);
    }

    // Se ejecuta al nacer: deja a este como el único, evita que se borre y mira qué ranura cargar.
    private void Awake()
    {
        // Si no había ninguno, este es el bueno y hacemos que no se borre al cambiar de escena.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // Miramos qué ranura quiere cargar el menú. -1 significa empezar una partida nueva.
            _requestedSlot = PlayerPrefs.GetInt("LoadSlot", SaveSystem.AutoSaveSlot);
        }
        else
        {
            // Si ya había uno, este sobra y se borra.
            Destroy(gameObject);
        }
    }

    // Se ejecuta al empezar: si hay partida guardada la carga; si no, prepara una partida nueva.
    private void Start()
    {
        // Si la ranura pedida es válida y tiene datos guardados, la cargamos.
        if (_requestedSlot >= 0 && SaveSystem.LoadGame(_requestedSlot) != null)
        {
            currentSlot = _requestedSlot;
            LoadGame();
        }
        else
        {
            // Partida nueva: usamos la ranura elegida en el menú. El progreso NO se guarda
            // solo: se guardará la primera vez que el jugador pase por un punto de guardado.
            currentSlot = _requestedSlot >= 0 ? _requestedSlot : 0;
            StartCoroutine(SetInitialRespawn());
        }
    }

    /// <summary>
    /// Para una partida nueva: espera a que cargue la primera zona y a que el jugador
    /// esté colocado, y entonces apunta ese sitio como punto de reaparición (solo en memoria).
    /// NO guarda en el archivo: eso solo pasa al usar un punto de guardado.
    /// </summary>
    private System.Collections.IEnumerator SetInitialRespawn()
    {
        // Esperamos a que el WorldManager termine de cargar la primera zona
        // antes de seguir (para no trabajar con datos a medias).
        if (Metroidvania.Core.WorldManager.Instance != null)
        {
            // Esperamos hasta que la primera zona esté cargada y no estemos cambiando de zona.
            while (string.IsNullOrEmpty(Metroidvania.Core.WorldManager.Instance.currentBiome)
                   || Metroidvania.Core.WorldManager.Instance.isTransitioning)
            {
                yield return null;
            }
        }

        // Esperamos hasta que el jugador exista en la escena.
        while (PlayerController.instance == null && GameObject.FindWithTag("Player") == null)
        {
            yield return null;
        }

        // Un fotograma más para asegurarnos de que el jugador ya está colocado en su sitio.
        yield return null;

        // Averiguamos en qué posición está el jugador ahora mismo.
        Vector3 startPosition;
        if (PlayerController.instance != null)
        {
            startPosition = PlayerController.instance.transform.position;
        }
        else
        {
            GameObject player = GameObject.FindWithTag("Player");
            startPosition = player != null ? player.transform.position : Vector3.zero;
        }

        // Apuntamos ese sitio como punto de reaparición, pero solo en memoria (no en el archivo).
        // Si el jugador muere antes de usar el primer punto de guardado, reaparecerá aquí.
        lastCheckpointPosition = startPosition;
        hasCheckpoint = true;
        Debug.Log("Nueva partida: punto de reaparición inicial en " + startPosition + " (sin autoguardado)");
    }

    // Apunta un nuevo punto de reaparición y guarda la partida en el archivo.
    public void SaveCheckpoint(Vector3 position)
    {
        // Actualizamos el sitio donde el jugador volverá a aparecer.
        lastCheckpointPosition = position;

        SaveGame();
        Debug.Log("Game Saved to file at " + position);
    }

    // Recoge todo el estado del juego y lo escribe en la ranura actual (a través de SaveSystem).
    public void SaveGame()
    {
        SaveData data = CollectSaveData();
        SaveSystem.SaveGame(data, currentSlot);
    }

    /// <summary>
    /// Reúne en una ficha (SaveData) todo el estado del juego: vida, maná, experiencia,
    /// dinero, inventario, jefes, habilidades y posición. No escribe en el archivo
    /// (de eso se encarga SaveSystem); solo prepara los datos.
    /// </summary>
    public SaveData CollectSaveData()
    {
        // Creamos una ficha vacía que iremos rellenando.
        SaveData data = new SaveData();

        // Si existe el ProgressionManager, le pedimos que guarde su propio progreso.
        if (Metroidvania.Core.ProgressionManager.Instance != null)
        {
            Metroidvania.Core.ProgressionManager.Instance.SaveGame();
        }

        // --- Posición ---
        data.posX = lastCheckpointPosition.x;
        data.posY = lastCheckpointPosition.y;
        data.posZ = lastCheckpointPosition.z;
        
        // Apuntamos la zona actual (o la escena que esté activa si no hay WorldManager).
        if (Metroidvania.Core.WorldManager.Instance != null)
        {
            data.sceneName = Metroidvania.Core.WorldManager.Instance.currentBiome;
        }
        else
        {
            data.sceneName = SceneManager.GetActiveScene().name;
        }

        // --- Vida y maná ---
        if (PlayerHealth.instance != null)
        {
            data.currentHealth = PlayerHealth.instance.health;
            data.maxHealth = PlayerHealth.instance.maxHealth;
        }
        if (PlayerMana.instance != null) data.currentMana = PlayerMana.instance.mana;

        // --- Experiencia y estadísticas ---
        if (ExperienceScript.instance != null)
        {
            data.playerLevel = ExperienceScript.instance.currentLevel;
            data.currentXP = ExperienceScript.instance.currentExperience;
            data.expToNextLevel = ExperienceScript.instance.expTNL;
        }
        if (PlayerAttack.instance != null) data.playerDamage = PlayerAttack.instance.damage;

        // --- Dinero ---
        if (BankAccount.Instance != null) data.bankBalance = BankAccount.Instance.bank;

        // --- Flechas (subítem y reserva) ---
        if (SubItems.Instance != null)
        {
            data.subItemsAmount = SubItems.Instance.subItemsAmount;
            data.reserveArrows = SubItems.Instance.reserveArrows;
        }

        // --- Inventario (colas de pociones) ---
        if (PlayerInventory.Instance != null)
        {
            data.healthPotions = PlayerInventory.Instance.GetPotionHealQueue();
            data.manaPotions = PlayerInventory.Instance.GetManaPotionQueue();
        }

        // --- Jefes derrotados --- (hacemos una copia para no compartir la lista original)
        data.defeatedBosses = new List<string>(defeatedBosses);

        // --- Habilidades desbloqueadas ---
        if (PlayerController.instance != null)
        {
            data.hasDash = PlayerController.instance.hasDash;
            data.hasDoubleJump = PlayerController.instance.hasDoubleJump;
        }

        return data;
    }

    /// <summary>
    /// Carga la partida guardada y vuelve a poner todo en su sitio: posición, zona,
    /// vida, maná, experiencia, dinero, flechas, inventario, jefes y habilidades.
    /// Después actualiza lo que se ve en pantalla.
    /// </summary>
    public void LoadGame()
    {
        // Leemos los datos guardados en la ranura actual.
        SaveData data = SaveSystem.LoadGame(currentSlot);
        if (data == null)
        {
            // Partida nueva sin guardar todavía: colocamos al jugador en el punto inicial guardado en memoria.
            SetPlayerPosition(lastCheckpointPosition);
            return;
        }
        ApplySaveData(data);
    }

    // Coge una ficha de datos ya leída y la usa para volver a poner todo el juego como estaba.
    public void ApplySaveData(SaveData data)
    {
        if (data == null) return;

        // Si existe el ProgressionManager, le pedimos que cargue su propio progreso.
        if (Metroidvania.Core.ProgressionManager.Instance != null)
        {
            Metroidvania.Core.ProgressionManager.Instance.LoadGame();
        }

        // --- Jefes --- (si la lista venía vacía/nula, usamos una lista vacía nueva)
        defeatedBosses = data.defeatedBosses ?? new List<string>();

        // --- Habilidades ---
        if (PlayerController.instance != null)
        {
            PlayerController.instance.hasDash = data.hasDash;
            PlayerController.instance.hasDoubleJump = data.hasDoubleJump;
        }

        // --- Posición y escena/bioma ---
        lastCheckpointPosition = new Vector3(data.posX, data.posY, data.posZ);
        
        if (!string.IsNullOrEmpty(data.sceneName))
        {
            if (Metroidvania.Core.WorldManager.Instance != null)
            {
                // Si la zona guardada no es la actual, viajamos a ella y luego colocamos al jugador.
                if (Metroidvania.Core.WorldManager.Instance.currentBiome != data.sceneName)
                {
                    StartCoroutine(LoadBiomeAndSetPosition(data.sceneName, lastCheckpointPosition));
                }
                else
                {
                    // Ya estamos en la zona correcta: colocamos al jugador directamente.
                    SetPlayerPosition(lastCheckpointPosition);
                }
            }
            else
            {
                // Si no hay WorldManager, cargamos la escena por su nombre (manera más sencilla).
                if (SceneManager.GetActiveScene().name != data.sceneName)
                {
                    SceneManager.LoadScene(data.sceneName);
                    // Nota: aquí la posición podría perderse al cargar; en este juego
                    // se espera que exista el WorldManager para hacerlo bien.
                }
                SetPlayerPosition(lastCheckpointPosition);
            }
        }
        else
        {
            // Si no hay nombre de escena, simplemente colocamos al jugador.
            SetPlayerPosition(lastCheckpointPosition);
        }

        // --- Vida y maná --- (usamos los valores guardados solo si son válidos, mayores que 0)
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.maxHealth = data.maxHealth > 0 ? data.maxHealth : PlayerHealth.instance.maxHealth;
            PlayerHealth.instance.health = data.currentHealth > 0 ? data.currentHealth : PlayerHealth.instance.maxHealth;
        }
        if (PlayerMana.instance != null)
        {
            PlayerMana.instance.mana = data.currentMana;
        }

        // --- Experiencia y estadísticas ---
        if (ExperienceScript.instance != null)
        {
            ExperienceScript.instance.currentLevel = data.playerLevel > 0 ? data.playerLevel : 1;
            ExperienceScript.instance.currentExperience = data.currentXP;
            ExperienceScript.instance.expTNL = data.expToNextLevel > 0 ? data.expToNextLevel : ExperienceScript.instance.expTNL;
            
            // Actualizamos lo que se ve en pantalla sobre experiencia y nivel.
            ExperienceScript.instance.UpdateUI(); 
        }
        if (PlayerAttack.instance != null)
        {
            PlayerAttack.instance.damage = data.playerDamage > 0 ? data.playerDamage : PlayerAttack.instance.damage;
        }

        // --- Dinero ---
        if (BankAccount.Instance != null)
        {
            BankAccount.Instance.bank = data.bankBalance;
            BankAccount.Instance.Money(0); // Pasamos 0 solo para que se actualice el dinero en pantalla.
        }

        // --- Flechas ---
        if (SubItems.Instance != null)
        {
            SubItems.Instance.subItemsAmount = data.subItemsAmount;
            SubItems.Instance.reserveArrows = data.reserveArrows;
            SubItems.Instance.SubItem(0); // Pasamos 0 solo para que se actualicen las flechas en pantalla.
        }

        // --- Inventario (colas de pociones) ---
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SetPotionQueues(data.healthPotions, data.manaPotions);
        }
    }

    // Coloca al jugador en la posición indicada (usa el PlayerController o lo busca por su etiqueta "Player").
    private void SetPlayerPosition(Vector3 position)
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.transform.position = position;
        }
        else
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = position;
            }
        }
    }

    /// <summary>
    /// Viaja a la zona guardada y, cuando termina el cambio de zona,
    /// coloca al jugador en el punto de reaparición.
    /// </summary>
    private System.Collections.IEnumerator LoadBiomeAndSetPosition(string biomeName, Vector3 position)
    {
        if (Metroidvania.Core.WorldManager.Instance != null)
        {
            // Si el WorldManager está ocupado (por ejemplo cargando la primera zona), esperamos.
            while (Metroidvania.Core.WorldManager.Instance.isTransitioning)
            {
                yield return null;
            }

            // Si ya estamos en la zona a la que íbamos, solo colocamos al jugador y salimos.
            if (Metroidvania.Core.WorldManager.Instance.currentBiome == biomeName)
            {
                SetPlayerPosition(position);
                yield break;
            }

            // Empezamos el viaje a la zona deseada.
            Metroidvania.Core.WorldManager.Instance.TravelToBiome(biomeName, "LOADING_FROM_SAVE");
            
            // Esperamos a que termine el cambio y estemos ya en la zona correcta.
            while (Metroidvania.Core.WorldManager.Instance.isTransitioning || Metroidvania.Core.WorldManager.Instance.currentBiome != biomeName)
            {
                yield return null;
            }
            
            // Un fotograma más para asegurarnos de que todo ha terminado.
            yield return null;
            
            // Por fin, colocamos al jugador en la posición guardada.
            SetPlayerPosition(position);
        }
    }

    // Devuelve el último sitio de reaparición (dónde volverá a aparecer el jugador).
    public Vector3 GetRespawnPosition()
    {
        return lastCheckpointPosition;
    }

    // Borra la partida guardada de la ranura actual (reinicia el progreso del archivo).
    public void ResetCheckpoint()
    {
        SaveSystem.DeleteSave(currentSlot);
    }
}
