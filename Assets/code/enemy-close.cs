using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyClose : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform player;

    [Header("Movimiento")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Distancias")]
    [SerializeField] private float detectionRange = 15f; // a qué distancia empieza a perseguirte
    [SerializeField] private float stopDistance = 1.5f;  // a qué distancia se detiene

    [Header("Ataque")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.8f;    // debe ser >= stopDistance
    [SerializeField] private float attackCooldown = 1f;   // segundos entre golpes

    [Header("Muerte")]
    [SerializeField] private float destroyDelay = 0.5f;

    private Health health;
    private Health playerHealth;
    private float nextAttackTime;
    private bool isDead;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.onDeath.AddListener(OnDeath);
    }

    private void OnDisable()
    {
        health.onDeath.RemoveListener(OnDeath);
    }

    private void Start()
    {
        // Si no asignaste el jugador en el Inspector, lo busca por la etiqueta "Player"
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }
    }

    private void Update()
    {
        if (isDead || player == null) return;

        // Si el jugador murió, el enemigo deja de actuar
        if (playerHealth != null && playerHealth.IsDead) return;

        // Dirección hacia el jugador (ignoramos la altura para que no se incline)
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        // Fuera de rango de detección: no hace nada
        if (distance > detectionRange) return;

        direction.Normalize();

        // Siempre mira al jugador mientras lo detecta
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Avanza solo si aún no está lo bastante cerca
        if (distance > stopDistance)
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        // Ataca si está en rango y el cooldown terminó
        if (distance <= attackRange)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || playerHealth == null) return;

        nextAttackTime = Time.time + attackCooldown;
        playerHealth.TakeDamage(damage);
        // Aquí luego: animación de ataque, sonido...
    }

    private void OnDeath()
    {
        isDead = true;

        // Desactiva el collider para que no estorbe ni reciba más golpes
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Aquí luego: soltar mineral (DropOnDeath), animación, partículas...
        Destroy(gameObject, destroyDelay);
    }

    // Dibuja los rangos en la vista Scene al seleccionar el enemigo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}