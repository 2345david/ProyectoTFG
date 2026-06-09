using System.IO;
using UnityEngine;

public class SaveSystem
{
    // Número máximo de ranuras de guardado
    public const int MaxSlots = 3;

    public static void SaveGame(SaveData data, int slot)
    {
        try
        {
            data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            string path = GetPath(slot);
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al guardar: " + e.Message);
        }
    }

    public static SaveData LoadGame(int slot)
    {
        string path = GetPath(slot);

        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Validamos que los datos esenciales existan
            if (data == null || string.IsNullOrEmpty(data.sceneName))
            {
                Debug.LogWarning("Datos corruptos en ranura " + slot);
                return null;
            }

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar ranura " + slot + ": " + e.Message);
            return null;
        }
    }

    public static bool DeleteSave(int slot)
    {
        string path = GetPath(slot);

        // Verificamos que el archivo existe antes de intentar borrarlo
        if (!File.Exists(path))
        {
            Debug.LogWarning("No existe guardado en ranura " + slot);
            return false;
        }

        try
        {
            File.Delete(path);
            Debug.Log("Ranura " + slot + " eliminada correctamente");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al eliminar ranura " + slot + ": " + e.Message);
            return false;
        }
    }

    // Método auxiliar para no repetir la ruta en cada método
    static string GetPath(int slot)
    {
        return Application.persistentDataPath + "/save_" + slot + ".json";
    }
}