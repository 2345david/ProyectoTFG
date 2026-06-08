using UnityEngine;

public class Potions : MonoBehaviour
{
    public float healthToGive;

    private void OnTriggerEnter2D(Collider2D collison)
    {
        if (collison.gameObject.tag == "Player")
        {
            PlayerInventory.Instance.AddPotion(healthToGive);
            if (AudioManager.instance != null && AudioManager.instance.potion != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.potion);
            Destroy(gameObject);
        }
    }
}
