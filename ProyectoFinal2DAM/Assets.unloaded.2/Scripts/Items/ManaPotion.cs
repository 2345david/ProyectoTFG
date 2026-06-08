using UnityEngine;

public class ManaPotion : MonoBehaviour
{
    public float manaToGive = 50f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[ManaPotion] Triggered by: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        
        if (collision.CompareTag("Player") || collision.gameObject.name.Contains("Player"))
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.AddManaPotion(manaToGive);
                Debug.Log("[ManaPotion] Added to inventory.");
                
                if (AudioManager.instance != null && AudioManager.instance.potion != null)
                    AudioManager.instance.PlayAudio(AudioManager.instance.potion);
                
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("[ManaPotion] PlayerInventory.Instance is null!");
            }
        }
    }
}
