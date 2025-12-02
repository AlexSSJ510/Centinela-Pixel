using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    public System.Action<int> OnHealthChanged;
    public System.Action OnPlayerDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();

        // Desactivar controles
        GetComponent<PlayerController>().enabled = false;

        // Animación de muerte
        GetComponent<Animator>().SetTrigger("Die");

        Debug.Log("Player murió - Game Over");

        // Reiniciar escena después de tiempo
        Invoke(nameof(Respawn), 2f);
    }

    private void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public bool IsAlive() => currentHealth > 0;
    public int GetCurrentHealth() => currentHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
}