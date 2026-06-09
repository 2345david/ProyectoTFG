using System.Collections;
using UnityEngine;

public class AutoSave : MonoBehaviour
{
    [Range(30f, 600f)]
    public float SaveInterval = 120f; // Intervalo en segundos, configurable en Inspector

    // public PlayerController Player; // Arrastra el objeto del jugador en Inspector


    void Start()
    {
        StartCoroutine(AutoSaveRoutine());
    }

    IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            // Esperamos el intervalo definido
            yield return new WaitForSeconds(SaveInterval);
            Save();
        }
    }

    void Save()
    {
        int slot = PlayerPrefs.GetInt("LoadSlot", 0);

        // Recopilamos los datos actuales del juego
        SaveData data = CollectSaveData();

        SaveSystem.SaveGame(data, slot);
        Debug.Log("Autoguardado en ranura " + slot);
    }

    SaveData CollectSaveData()
    {
        // Aquí recopilamos los datos reales del juego
        // Por ahora devolvemos datos básicos — adaptar según la lógica del juego
        return new SaveData
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")

            /*
            playerHealth = Player.Health, // datos reales de jugador
            level = Player.Level
            */
        };
    }
}