using System.IO;
using UnityEngine;

/// <summary>
/// Este script se encarga de guardar y cargar la partida en archivos del ordenador.
/// Convierte los datos del juego en texto (formato JSON) y los escribe en un archivo,
/// o lee ese archivo para recuperar la partida. También puede borrar partidas guardadas.
/// </summary>
public class SaveSystem
{
    // Cuántas partidas distintas se pueden guardar a la vez (4 "cajones" o ranuras).
    public const int MaxSlots = 4;
    // La ranura número 0 se reserva para el guardado automático.
    public const int AutoSaveSlot = 0;

    // Guarda los datos de la partida en la ranura indicada, escribiéndolos en un archivo.
    public static void SaveGame(SaveData data, int slot)
    {
        // "try" intenta hacer algo; si algo falla, saltamos al "catch" para no romper el juego.
        try
        {
            // Apuntamos la fecha y hora actuales para saber cuándo se guardó la partida.
            data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Calculamos dónde se va a guardar el archivo y convertimos los datos a texto (JSON).
            string path = GetPath(slot);
            string json = JsonUtility.ToJson(data);
            // Escribimos ese texto en el archivo.
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            // Si algo salió mal al guardar, mostramos el error en la consola.
            Debug.LogError("Error al guardar: " + e.Message);
        }
    }

    // Lee de la ranura indicada y devuelve la partida guardada (o null si no hay nada válido).
    public static SaveData LoadGame(int slot)
    {
        string path = GetPath(slot);

        // Si el archivo no existe, no hay nada que cargar.
        if (!File.Exists(path))
            return null;

        try
        {
            // Leemos el texto del archivo y lo convertimos otra vez en datos del juego.
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Comprobamos que los datos importantes existan (si no, el archivo está dañado).
            if (data == null || string.IsNullOrEmpty(data.sceneName))
            {
                Debug.LogWarning("Datos corruptos en ranura " + slot);
                return null;
            }

            return data;
        }
        catch (System.Exception e)
        {
            // Si algo salió mal al leer, avisamos del error y devolvemos null.
            Debug.LogError("Error al cargar ranura " + slot + ": " + e.Message);
            return null;
        }
    }

    // Borra la partida guardada en la ranura indicada. Devuelve true si se borró bien.
    public static bool DeleteSave(int slot)
    {
        string path = GetPath(slot);

        // Comprobamos que el archivo exista antes de intentar borrarlo.
        if (!File.Exists(path))
        {
            Debug.LogWarning("No existe guardado en ranura " + slot);
            return false;
        }

        try
        {
            // Borramos el archivo de guardado.
            File.Delete(path);
            Debug.Log("Ranura " + slot + " eliminada correctamente");
            return true;
        }
        catch (System.Exception e)
        {
            // Si no se pudo borrar, avisamos del error.
            Debug.LogError("Error al eliminar ranura " + slot + ": " + e.Message);
            return false;
        }
    }

    // Construye la ruta (la dirección) del archivo de guardado según el número de ranura.
    static string GetPath(int slot)
    {
        return Application.persistentDataPath + "/save_" + slot + ".json";
    }
}