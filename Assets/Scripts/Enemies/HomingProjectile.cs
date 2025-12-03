using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("Homing Settings")]
    public int damage = 1;
    public float speed = 5f;
    public float rotationSpeed = 200f; // Velocidad de giro
    public float lifetime = 5f;
    public float initialDelay = 0.3f; // Tiempo antes de empezar a seguir

    [Header("Effects")]
    public GameObject explosionEffect;
    public GameObject trailEffect;

    // Componentes
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private GameObject trailInstance;

    // Estado
    private bool isTracking = false;
    private Vector2 currentDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Direcci�n inicial (hacia donde fue disparado)
        currentDirection = transform.right;

        // Efecto de estela
        if (trailEffect != null)
        {
            trailInstance = Instantiate(trailEffect, transform.position, Quaternion.identity);
            trailInstance.transform.SetParent(transform);
        }

        // Empezar a seguir despu�s de un delay
        Invoke(nameof(StartTracking), initialDelay);

        // Destruir despu�s de lifetime
        Destroy(gameObject, lifetime);
    }

    void StartTracking()
    {
        isTracking = true;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (isTracking)
        {
            // Calcular direcci�n hacia el jugador
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // Rotar gradualmente hacia el jugador
            float rotateAmount = Vector3.Cross(directionToPlayer, currentDirection).z;
            rb.angularVelocity = -rotateAmount * rotationSpeed;

            // Actualizar direcci�n actual
            currentDirection = Vector2.Lerp(currentDirection, directionToPlayer, Time.fixedDeltaTime * 2f);
        }

        // Mover en la direcci�n actual
        rb.linearVelocity = currentDirection * speed;

        // Rotar sprite para que mire en direcci�n del movimiento
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar enemigos
        if (collision.CompareTag("Enemy")) return;

        // Da�ar al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController playerCtrl = collision.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(damage);
            }
        }

        // Efecto de explosi�n
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Destruir efecto de estela
        if (trailInstance != null)
            Destroy(trailInstance);
    }
}