// Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public float speed = 10f;
    public float lifetime = 3f;
    public GameObject hitEffect;
    
    private Vector2 direction;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Destruir automáticamente después de un tiempo
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Movimiento constante
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            // Movimiento simple si no hay Rigidbody2D
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
    
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        
        // Rotar el proyectil para que mire en la dirección del movimiento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // No colisionar con la torreta que lo disparó
        if (collision.CompareTag("Enemy") && collision.transform == transform.parent)
            return;
        
        // Dañar al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Proyectil golpeó al jugador!");
            }
        }
        
        // Efecto de impacto
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Destruir proyectil
        Destroy(gameObject);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // También detectar colisiones normales
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}