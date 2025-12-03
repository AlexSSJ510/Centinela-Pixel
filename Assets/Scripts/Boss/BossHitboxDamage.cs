using UnityEngine;

public class BossHitboxDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 2;
    public float knockbackForce = 3f;

    [Header("Visual")]
    public GameObject hitEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar si es el propio boss
        if (collision.transform == transform.parent ||
            collision.transform == transform.root)
            return;

        // Da�ar al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);

                // Aplicar knockback
                ApplyKnockback(player.transform);

                // Efecto visual
                if (hitEffect != null)
                    Instantiate(hitEffect, collision.transform.position, Quaternion.identity);
            }
        }
    }

    void ApplyKnockback(Transform playerTransform)
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }
}