using System;
using System.Collections;
using EnemyScript;
using UnityEngine;

public class EnemyMovements : MonoBehaviour
{
    private float speed;
    private Rigidbody2D rb;
    private Animator anim;
    private Enemy enemyScript;

    [Header("Movement Type")]
    public bool isStatic;
    public bool isWalker;
    public bool isPatroller;

    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public bool shouldWait;
    public float timeToWait;
    public float pathThreshold = 1.5f;
    public float verticalThreshold = 1.5f;

    [Header("Visuals")]
    public Transform visualTransform;

    private bool goToA = true;
    private bool goToB = false;
    private bool isWaiting;

    private Vector2 _worldPointA;
    private Vector2 _worldPointB;

    [Header("Detection Settings")]
    public Transform wallCheck, pitCheck, groundCheck;
    public float detectionRadius = 0.2f;
    public LayerMask whatIsGround;

    [Header("State")]
    public bool walkRight;
    public bool isKnockedBack;
    public bool wallDetected, pitDetected, groundDetected;

    private float _initialScaleX;
    private float _flipCooldown = 0.2f;
    private float _lastFlipTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyScript = GetComponent<Enemy>();
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
        
        Transform target = visualTransform != null ? visualTransform : transform;
        _initialScaleX = Mathf.Abs(target.localScale.x);

        if (isPatroller)
        {
            goToA = true;
            goToB = false;
            if (pointA != null) _worldPointA = pointA.position;
            if (pointB != null) _worldPointB = pointB.position;
        }
        }

    private void Update()
    {
        if (enemyScript != null) speed = enemyScript.speed;

        pitDetected = !Physics2D.OverlapCircle(pitCheck.position, detectionRadius, whatIsGround);
        wallDetected = Physics2D.OverlapCircle(wallCheck.position, detectionRadius, whatIsGround);
        groundDetected = Physics2D.OverlapCircle(groundCheck.position, detectionRadius, whatIsGround);

        var health = GetComponent<EnemyHealth>();
        if (health != null) isKnockedBack = health.isKnockedBack;

        // Lógica de giro para Walkers con cooldown para evitar bucles de giro (jitter)
        if (isWalker && !isKnockedBack && groundDetected && (pitDetected || wallDetected))
        {
            if (Time.time > _lastFlipTime + _flipCooldown)
            {
                if (pitDetected) Debug.Log(name + " girando por ABISMO detectado.");
                if (wallDetected) Debug.Log(name + " girando por PARED detectada.");
                
                Flip();
                _lastFlipTime = Time.time;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (isStatic || isKnockedBack)
        {
            if (isStatic)
            {
                anim.SetBool("Idle", true);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        if (isWalker)
        {
            HandleWalkerMovement();
        }
        else if (isPatroller)
        {
            HandlePatrollerMovement();
        }
    }

    private void HandleWalkerMovement()
    {
        anim.SetBool("Idle", false);
        float moveDir = walkRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
        UpdateScale(walkRight);
    }

    private void HandlePatrollerMovement()
    {
        if (isWaiting)
        {
            anim.SetBool("Idle", true);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (pointA == null || pointB == null) return;

        // --- Detección de salida de patrulla precisa (Distancia a Segmento) ---
        Vector2 p1 = _worldPointA;
        Vector2 p2 = _worldPointB;
        Vector2 p = transform.position;

        Vector2 lineDir = p2 - p1;
        float lineLengthSq = lineDir.sqrMagnitude;

        if (lineLengthSq > 0)
        {
            float t = Vector2.Dot(p - p1, lineDir) / lineLengthSq;

            if (t < 0) 
            {
                if (Vector2.Distance(p, p1) > pathThreshold) { SwitchToWalker(); return; }
            }
            else if (t > 1) 
            {
                if (Vector2.Distance(p, p2) > pathThreshold) { SwitchToWalker(); return; }
            }
            else 
            {
                Vector2 projection = p1 + t * lineDir;
                if (Vector2.Distance(p, projection) > verticalThreshold) { SwitchToWalker(); return; }
            }
        }
        // ---------------------------------------------------------------------

        anim.SetBool("Idle", false);
        float moveDir = 0;
        bool targetRight = walkRight;

        float distToA = Mathf.Abs(p.x - p1.x);
        float distToB = Mathf.Abs(p.x - p2.x);

        if (goToA)
        {
            moveDir = -1f;
            targetRight = false;
            if (distToA < 0.2f)
            {
                if (shouldWait) StartCoroutine(Waiting());
                else
                {
                    goToA = false;
                    goToB = true;
                }
            }
        }
        else if (goToB)
        {
            moveDir = 1f;
            targetRight = true;
            if (distToB < 0.2f)
            {
                if (shouldWait) StartCoroutine(Waiting());
                else
                {
                    goToA = true;
                    goToB = false;
                }
            }
        }

        rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
        walkRight = targetRight;
        UpdateScale(walkRight);
    }

    private void SwitchToWalker()
    {
        isPatroller = false;
        isWalker = true;
        Debug.Log(name + " se ha salido de su camino y ahora es un Walker.");
    }

    private void UpdateScale(bool lookingRight)
    {
        Transform target = visualTransform != null ? visualTransform : transform;
        float scaleX = lookingRight ? -_initialScaleX : _initialScaleX;
        target.localScale = new Vector3(scaleX, target.localScale.y, target.localScale.z);
    }

    IEnumerator Waiting()
    {
        isWaiting = true;
        anim.SetBool("Idle", true);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        yield return new WaitForSeconds(timeToWait);
        
        isWaiting = false;
        
        // Solo cambiamos el objetivo DESPUÉS de esperar
        if (goToA)
        {
            goToA = false;
            goToB = true;
        }
        else
        {
            goToA = true;
            goToB = false;
        }
    }

    public void Flip()
    {
        walkRight = !walkRight;
        UpdateScale(walkRight);
    }
}