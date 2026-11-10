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
    [SerializeField] private RuntimeAnimatorController movementController;

    private Health health;
    private Animator animator;
    private float nextAttackTime;
    private int bonusDamage; // lo pone PlayerUpgrades según el nivel del equipo
    private static readonly int AttackState = Animator.StringToHash("Base Layer.Attack");

    // Buffers reutilizables: evitan crear arrays/listas nuevos en cada golpe
    private readonly Collider[] hitBuffer = new Collider[32];
    private readonly HashSet<Health> alreadyHit = new HashSet<Health>();

    /// <summary>Daño total = daño base + bonus del equipo.</summary>
    public int Damage => damage + bonusDamage;

    public void SetBonusDamage(int bonus)
    {
        bonusDamage = Mathf.Max(0, bonus);
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();

        // El prefab del jugador puede conservar un Animator Controller anterior.
        // Asignamos el del jugador aquí también para que movimiento y ataque usen el mismo.
        if (animator != null && movementController != null)
        {
            animator.runtimeAnimatorController = movementController;
            animator.applyRootMotion = false;
        }
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
        if (animator != null && animator.HasState(0, AttackState))
        {
            // Reinicia la animación en cada clic, incluso si el jugador vuelve a atacar
            // antes de que termine el golpe anterior.
            animator.Play(AttackState, 0, 0f);
        }

        Vector3 attackCenter = transform.position + transform.forward * attackDistance;
        int hitCount = Physics.OverlapSphereNonAlloc(attackCenter, attackRadius, hitBuffer, enemyLayer);

        // Evita golpear dos veces al mismo enemigo si tiene varios colliders
        alreadyHit.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Health enemyHealth = hitBuffer[i].GetComponentInParent<Health>();
            if (enemyHealth == null || enemyHealth == health) continue;
            if (!alreadyHit.Add(enemyHealth)) continue;

            enemyHealth.TakeDamage(Damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackDistance, attackRadius);
    }
}
