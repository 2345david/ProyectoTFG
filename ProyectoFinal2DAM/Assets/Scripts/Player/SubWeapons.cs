using UnityEngine;

public class SubWeapons : MonoBehaviour
{
    public int ArrowCost = 1;
    public GameObject ArrowPrefab;

    public float shootCooldown = 1f;
    private float currentCooldown = 0f;

    void Update()
    {
        currentCooldown -= Time.deltaTime;
        UseSubWeapon();
    }

    public void UseSubWeapon()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            TryFireArrow();
    }

    public bool TryFireArrow()
    {
        if (SubItems.Instance == null || ArrowCost > SubItems.Instance.subItemsAmount || currentCooldown > 0f)
            return false;

        SubItems.Instance.SubItem(-ArrowCost);

        GameObject sub;

        if (transform.localScale.x < 0)
        {
            sub = Instantiate(ArrowPrefab, transform.position, Quaternion.Euler(0, 0, 50));
            sub.GetComponent<SpriteRenderer>().flipX = true;
            sub.GetComponent<Rigidbody2D>().AddForce(new Vector2(-600f, 0f), ForceMode2D.Force);
        }
        else
        {
            sub = Instantiate(ArrowPrefab, transform.position, Quaternion.Euler(0, 0, -50));
            sub.GetComponent<Rigidbody2D>().AddForce(new Vector2(600f, 0f), ForceMode2D.Force);
        }

        if (AudioManager.instance != null && AudioManager.instance.arrow != null)
            AudioManager.instance.PlayAudio(AudioManager.instance.arrow);

        currentCooldown = shootCooldown;
        return true;
    }
}