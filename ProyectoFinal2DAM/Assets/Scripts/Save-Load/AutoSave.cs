using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Esta clase ayuda a "hacer una foto" del estado de la partida (vida, posición, etc.)
/// para poder guardarla. OJO: el guardado automático está apagado; la partida solo se
/// guarda al pasar por un punto de control (checkpoint). Aquí solo se prepara la
/// información en el formato que usa el juego.
/// </summary>
public class AutoSave : MonoBehaviour
{
    // Junta todos los datos de la partida en un solo paquete (SaveData) y lo devuelve.
    // Si existe el CheckpointManager, le pedimos a él los datos para usar siempre el
    // mismo formato. Si no existe, montamos un paquete mínimo a mano.
    public static SaveData CollectSaveData()
    {
        // Caso normal: hay un CheckpointManager, así que él arma el paquete de datos.
        if (CheckpointManager.instance != null)
            return CheckpointManager.instance.CollectSaveData();

        // Plan B: si no hay CheckpointManager, guardamos lo mínimo (escena y posición).
        SaveData data = new SaveData();
        // Guardamos en qué escena (nivel) estamos ahora mismo.
        data.sceneName = SceneManager.GetActiveScene().name;
        // Si el jugador existe, guardamos dónde está (sus coordenadas X, Y, Z).
        if (PlayerController.instance != null)
        {
            data.posX = PlayerController.instance.transform.position.x;
            data.posY = PlayerController.instance.transform.position.y;
            data.posZ = PlayerController.instance.transform.position.z;
        }
        return data;
    }
}
