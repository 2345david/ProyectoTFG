using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    public float mana;
    public float maxMana = 100f;
    public Image manaBar;

    public static PlayerMana instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        mana = maxMana;
        if (manaBar != null)
        {
            manaBar.fillAmount = mana / maxMana;
        }
    }

    void Update()
    {
        if (manaBar != null)
        {
            manaBar.fillAmount = mana / maxMana;
        }

        if (mana > maxMana)
        {
            mana = maxMana;
        }
    }

    public void UseMana(float amount)
    {
        mana -= amount;
        if (mana < 0) mana = 0;
    }

    public void AddMana(float amount)
    {
        mana += amount;
        if (mana > maxMana) mana = maxMana;
    }

    public bool HasEnoughMana(float amount)
    {
        return mana >= amount;
    }
}
