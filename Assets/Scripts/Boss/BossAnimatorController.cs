using UnityEngine;

public class BossAnimatorController : MonoBehaviour
{
    private Animator animator;
    private Vector2 lastDirection = Vector2.down;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Obtener dirección hacia el jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 dir = (player.transform.position - transform.position).normalized;

            // Actualizar dirección para animaciones
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);

            // Guardar última dirección para cuando esté quieto
            if (dir.magnitude > 0.1f)
                lastDirection = dir;

            // Speed para transiciones (0 = idle, 1 = moving)
            float isMoving = GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0.1f ? 1f : 0f;
            animator.SetFloat("Speed", isMoving);
        }
    }

    // Métodos públicos para que BossController los llame
    public void TriggerHeavyAttack()
    {
        animator.SetTrigger("Attack_Heavy");
    }

    public void TriggerShootAttack()
    {
        animator.SetTrigger("Attack_Shoot");
    }

    public void TriggerStompAttack()
    {
        animator.SetTrigger("Attack_Stomp");
    }

    public void TriggerStunned()
    {
        animator.SetTrigger("Stunned");
    }

    public void TriggerDie()
    {
        animator.SetTrigger("Die");
    }

    public void TriggerPhase2()
    {
        animator.SetTrigger("Phase2");
    }

    public void TriggerPhase3()
    {
        animator.SetTrigger("Phase3");
    }
}