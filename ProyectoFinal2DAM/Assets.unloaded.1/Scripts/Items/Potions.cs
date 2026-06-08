using UnityEngine;
using System.Collections;


using System.Collections.Generic;
using Unity.VisualScripting;


public class Potions : MonoBehaviour
{
    public float healthToGive;


    private void OnTriggerEnter2D(Collider2D collison)
    {
        if(collison.gameObject.tag == "Player")
        {
            collison.gameObject.GetComponent<PlayerHealth>().health += healthToGive;

            var uid = GetComponent<UniqueID>();
            if (uid != null)
                WorldStateTracker.Instance.RegisterCollectedItem(uid.ID);

            Destroy(gameObject);
        }
    }
}
