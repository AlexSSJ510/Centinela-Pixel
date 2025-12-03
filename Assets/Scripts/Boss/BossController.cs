using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BossController : MonoBehaviour, IDamageable
{
    [Header("Basic Settings")]
    public int maxHealth = 25;
    public float moveSpeed = 2f;
    public float attackCooldown = 2f;
    public float invulnerabilityTime = 0.5f;

    [Header("Attack Settings")]
    public GameObject meleeHitbox;
    public GameObject stompArea;
    public Transform projectileSpawn;
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;

    [Header("UI References")]
    public Slider healthSlider;
    public GameObject healthBarUI;
    public Text phaseText;

    [Header("References")]
    public Transform player;

    [Header("Phase Settings")]
    public int phase2Threshold = 15;
    public int phase3Threshold = 8;
    public float phase2SpeedMultiplier = 1.3f;
    public float phase3SpeedMultiplier = 1.6f;
    public float phase2CooldownMultiplier = 0.8f;
    public float phase3CooldownMultiplier = 0.6f;

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; 

    private int currentHealth;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private bool isStunned = false;
    private float attackTimer = 0f;
    private Color originalColor;
    private int currentPhase = 1;

    // Variables para animación
    private Vector2 moveDirection;
    private Vector2 lastMoveDirection = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // <-- OBTENER Animator

        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        // Buscar jugador
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Desactivar hitboxes
        if (meleeHitbox != null) meleeHitbox.SetActive(false);
        if (stompArea != null) stompArea.SetActive(false);

        // Inicializar UI
        InitializeHealthUI();
    }

    void InitializeHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Ocultar UI si no hay referencia
        if (healthBarUI != null && !healthBarUI.activeSelf)
            healthBarUI.SetActive(true);
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Cooldown de ataque
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Calcular dirección de movimiento
        if (!isAttacking && !isStunned && attackTimer <= 0)
        {
            CalculateMovement();
            MoveTowardsPlayer();

            // Verificar si está en rango para atacar
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < 3f)
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        UpdateHealthUI();

        UpdateAnimator();

        CheckPhaseChange();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Opcional: cambiar color según salud
        if (healthSlider != null)
        {
            Image fill = healthSlider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                float healthPercent = (float)currentHealth / maxHealth;
                if (healthPercent > 0.6f)
                    fill.color = Color.green;
                else if (healthPercent > 0.3f)
                    fill.color = Color.yellow;
                else
                    fill.color = Color.red;
            }
        }
    }

    void CalculateMovement()
    {
        if (player == null) return;

        moveDirection = (player.position - transform.position).normalized;

        // Guardar última dirección para animaciones
        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection;
        }
    }

    void HidePhaseText()
    {
        if (phaseText != null)
            phaseText.gameObject.SetActive(false);
    }

    void MoveTowardsPlayer()
    {
        if (player == null || isAttacking || isStunned) return;

        float speedMultiplier = 1f;
        if (currentPhase == 2) speedMultiplier = phase2SpeedMultiplier;
        else if (currentPhase >= 3) speedMultiplier = phase3SpeedMultiplier;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * speedMultiplier * Time.deltaTime
        );
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Parámetros de movimiento (para Blend Tree)
        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);
        animator.SetFloat("Speed", moveDirection.magnitude);

        // Parámetros de estado
        animator.SetBool("Phase2", currentPhase >= 2);
        animator.SetBool("Stunned", isStunned);
    }

    IEnumerator Attack()
    {
        if (isAttacking || isStunned) yield break;
        
        isAttacking = true;
        moveDirection = Vector2.zero; // Dejar de moverse al atacar

        // Elegir ataque según fase
        int attackType = ChooseAttackType();

        switch (attackType)
        {
            case 0: // Melee (Heavy)
                yield return MeleeAttack();
                break;
            case 1: // Proyectiles (Shoot)
                yield return ProjectileAttack();
                break;
            case 2: // Stomp
                yield return StompAttack();
                break;
        }

        // Ajustar cooldown según fase
        float cooldownMultiplier = 1f;
        if (currentPhase == 2) cooldownMultiplier = phase2CooldownMultiplier;
        else if (currentPhase >= 3) cooldownMultiplier = phase3CooldownMultiplier;

        attackTimer = attackCooldown * cooldownMultiplier;
        isAttacking = false;
    }

    int ChooseAttackType()
    {
        // Fase 1: Todos los ataques iguales
        // Fase 2: Más probabilidad de proyectiles
        // Fase 3: Más probabilidad de stomp
        if (currentPhase == 1)
        {
            return Random.Range(0, 3);
        }
        else if (currentPhase == 2)
        {
            float rand = Random.value;
            if (rand < 0.5f) return 1; // 50% proyectiles
            else if (rand < 0.75f) return 0; // 25% melee
            else return 2; // 25% stomp
        }
        else // Fase 3
        {
            float rand = Random.value;
            if (rand < 0.4f) return 2; // 40% stomp
            else if (rand < 0.7f) return 1; // 30% proyectiles
            else return 0; // 30% melee
        }
    }

    IEnumerator MeleeAttack()
    {
        // Activar animación
        if (animator != null)
            animator.SetTrigger("Attack_Heavy");

        // Esperar a que la animación muestre telegrafía
        yield return new WaitForSeconds(0.3f);

        // Activar hitbox (en el momento del golpe)
        if (meleeHitbox != null)
        {
            meleeHitbox.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            meleeHitbox.SetActive(false);
        }

        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator ProjectileAttack()
    {
        // Activar animación
        if (animator != null)
            animator.SetTrigger("Attack_Shoot");

        // Esperar a que la animación llegue al frame de disparo
        yield return new WaitForSeconds(0.4f);

        if (projectilePrefab != null && projectileSpawn != null)
        {
            int projectileCount = 8;
            if (currentPhase >= 2) projectileCount = 12;
            if (currentPhase >= 3) projectileCount = 16;

            float angleStep = 360f / projectileCount;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = i * angleStep;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                GameObject projectile = Instantiate(
                    projectilePrefab,
                    projectileSpawn.position,
                    rotation
                );

                Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
                if (projRb != null)
                {
                    Vector2 direction = rotation * Vector2.right;
                    projRb.linearVelocity = direction * projectileSpeed;
                }

                // Pequeño delay entre proyectiles para efecto
                if (i % 2 == 0) yield return new WaitForSeconds(0.05f);
            }
        }

        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator MakeProjectileHoming(Rigidbody2D projRb, GameObject projectile)
    {
        yield return new WaitForSeconds(0.5f);

        if (player != null && projRb != null)
        {
            // Cambiar dirección hacia el jugador
            Vector2 direction = (player.position - projectile.transform.position).normalized;
            projRb.linearVelocity = direction * projectileSpeed;

            // Rotar proyectil
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    IEnumerator StompAttack()
    {
        // Activar animación
        if (animator != null)
            animator.SetTrigger("Attack_Stomp");

        // Temblar durante la carga
        Vector3 originalPos = transform.position;
        for (int i = 0; i < 5; i++)
        {
            transform.position = originalPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
            yield return new WaitForSeconds(0.05f);
        }
        transform.position = originalPos;

        yield return new WaitForSeconds(0.3f);

        // Activar área de stomp
        if (stompArea != null)
        {
            stompArea.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            stompArea.SetActive(false);
        }

        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.5f);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;

        // Efecto visual
        StartCoroutine(DamageFlash());
        StartCoroutine(InvulnerabilityFrame());

        // Aturdir temporalmente (opcional)
        if (damage >= 3) // Solo si el daño es significativo
        {
            StartCoroutine(StunEffect(0.3f));
        }

        // Actualizar UI
        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator InvulnerabilityFrame()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    IEnumerator StunEffect(float duration)
    {
        isStunned = true;
        if (animator != null)
            animator.SetBool("Stunned", true);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        if (animator != null)
            animator.SetBool("Stunned", false);
    }

    void CheckPhaseChange()
    {
        int newPhase = currentPhase;

        if (currentHealth <= phase3Threshold && currentPhase < 3)
        {
            newPhase = 3;
        }
        else if (currentHealth <= phase2Threshold && currentPhase < 2)
        {
            newPhase = 2;
        }

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged(currentPhase);
        }
    }

    void OnPhaseChanged(int newPhase)
    {
        Debug.Log($"BOSS entró a Fase {newPhase}!");

        // Actualizar animator
        if (animator != null)
        {
            animator.SetBool("Phase2", newPhase >= 2);

            // Si quieres un trigger específico para cambio de fase:
            // animator.SetTrigger("PhaseChange");
        }

        // Efecto visual
        StartCoroutine(PhaseChangeEffect());

        // Actualizar texto de fase
        if (phaseText != null)
        {
            phaseText.text = $"FASE {newPhase}";
            phaseText.gameObject.SetActive(true);
            Invoke(nameof(HidePhaseText), 2f);
        }
    }

    IEnumerator PhaseChangeEffect()
    {
        // Flash de color
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Die()
    {
        isDead = true;

        // Desactivar hitboxes
        if (meleeHitbox != null) meleeHitbox.SetActive(false);
        if (stompArea != null) stompArea.SetActive(false);

        // Activar animación de muerte
        if (animator != null)
            animator.SetTrigger("Die");

        // Ocultar UI
        if (healthBarUI != null)
            healthBarUI.SetActive(false);

        Debug.Log("¡BOSS DERROTADO!");

        // Mostrar mensaje de victoria (opcional)
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        // Esperar a que termine la animación de muerte
        yield return new WaitForSeconds(2f);

        // Mostrar texto de victoria
        // (opcional - puedes crear un UI para esto)

        // Esperar unos segundos más
        yield return new WaitForSeconds(2f);

        // Volver al menú principal
        ReturnToMainMenu();
    }

    void ReturnToMainMenu()
    {
        // Cargar la escena del menú principal
        // Asegúrate de que tu escena de menú se llame "MainMenu"
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    IEnumerator DeathEffect()
    {
        // Flash de muerte
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = Color.black;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
        }

        // Hacer transparente
        float fadeTime = 1f;
        float timer = 0f;
        Color startColor = spriteRenderer.color;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    public bool IsAlive() => !isDead;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentPhase() => currentPhase;

    void OnDestroy()
    {
        // Limpiar UI si es necesario
    }
}