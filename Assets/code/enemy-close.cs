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
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Ataque")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Animación")]
    [SerializeField] private string movementParameter = "Speed";
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Muerte")]
    [SerializeField] private float destroyDelay = 0.5f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Health health;
    private Health playerHealth;
    private Animator animator;
    private bool hasSpeedParameter;
    private bool hasAttackParameter;
    private float nextAttackTime;
    private bool isDead;
    private float damageMultiplier = 1f;

    /// <summary>La usa Roomtemplates para hacer más fuertes a los enemigos en pisos altos.</summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == movementParameter && parameter.type == AnimatorControllerParameterType.Float)
                    hasSpeedParameter = true;
                if (parameter.name == attackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                    hasAttackParameter = true;
            }
        }
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
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        if (player != null) playerHealth = player.GetComponent<Health>();
    }

    private void Update()
    {
        if (isDead || player == null || (playerHealth != null && playerHealth.IsDead))
        {
            SetMoving(false);
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            SetMoving(false);
            return;
        }

        direction.Normalize();
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        bool isMoving = distance > stopDistance;
        SetMoving(isMoving);
        if (isMoving) transform.position += direction * speed * Time.deltaTime;

        if (distance <= attackRange) TryAttack();
    }

    private void SetMoving(bool moving)
    {
        if (animator != null && hasSpeedParameter)
            animator.SetFloat(SpeedHash, moving ? 1f : 0f, 0.1f, Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || playerHealth == null) return;

        nextAttackTime = Time.time + attackCooldown;
        if (animator != null && hasAttackParameter) animator.SetTrigger(AttackHash);
        playerHealth.TakeDamage(Mathf.RoundToInt(damage * damageMultiplier));
    }

    private void OnDeath()
    {
        isDead = true;
        SetMoving(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

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
