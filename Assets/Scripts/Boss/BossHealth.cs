using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 30;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider; // asigna un Slider de la UI en el Inspector
    public GameObject healthContainer; // opcional: panel que contiene la barra (activar/desactivar)

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Boss TakeDamage {damage} -> {currentHealth}");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss muerto");
        // Aquí enlaza con BossController para anim de muerte, drop, abrir puerta, etc.
        // e.g. GetComponent<BossController>()?.OnDeath();
    }
}