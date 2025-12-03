// DebugEnemy.cs - Ponlo en tu enemigo
using UnityEngine;

public class DebugEnemy : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"ENEMIO: Algo me tocó! Nombre: {collision.name}, Tag: {collision.tag}");

        // Verificar si es el ataque del jugador
        if (collision.name == "AttackHitbox" ||
            collision.transform.parent?.name == "AttackHitbox")
        {
            Debug.Log("¡ES EL ATAQUE DEL JUGADOR!");
        }
    }
}