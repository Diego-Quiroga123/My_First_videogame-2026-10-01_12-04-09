using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private float attackDistance = 1.2f; // qué tan adelante del jugador está el golpe
    [SerializeField] private float attackRadius = 1f;     // tamaño del golpe
    [SerializeField] private LayerMask enemyLayer;        // asigna la capa "Enemy"

    private Health health;
    private float nextAttackTime;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Update()
    {
        if (health.IsDead) return;

        // Clic izquierdo (Fire1)
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Attack();
        }
    }

    private void Attack()
    {
        Vector3 attackCenter = transform.position + transform.forward * attackDistance;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRadius, enemyLayer);

        // Evita golpear dos veces al mismo enemigo si tiene varios colliders
        HashSet<Health> alreadyHit = new HashSet<Health>();

        foreach (Collider hit in hits)
        {
            Health enemyHealth = hit.GetComponentInParent<Health>();
            if (enemyHealth == null || enemyHealth == health) continue;
            if (!alreadyHit.Add(enemyHealth)) continue;

            enemyHealth.TakeDamage(damage);
        }

        // Aquí luego: animación de golpe, sonido, partículas...
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackDistance, attackRadius);
    }
}
