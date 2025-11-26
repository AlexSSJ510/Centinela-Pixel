using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour, IDamageable
{
    public enum BossPhase { Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;

    [Header("Stats")]
    public float maxHP = 300;
    public float hp;

    [Header("UI - Boss Health Bar")]
    public Slider healthSlider;
    public GameObject healthContainer;

    [Header("References")]
    public Animator anim;
    public Rigidbody2D rb;
    public Transform player;
    private SpriteRenderer sr;

    [Header("Hitboxes")]
    public GameObject meleeHitbox;
    public GameObject stompArea;
    public Transform shootPoint;
    public GameObject projectilePrefab;

    [Header("Movement")]
    public float moveSpeed = 2f;
    private Vector2 moveDir;

    private bool isAttacking = false;

    [Header("Invulnerability")]
    public float invulnTime = 0.6f;
    private bool isInvulnerable = false;
    private Coroutine invulnRoutine;

    // ===================================
    // INIT
    // ===================================
    private void Start()
    {
        hp = maxHP;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = hp;
        }

        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(Phase1Routine());
    }

    private void Update()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        moveDir = dir;

        anim.SetFloat("Horizontal", dir.x);
        anim.SetFloat("Vertical", dir.y);
        anim.SetBool("IsMoving", !isAttacking);
    }

    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // ===================================
    // DAMAGE SYSTEM
    // ===================================
    public void TakeDamage(int dmg)
    {
        if (isInvulnerable) return;

        if (invulnRoutine != null)
            StopCoroutine(invulnRoutine);

        invulnRoutine = StartCoroutine(InvulnerabilityCoroutine());

        hp -= dmg;

        if (healthSlider != null)
            healthSlider.value = hp;

        if (hp <= 0)
        {
            Die();
            return;
        }

        // Fase final
        if (hp <= maxHP * 0.30f && currentPhase != BossPhase.Phase3)
        {
            ChangePhase(BossPhase.Phase3, Phase3Routine());
            return;
        }

        // Segunda fase
        if (hp <= maxHP * 0.70f && currentPhase != BossPhase.Phase2)
        {
            ChangePhase(BossPhase.Phase2, Phase2Routine());
            return;
        }
    }

    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        float blink = 0.1f;

        while (elapsed < invulnTime)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blink);
            elapsed += blink;
        }

        sr.enabled = true;
        isInvulnerable = false;
    }

    private void ChangePhase(BossPhase nextPhase, IEnumerator routine)
    {
        currentPhase = nextPhase;

        StopAllCoroutines();

        sr.enabled = true;

        if (invulnRoutine != null)
        {
            StopCoroutine(invulnRoutine);
            sr.enabled = true;
            isInvulnerable = false;
        }

        StartCoroutine(routine);
    }

    // ===================================
    // PHASE ROUTINES
    // ===================================
    IEnumerator Phase1Routine()
    {
        while (currentPhase == BossPhase.Phase1)
        {
            yield return MoveThenAttack(HeavyAttack());
        }
    }

    IEnumerator Phase2Routine()
    {
        while (currentPhase == BossPhase.Phase2)
        {
            yield return MoveThenAttack(ShootAttack());
        }
    }

    IEnumerator Phase3Routine()
    {
        while (currentPhase == BossPhase.Phase3)
        {
            yield return MoveThenAttack(StompAttack());
        }
    }

    // ===================================
    // MOVEMENT + GENERIC ATTACK FLOW
    // ===================================
    IEnumerator MoveThenAttack(IEnumerator attackRoutine)
    {
        float moveTime = Random.Range(1f, 2f);
        float attackDelay = Random.Range(1.2f, 2f);

        yield return new WaitForSeconds(moveTime);
        yield return attackRoutine;
        yield return new WaitForSeconds(attackDelay);
    }

    // ===================================
    // MELEE ATTACK
    // ===================================
    IEnumerator HeavyAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack_High");

        try
        {
            yield return new WaitForSeconds(0.5f);

            meleeHitbox.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            meleeHitbox.SetActive(false);
        }
        finally
        {
            isAttacking = false;   // SIEMPRE vuelve a false
        }
    }

    // ===================================
    // PROJECTILE ATTACK WITH RANDOM PATTERN
    // ===================================
    IEnumerator ShootAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack_Shoot");

        try
        {
            yield return new WaitForSeconds(0.4f);

            int rnd = Random.Range(0, 2);

            if (rnd == 0)
                yield return StartCoroutine(SpiralProjectilePattern());
            else
                yield return StartCoroutine(FullCircleProjectilePattern());

            yield return new WaitForSeconds(0.4f);
        }
        finally
        {
            isAttacking = false;
        }
    }

    // --------------------------------------
    // SPIRAL PROJECTILE PATTERN
    // --------------------------------------
    IEnumerator SpiralProjectilePattern()
    {
        int bullets = 28;
        float angleStep = 12f;
        float angle = 0f;

        for (int i = 0; i < bullets; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(projectilePrefab, shootPoint.position, rot);
            angle += angleStep;
            yield return new WaitForSeconds(0.03f);
        }
    }

    // --------------------------------------
    // FULL 360° PROJECTILE PATTERN
    // --------------------------------------
    IEnumerator FullCircleProjectilePattern()
    {
        int bullets = 20;

        for (int i = 0; i < bullets; i++)
        {
            float angle = (360f / bullets) * i;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(projectilePrefab, shootPoint.position, rot);
        }

        yield return null;
    }

    // ===================================
    // STOMP ATTACK
    // ===================================
    IEnumerator StompAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack_Stomp");

        try
        {
            yield return new WaitForSeconds(0.7f);

            stompArea.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            stompArea.SetActive(false);
        }
        finally
        {
            isAttacking = false;
        }
    }

    // ===================================
    // DEATH
    // ===================================
    private void Die()
    {
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;

        meleeHitbox.SetActive(false);
        stompArea.SetActive(false);
        GetComponent<Collider2D>().enabled = false;

        enabled = false;

        Destroy(gameObject, 1.5f);
    }
}