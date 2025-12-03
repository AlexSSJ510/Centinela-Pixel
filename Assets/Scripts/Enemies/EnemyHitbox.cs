// EnemyHitbox.cs
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 1;
    public float knockbackForce = 5f;

    private BasicEnemy enemyParent;

    void Start()
    {
        enemyParent = GetComponentInParent<BasicEnemy>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo da�ar al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);

                // Aplicar knockback (opcional)
                ApplyKnockback(player.transform);
            }
        }

        // Si el jugador ataca a este enemigo
        if (collision.gameObject.name == "AttackHitbox" && enemyParent != null)
        {
            // El PlayerHitbox ya maneja esto, pero por si acaso
            enemyParent.TakeDamage(1);
        }
    }

    void ApplyKnockback(Transform playerTransform)
    {
        // Empujar al jugador en direcci�n opuesta
        Vector2 knockbackDirection = (playerTransform.position - transform.position).normalized;
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero; // Resetear velocidad
            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}