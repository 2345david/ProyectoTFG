using UnityEngine;
using System.Collections.Generic;

namespace Metroidvania.Core
{
    /// <summary>
    /// Lista de todas las habilidades que el jugador puede ir consiguiendo en el juego.
    /// Por ejemplo, las puertas especiales miran esta lista para ver si puedes pasar.
    /// </summary>
    [System.Serializable]
    public enum Ability
    {
        DoubleJump,    // Doble salto
        Dash,          // Impulso/embestida rápida
        WallJump,      // Salto de pared
        MorphBall,     // Forma esférica para pasar por huecos
        GrapplingHook, // Gancho de agarre
        Swim           // Nadar
    }

    /// <summary>
    /// Lleva la cuenta del progreso del jugador: qué habilidades ha desbloqueado y qué
    /// objetos ha recogido. También sabe guardar y cargar esa información.
    /// Es un "singleton": solo existe uno y se puede usar desde cualquier script con Instance.
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        // El único ProgressionManager que existe; accesible desde cualquier parte del juego.
        public static ProgressionManager Instance { get; private set; }

        // Las habilidades ya conseguidas. Es un "conjunto", o sea que no se repiten.
        public HashSet<Ability> unlockedAbilities = new HashSet<Ability>();
        // Los objetos que el jugador ha recogido (guardados por su nombre/identificador).
        public List<string> collectedItems = new List<string>();

        // Se ejecuta al nacer el objeto: deja a este como el único ProgressionManager.
        private void Awake()
        {
            // Si no hay ninguno todavía, este es el bueno; si ya había uno, este sobra y se borra.
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // Responde sí o no: ¿el jugador ya tiene esa habilidad?
        public bool HasAbility(Ability ability) => unlockedAbilities.Contains(ability);

        // Da al jugador una habilidad nueva (solo si todavía no la tenía).
        public void UnlockAbility(Ability ability)
        {
            // Si esa habilidad aún no estaba en la lista, la añadimos y lo apuntamos en consola.
            if (!unlockedAbilities.Contains(ability))
            {
                unlockedAbilities.Add(ability);
                Debug.Log($"Unlocked: {ability}");
            }
        }

        // Guarda este progreso en PlayerPrefs (una pequeña memoria que ofrece Unity).
        public void SaveGame()
        {
            // Convertimos los datos de este objeto a texto y los guardamos con la etiqueta "SaveData".
            string data = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("SaveData", data);
            PlayerPrefs.Save();
        }

        // Recupera el progreso guardado en PlayerPrefs (si es que había algo guardado).
        public void LoadGame()
        {
            // Solo cargamos si existe algo guardado con la etiqueta "SaveData".
            if (PlayerPrefs.HasKey("SaveData"))
            {
                string data = PlayerPrefs.GetString("SaveData");
                // Volcamos esos datos de texto otra vez dentro de este objeto.
                JsonUtility.FromJsonOverwrite(data, this);
            }
        }
    }
}
