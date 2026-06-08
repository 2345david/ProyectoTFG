using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public float healAmount = 50f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.AddPotion(healAmount);
                if (AudioManager.instance != null && AudioManager.instance.potion != null)
                    AudioManager.instance.PlayAudio(AudioManager.instance.potion);
                
                Destroy(gameObject);
            }
        }
    }
}
