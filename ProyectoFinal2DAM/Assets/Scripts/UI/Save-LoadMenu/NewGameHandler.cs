using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Inicia una partida nueva. Limpia el autoguardado (ranura 0) para empezar de
/// cero y arranca el juego a través de la PersistentScene. El CheckpointManager
/// (en la PersistentScene) detecta el slot -1 como "partida nueva" y crea el
/// guardado inicial usando nuestro formato JSON enriquecido.
/// </summary>
public class NewGameHandler : MonoBehaviour
{
    // Escena raíz del juego (contiene WorldManager y CheckpointManager).
    public string PersistentSceneName = "PersistentScene";

    // Empieza una partida nueva: busca una ranura libre y arranca el juego.
    public void StartNewGame()
    {
        // Buscamos la primera ranura libre para la partida nueva.
        int slot = -1;
        for (int i = 0; i < SaveSystem.MaxSlots; i++)
        {
            if (SaveSystem.LoadGame(i) == null)
            {
                slot = i;
                break;
            }
        }

        // Si todas las ranuras están ocupadas, reutilizamos la primera (slot 0).
        if (slot < 0)
            slot = 0;

        // El CheckpointManager usará esta ranura; el progreso se guardará al pasar
        // por un checkpoint (no hay autoguardado al iniciar).
        PlayerPrefs.SetInt("LoadSlot", slot);
        PlayerPrefs.Save();

        Debug.Log("Nueva partida en ranura " + slot + ": cargando " + PersistentSceneName);
        SceneManager.LoadScene(PersistentSceneName, LoadSceneMode.Single);
    }
}