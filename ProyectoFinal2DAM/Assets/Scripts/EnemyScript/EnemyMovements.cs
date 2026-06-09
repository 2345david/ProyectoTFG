using System.Collections;
using EnemyScript;
using UnityEngine;

/// <summary>
/// Mueve a un enemigo que camina por el suelo (juego 2D). Tiene tres formas de moverse:
/// - Static (quieto): no se mueve.
/// - Walker (andador): camina y se da la vuelta cuando encuentra una pared o un borde.
/// - Patroller (vigilante): va y viene entre dos puntos; si se aleja mucho de su camino,
///   se convierte solo en andador.
/// </summary>
public class EnemyMovements : MonoBehaviour
{
    // Lo rápido que se mueve ahora (se copia de Enemy.speed en cada frame).
    private float speed;
    // Componentes que guardamos al empezar para no buscarlos cada vez.
    private Rigidbody2D rb;
    private Animator anim;
    private Enemy enemyScript;

    [Header("Movement Type")]
    // Modo quieto.
    public bool isStatic;
    // Modo andador (camina y se da la vuelta ante obstáculos).
    public bool isWalker;
    // Modo vigilante (va y viene entre dos puntos).
    public bool isPatroller;

    [Header("Patrol Settings")]
    // Los dos puntos entre los que va y viene.
    public Transform pointA;
    public Transform pointB;
    // Si debe pararse un momento al llegar a cada punto.
    public bool shouldWait;
    // Cuántos segundos espera en cada punto (si shouldWait está activado).
    public float timeToWait;
    // Cuánto puede alejarse hacia los lados de su camino antes de pasar a andador.
    public float pathThreshold = 1.5f;
    // Cuánto puede alejarse hacia arriba/abajo de su camino antes de pasar a andador.
    public float verticalThreshold = 1.5f;

    [Header("Visuals")]
    // El dibujo que se voltea al cambiar de dirección (si está vacío, se usa este mismo objeto).
    public Transform visualTransform;

    // Hacia qué punto va ahora: hacia A o hacia B.
    private bool goToA = true;
    private bool goToB = false;
    // Verdadero mientras está parado esperando en un punto.
    private bool isWaiting;

    // La posición real (en el mundo) de los dos puntos, guardada al empezar.
    private Vector2 _worldPointA;
    private Vector2 _worldPointB;

    [Header("Detection Settings")]
    // Los tres "sensores": para ver la pared, el borde (abismo) y el suelo.
    public Transform wallCheck, pitCheck, groundCheck;
    // El tamaño del círculo que usan los sensores para detectar.
    public float detectionRadius = 0.2f;
    // Qué capas cuentan como "suelo".
    public LayerMask whatIsGround;

    [Header("State")]
    // Hacia dónde camina: verdadero = a la derecha.
    public bool walkRight;
    // Verdadero mientras lo están empujando (lo avisa EnemyHealth).
    public bool isKnockedBack;
    // Lo que han detectado los sensores en este frame.
    public bool wallDetected, pitDetected, groundDetected;

    // El tamaño original del dibujo (en X), para poder voltearlo bien.
    private float _initialScaleX;
    // Tiempo mínimo entre giros, para que no se quede dando vueltas sin parar (temblando).
    private float _flipCooldown = 0.2f;
    // Cuándo fue el último giro.
    private float _lastFlipTime;

    // Al empezar: guardamos componentes, ajustamos el cuerpo físico y preparamos la patrulla.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyScript = GetComponent<Enemy>();
        
        // Evitamos que el enemigo gire sobre sí mismo y que el cuerpo físico se "duerma".
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
        
        // Guardamos el tamaño original del dibujo que vamos a voltear.
        Transform target = visualTransform != null ? visualTransform : transform;
        _initialScaleX = Mathf.Abs(target.localScale.x);

        // Si es vigilante, decidimos que empiece yendo hacia A y guardamos las posiciones de A y B.
        if (isPatroller)
        {
            goToA = true;
            goToB = false;
            if (pointA != null) _worldPointA = pointA.position;
            if (pointB != null) _worldPointB = pointB.position;
        }
        }

    // Cada frame: mira con los sensores qué hay alrededor y, si es andador, decide si girar.
    private void Update()
    {
        // Copiamos la velocidad desde la ficha de datos del enemigo.
        if (enemyScript != null) speed = enemyScript.speed;

        // Miramos con los sensores: si hay borde delante (sin suelo), si hay pared y si hay suelo bajo los pies.
        pitDetected = !Physics2D.OverlapCircle(pitCheck.position, detectionRadius, whatIsGround);
        wallDetected = Physics2D.OverlapCircle(wallCheck.position, detectionRadius, whatIsGround);
        groundDetected = Physics2D.OverlapCircle(groundCheck.position, detectionRadius, whatIsGround);

        // Preguntamos a EnemyHealth si ahora mismo lo están empujando.
        var health = GetComponent<EnemyHealth>();
        if (health != null) isKnockedBack = health.isKnockedBack;

        // Si es andador y hay pared o borde delante, se da la vuelta (esperando un poco entre giros para no temblar).
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

    // Aquí se aplica el movimiento físico (Unity llama a esto a ritmo fijo, ideal para mover el cuerpo).
    private void FixedUpdate()
    {
        if (rb == null) return;

        // Si está quieto o lo están empujando, no aplicamos su movimiento normal.
        if (isStatic || isKnockedBack)
        {
            // El quieto se queda parado, sin moverse a los lados.
            if (isStatic)
            {
                if (anim != null) anim.SetBool("Idle", true);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        // Movemos al enemigo según el modo que tenga activado.
        if (isWalker)
        {
            HandleWalkerMovement();
        }
        else if (isPatroller)
        {
            HandlePatrollerMovement();
        }
    }

    /// <summary>Movimiento del andador: camina hacia donde mira, siempre a la misma velocidad.</summary>
    private void HandleWalkerMovement()
    {
        if (anim != null) anim.SetBool("Idle", false);
        float moveDir = walkRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
        UpdateScale(walkRight);
    }

    /// <summary>
    /// Movimiento del vigilante: va y viene entre el punto A y el punto B. Si se separa
    /// demasiado de la línea que une A y B, deja de vigilar y pasa a ser andador.
    /// </summary>
    private void HandlePatrollerMovement()
    {
        // Si está esperando en un punto, se queda quieto y no avanza.
        if (isWaiting)
        {
            if (anim != null) anim.SetBool("Idle", true);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (pointA == null || pointB == null) return;

        // --- Comprobamos si se ha salido demasiado de su camino ---
        // p1 y p2 son los dos puntos; p es donde está el enemigo ahora.
        Vector2 p1 = _worldPointA;
        Vector2 p2 = _worldPointB;
        Vector2 p = transform.position;

        // Calculamos la línea que va de A a B.
        Vector2 lineDir = p2 - p1;
        float lineLengthSq = lineDir.sqrMagnitude;

        if (lineLengthSq > 0)
        {
            // "t" nos dice en qué parte de la línea está el enemigo: antes de A, entre A y B, o pasado B.
            float t = Vector2.Dot(p - p1, lineDir) / lineLengthSq;

            if (t < 0) 
            {
                // Se ha pasado antes del punto A: miramos lo lejos que está de A.
                if (Vector2.Distance(p, p1) > pathThreshold) { SwitchToWalker(); return; }
            }
            else if (t > 1) 
            {
                // Se ha pasado más allá del punto B: miramos lo lejos que está de B.
                if (Vector2.Distance(p, p2) > pathThreshold) { SwitchToWalker(); return; }
            }
            else 
            {
                // Está entre A y B: miramos si se ha alejado de la línea (por arriba o por abajo).
                Vector2 projection = p1 + t * lineDir;
                if (Vector2.Distance(p, projection) > verticalThreshold) { SwitchToWalker(); return; }
            }
        }
        // ----------------------------------------------------------

        if (anim != null) anim.SetBool("Idle", false);
        float moveDir = 0;
        bool targetRight = walkRight;

        // Miramos lo cerca que está de cada punto para saber si ya ha llegado.
        float distToA = Mathf.Abs(p.x - p1.x);
        float distToB = Mathf.Abs(p.x - p2.x);

        if (goToA)
        {
            // Va hacia A (a la izquierda).
            moveDir = -1f;
            targetRight = false;
            if (distToA < 0.2f)
            {
                // Llegó a A: o espera, o cambia para ir ahora hacia B.
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
            // Va hacia B (a la derecha).
            moveDir = 1f;
            targetRight = true;
            if (distToB < 0.2f)
            {
                // Llegó a B: o espera, o cambia para ir ahora hacia A.
                if (shouldWait) StartCoroutine(Waiting());
                else
                {
                    goToA = true;
                    goToB = false;
                }
            }
        }

        // Aplicamos la velocidad y hacemos que mire hacia donde va.
        rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
        walkRight = targetRight;
        UpdateScale(walkRight);
    }

    /// <summary>Convierte al enemigo de vigilante a andador (cuando se aleja de su camino).</summary>
    private void SwitchToWalker()
    {
        isPatroller = false;
        isWalker = true;
        Debug.Log(name + " se ha salido de su camino y ahora es un Walker.");
    }

    /// <summary>Voltea el dibujo del enemigo para que mire a la izquierda o a la derecha.</summary>
    private void UpdateScale(bool lookingRight)
    {
        Transform target = visualTransform != null ? visualTransform : transform;
        // Para mirar a la derecha le damos la vuelta al tamaño original (signo negativo).
        float scaleX = lookingRight ? -_initialScaleX : _initialScaleX;
        target.localScale = new Vector3(scaleX, target.localScale.y, target.localScale.z);
    }

    /// <summary>Hace que el enemigo espere un rato en un punto y, al terminar, cambie de dirección.</summary>
    IEnumerator Waiting()
    {
        isWaiting = true;
        if (anim != null) anim.SetBool("Idle", true);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        yield return new WaitForSeconds(timeToWait);
        
        isWaiting = false;
        
        // Solo cambiamos de dirección DESPUÉS de haber esperado.
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

    /// <summary>Cambia la dirección de avance (de izquierda a derecha o al revés) y voltea el dibujo.</summary>
    public void Flip()
    {
        walkRight = !walkRight;
        UpdateScale(walkRight);
    }
}