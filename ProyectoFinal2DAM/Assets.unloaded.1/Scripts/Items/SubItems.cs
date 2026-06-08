using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubItems : MonoBehaviour
{
    
    public Text subItemAmountText;

    public int subItemsAmount;

    public static SubItems Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        subItemAmountText.text = "x " + subItemsAmount.ToString();
    }

    public void SubItem(int subItemAmount)
    {
        subItemsAmount += subItemAmount;
        subItemAmountText.text = "x " + subItemsAmount.ToString();
    }

}
