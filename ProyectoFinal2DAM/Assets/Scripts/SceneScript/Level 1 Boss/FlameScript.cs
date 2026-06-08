using EnemyScript;
using UnityEngine;

public class FlameScript : MonoBehaviour
{ 
    private float moveSpeed;
    Rigidbody2D rb;
    Vector2 moveDirection;
    PlayerController target;
    void Start()
    {
        moveSpeed = GetComponent<Enemy>().speed;
        rb = GetComponent<Rigidbody2D>();
        target = PlayerController.instance;
        
        moveDirection = (target.transform.position - transform.position).normalized * moveSpeed;
        rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
