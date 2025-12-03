// HomingProjectileUniversal.cs
using UnityEngine;

public class HomingProjectileUniversal : MonoBehaviour
{
    [Header("Basic Settings")]
    public int damage = 1;
    public float speed = 5f;
    public float lifetime = 4f;
    public bool destroyOnHit = true;

    [Header("Homing Settings")]
    public bool isHoming = true;
    public float rotationSpeed = 180f;
    public float homingDelay = 0.3f;
    public Transform target; // Si es null, busca al jugador

    [Header("Visual Effects")]
    public GameObject hitEffect;
    public Color[] phaseColors; // Para boss phases

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isActive = true;
    private Vector2 currentDirection;
    private float timeAlive = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Buscar target si no está asignado
        if (target == null && isHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        // Dirección inicial
        currentDirection = transform.right;

        // Iniciar homing después de delay
        if (isHoming && homingDelay > 0)
        {
            Invoke(nameof(ActivateHoming), homingDelay);
        }

        // Destruir después de tiempo
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }

    void ActivateHoming()
    {
        // Si es homing pero sin target, buscar jugador
        if (target == null && isHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        timeAlive += Time.fixedDeltaTime;

        if (isHoming && target != null && timeAlive > homingDelay)
        {
            // Calcular dirección hacia el target
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            // Rotar gradualmente
            float rotateAmount = Vector3.Cross(directionToTarget, currentDirection).z;
            rb.angularVelocity = -rotateAmount * rotationSpeed;

            // Actualizar dirección
            currentDirection = Vector2.Lerp(currentDirection, directionToTarget,
                Time.fixedDeltaTime * 2f);
        }

        // Aplicar movimiento
        rb.linearVelocity = currentDirection * speed;

        // Rotar sprite
        if (rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Configure(float newSpeed, float newRotationSpeed, int newDamage, bool homingEnabled = true)
    {
        speed = newSpeed;
        rotationSpeed = newRotationSpeed;
        damage = newDamage;
        isHoming = homingEnabled;
    }

    public void SetPhase(int phase)
    {
        if (phaseColors != null && phaseColors.Length >= phase)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = phaseColors[phase - 1];
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        // Ignorar ciertos tags
        string[] ignoreTags = { "Enemy", "Boss", "Projectile" };
        foreach (string tag in ignoreTags)
        {
            if (collision.CompareTag(tag)) return;
        }

        // Dañar al jugador
        if (collision.CompareTag("Player"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        // Efecto de impacto
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Destruir
        if (destroyOnHit)
        {
            isActive = false;
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Limpiar si es necesario
    }
}