using UnityEngine;

public class Slime : MonoBehaviour, IDamageable
{
    public int maxHealth = 5;
    private int health;

    private void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Slime recibió daño. HP = " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Slime muerto");
        Destroy(gameObject);
    }
}