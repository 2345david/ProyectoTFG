using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;

    [Header("Loot Prefabs")]
    public GameObject coinPrefab;
    public GameObject arrowPrefab;
    public GameObject healthPotionPrefab;
    public GameObject manaPotionPrefab;

    [Header("Drop Weights (Total should be 100)")]
    public int coinWeight = 40;
    public int arrowWeight = 30;
    public int healthPotionWeight = 15;
    public int manaPotionWeight = 15;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void DropLoot(Vector3 position)
    {
        int randomValue = Random.Range(0, 100);

        GameObject prefabToDrop = null;

        if (randomValue < coinWeight)
        {
            prefabToDrop = coinPrefab;
        }
        else if (randomValue < coinWeight + arrowWeight)
        {
            prefabToDrop = arrowPrefab;
        }
        else if (randomValue < coinWeight + arrowWeight + healthPotionWeight)
        {
            prefabToDrop = healthPotionPrefab;
        }
        else
        {
            prefabToDrop = manaPotionPrefab;
        }

        if (prefabToDrop != null)
        {
            Instantiate(prefabToDrop, position, Quaternion.identity);
        }
    }
}
