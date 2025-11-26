using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHP = 10;
    private int hp;

    private Animator anim;
    private bool isDead = false;

    private void Awake()
    {
        hp = maxHP;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        hp -= dmg;
        Debug.Log("Player recibi� da�o. HP: " + hp);

        if (hp <= 0)
        {
            Die();
        }
        else
        {
            anim.SetTrigger("Hurt"); // si tienes animaci�n
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("PLAYER MURI�");

        anim.SetTrigger("Die");

        // Desactivar movimiento y ataque
        GetComponent<PlayerController>().enabled = false;

        // opcional: congelar f�sica
        if (TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = Vector2.zero;

        // Desaparecer luego de animaci�n
        Destroy(gameObject, 1.2f);
    }
}