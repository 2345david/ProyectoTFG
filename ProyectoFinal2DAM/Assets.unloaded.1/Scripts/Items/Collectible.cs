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
            SubItems.Instance.SubItem(arrowsToGive);

            var uid = GetComponent<UniqueID>();
            if (uid != null)
                WorldStateTracker.Instance.RegisterCollectedItem(uid.ID);

            Destroy(gameObject);
        }
    }

}
