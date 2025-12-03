// StaticTurret.cs
using UnityEngine;
using System.Collections;

public class StaticTurret : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int health = 5;
    public float detectionRange = 8f;
    public float rotationSpeed = 180f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float projectileSpeed = 8f;
    public int projectileDamage = 1;

    [Header("Visual")]
    public Transform turretHead; // Parte que gira
    public Color damageColor = Color.white;

    // Componentes
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Estado
    private bool canShoot = true;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (turretHead == null)
            turretHead = transform; // Usar todo el objeto si no hay cabeza espec�fica
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= detectionRange;

        if (playerInRange)
        {
            // Rotar hacia el jugador
            RotateTowardsPlayer();

            // Disparar si puede
            if (canShoot)
            {
                Shoot();
                StartCoroutine(ShootCooldown());
            }
        }
    }

    void RotateTowardsPlayer()
    {
        Vector2 direction = player.position - turretHead.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        turretHead.rotation = Quaternion.RotateTowards(
            turretHead.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Instanciar proyectil
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Configurar proyectil
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.damage = projectileDamage;
            projScript.speed = projectileSpeed;
            projScript.SetDirection(firePoint.right); // Disparar hacia donde apunta
        }
        else
        {
            // Si no tiene script Projectile, usar Rigidbody2D simple
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.right * projectileSpeed;
            }
        }

        Debug.Log("�Torreta dispar�!");
    }

    IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    // IDamageable implementation
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Efecto visual
        if (spriteRenderer != null)
            StartCoroutine(DamageFlash());

        if (health <= 0)
            Die();
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        Debug.Log("Torreta destruida!");

        // Efectos opcionales antes de destruir
        // Ej: explosion, sonido, etc.

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detecci�n
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Direcci�n de disparo
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, firePoint.right * 2f);
        }
    }
}