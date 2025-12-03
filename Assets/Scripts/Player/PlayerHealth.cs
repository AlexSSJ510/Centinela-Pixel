using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    public float invulnerabilityTime = 1f;

    [Header("UI Reference")]
    public Slider healthSlider;
    public Text healthText;

    private int currentHealth;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateUI();

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
            Die();
    }

    System.Collections.IEnumerator DamageFlash()
    {
        isInvulnerable = true;

        // Parpadeo
        float timer = 0f;
        while (timer < invulnerabilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    void Die()
    {
        Debug.Log("Player murió");

        // Reiniciar escena
        Invoke(nameof(Respawn), 2f);
    }

    void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"{currentHealth}/{maxHealth}";
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsAlive() => currentHealth > 0;
}