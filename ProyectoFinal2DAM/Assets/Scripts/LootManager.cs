using UnityEngine;

/// <summary>
/// Gestor central de botín (loot). Decide qué objeto soltar cuando un enemigo
/// muere u otro sistema lo solicita, usando un sistema de pesos ponderados.
/// Es un singleton accesible mediante <see cref="instance"/>.
/// </summary>
public class LootManager : MonoBehaviour
{
    // Instancia única (singleton) para acceder al gestor desde cualquier script.
    public static LootManager instance;

    [Header("Loot Prefabs")]
    // Prefabs de los distintos objetos que pueden soltarse al morir un enemigo.
    public GameObject coinPrefab;        // Moneda
    public GameObject arrowPrefab;       // Flecha
    public GameObject healthPotionPrefab; // Poción de vida
    public GameObject manaPotionPrefab;  // Poción de maná

    [Header("Drop Weights (Total should be 100)")]
    // Cuántas probabilidades tiene cada objeto de salir. A más número, más fácil que salga.
    // Lo ideal es que entre todos sumen 100, así se reparten bien los números del 0 al 99.
    public int coinWeight = 40;
    public int arrowWeight = 30;
    public int healthPotionWeight = 15;
    public int manaPotionWeight = 15;

    // Al arrancar, guardamos esta copia para que otros scripts puedan usarla (solo si no había otra).
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    /// <summary>
    /// Suelta un objeto premio al azar en el sitio indicado, haciendo que unos salgan
    /// más a menudo que otros según los números que pusimos arriba.
    /// </summary>
    /// <param name="position">Sitio del mundo donde aparecerá el premio.</param>
    public void DropLoot(Vector3 position)
    {
        // Sacamos un número al azar del 0 al 99, como si tiráramos un dado de 100 caras.
        int randomValue = Random.Range(0, 100);

        GameObject prefabToDrop = null;

        // Repartimos los números 0-99 en tramos seguidos según las probabilidades de arriba:
        // si el número cae en el primer tramo -> sale una moneda
        if (randomValue < coinWeight)
        {
            prefabToDrop = coinPrefab;
        }
        // si cae en el segundo tramo -> sale una flecha
        else if (randomValue < coinWeight + arrowWeight)
        {
            prefabToDrop = arrowPrefab;
        }
        // si cae en el tercer tramo -> sale una poción de vida
        else if (randomValue < coinWeight + arrowWeight + healthPotionWeight)
        {
            prefabToDrop = healthPotionPrefab;
        }
        // el resto de números -> sale una poción de maná
        else
        {
            prefabToDrop = manaPotionPrefab;
        }

        // Solo creamos el objeto si su modelo (prefab) está puesto en el Inspector.
        if (prefabToDrop != null)
        {
            Instantiate(prefabToDrop, position, Quaternion.identity);
        }
    }
}
