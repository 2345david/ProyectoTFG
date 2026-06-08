using System;
using System.Collections;
using EnemyScript;
using UnityEngine;

public class EnemyMovements : MonoBehaviour
{
    private float speed;
    private Rigidbody2D rb;
    Animator anim;

    public bool isStatic;
    public bool isWalker;
    public bool isPatroller;
    public bool walkRight;
    public bool shouldWait;
    public float timeToWait;
    bool isWaiting;
    public bool isKnockedBack;

    public Transform wallCheck, pitCheck, groundCheck;
    public bool wallDetected, pitDetected, groundDetected;
    public float detectionRadius;
    public LayerMask whatIsGround;

    public Transform pointA, pointB;
    bool goToA, goToB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goToA = true;
        speed = GetComponent<Enemy>().speed;
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        pitDetected = !Physics2D.OverlapCircle(pitCheck.position, detectionRadius, whatIsGround);
        wallDetected = Physics2D.OverlapCircle(wallCheck.position, detectionRadius, whatIsGround);
        groundDetected = Physics2D.OverlapCircle(groundCheck.position, detectionRadius, whatIsGround);

        isKnockedBack = GetComponent<EnemyHealth>().isKnockedBack;

        if ((!pitDetected || wallDetected) && groundDetected && !isKnockedBack && !isStatic)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isStatic)
        {
            anim.SetBool("Idle", true);
        }

        if (isWalker)
        {
            anim.SetBool("Idle", false);
            float desiredVelocityX = walkRight ? speed * Time.deltaTime : -speed * Time.deltaTime;
            if (!isKnockedBack)
            {
                rb.linearVelocity = new Vector2(desiredVelocityX, rb.linearVelocity.y);
            }

            // Ajustar dirección del sprite
            transform.localScale = walkRight
                ? new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z)
                : new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (isPatroller)
        {
            if (goToA)
            {
                if (!isWaiting)
                {
                    anim.SetBool("Idle", false);
                    float desiredVelocityX = -speed * Time.deltaTime;
                    if (!isKnockedBack)
                    {
                        rb.linearVelocity = new Vector2(desiredVelocityX, rb.linearVelocity.y);
                    }
                }

    if (Vector2.Distance(transform.position, pointA.position) < 0.2f)
    {
        if (shouldWait)
        {
            StartCoroutine(Waiting());
        }
        
        goToA = false;
        goToB = true;
    }

              
            }

            if (goToB)
            {
                if (!isWaiting)
                {
                    anim.SetBool("Idle", false);
                    float desiredVelocityX = speed * Time.deltaTime;
                    if (!isKnockedBack)
                    {
                        rb.linearVelocity = new Vector2(desiredVelocityX, rb.linearVelocity.y);
                    }
                }

    if (Vector2.Distance(transform.position, pointB.position) < 0.2f)
    {
        if (shouldWait)
        {
            StartCoroutine(Waiting());
        }
        
        goToA = true;
        goToB = false;
    }

                
            }
        }
    }


    IEnumerator Waiting()
    {
        anim.SetBool("Idle", true);
        isWaiting = true;
        yield return new WaitForSeconds(timeToWait);
        isWaiting = false;
        anim.SetBool("Idle", false);
        Flip();
    }

    public void Flip()
    {
        walkRight = !walkRight;
        transform.localScale =
            new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
}