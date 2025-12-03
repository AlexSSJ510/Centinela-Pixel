using UnityEngine;

public class BasicEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int damage = 1;
    public float moveSpeed = 2f;
    public float attackCooldown = 1f;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1f;

    [Header("Visual")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    // Estado
    private int currentHealth;
    private bool isChasing = false;
    private bool canAttack = true;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Detectar jugador
        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;

            // Perseguir al jugador
            if (distanceToPlayer > attackRange)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                // Est� en rango de ataque
                rb.linearVelocity = Vector2.zero;

                // Atacar si puede
                if (canAttack)
                {
                    AttackPlayer();
                }
            }
        }
        else
        {
            isChasing = false;
            rb.linearVelocity = Vector2.zero;
        }

        // Rotar sprite hacia el jugador (opcional)
        if (isChasing && spriteRenderer != null)
        {
            Vector2 direction = player.position - transform.position;
            if (direction.x > 0)
                spriteRenderer.flipX = false;
            else if (direction.x < 0)
                spriteRenderer.flipX = true;
        }
    }

    void AttackPlayer()
    {
        // Enviar da�o al jugador
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            StartCoroutine(AttackCooldown());
        }
    }

    System.Collections.IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Implementaci�n de IDamageable
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Efecto visual de da�o
        if (spriteRenderer != null)
            StartCoroutine(DamageFlash());

        // Morir si la salud llega a 0
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        // Efectos antes de morir (opcional)
        Debug.Log("Enemigo derrotado!");

        // Destruir el GameObject
        Destroy(gameObject);
    }

    // Para debug visual en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de detecci�n
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Da�o por contacto (opcional - descomenta si quieres)
        /*
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }
        */
    }
}