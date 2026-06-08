using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldCoins : MonoBehaviour
{

    public float cashToGive;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BankAccount.Instance.Money((float)cashToGive);

            var uid = GetComponent<UniqueID>();
            if (uid != null)
                WorldStateTracker.Instance.RegisterCollectedItem(uid.ID);

            Destroy(gameObject);
        }
    }

}
