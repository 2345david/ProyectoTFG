using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    
    public int arrowsToGive;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (SubItems.Instance != null)
                SubItems.Instance.AddToReserve(arrowsToGive);
                
            AudioManager.instance.PlayAudio(AudioManager.instance.recoger_flechas);
            Destroy(gameObject);
        }
    }

}
