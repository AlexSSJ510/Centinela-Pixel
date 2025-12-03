// AttackDamage.cs
using UnityEngine;

public class AttackDamage : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar colisiones con el propio jugador
        if (collision.transform == transform.parent ||
            collision.transform == transform.root)
            return;

        // Buscar componentes IDamageable
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
        {
            // Buscar en el padre por si el collider es hijo
            damageable = collision.GetComponentInParent<IDamageable>();
        }

        // Aplicar daño si encontramos un IDamageable
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"Daño aplicado a: {collision.name}");
        }
    }
}