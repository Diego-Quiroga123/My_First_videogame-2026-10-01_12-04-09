using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Jefe con patrón de ataque. REEMPLAZA a EnemyClose en el prefab del jefe (quita EnemyClose).
/// Persigue al jugador y cada cierto tiempo hace el siguiente ataque del patrón:
///   - Golpe en área: círculo rojo en el suelo, y al terminar el aviso daña a todo lo que esté dentro.
///   - Embestida: franja roja hacia el jugador, luego corre en línea recta y daña si lo atropella.
/// Por debajo de la mitad de vida entra en furia: todo es más rápido.
/// Al morir hace aparecer la puerta de salida (ExitDoor).
/// </summary>
[RequireComponent(typeof(Health))]
public class BossController : MonoBehaviour
{
    public enum BossAttack { Slam, Charge }

    private enum State { Chasing, Attacking, Dead }

    [Header("Objetivo (si lo dejas vacío, busca el tag 'Player')")]
    [SerializeField] private Transform player;

    [Header("Movimiento")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float stopDistance = 3f;
    [Tooltip("Altura del suelo respecto al pivote del jefe. Si el pivote está en el centro del modelo, pon un valor negativo (ej. -1).")]
    [SerializeField] private float groundOffset = 0f;

    [Header("Patrón de ataque (se repite en orden)")]
    [SerializeField] private BossAttack[] pattern =
    {
        BossAttack.Slam, BossAttack.Charge, BossAttack.Slam, BossAttack.Charge, BossAttack.Charge
    };
    [SerializeField] private float timeBetweenAttacks = 2f; // segundos persiguiendo entre un ataque y otro

    [Header("Golpe en área")]
    [SerializeField] private float slamWindup = 1.2f;   // tiempo de aviso
    [SerializeField] private float slamRadius = 5f;
    [SerializeField] private int slamDamage = 30;
    [SerializeField] private float slamRecovery = 0.8f; // se queda quieto después

    [Header("Embestida")]
    [SerializeField] private float chargeWindup = 1f;
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float chargeDistance = 12f;
    [SerializeField] private float chargeHitRadius = 1.5f; // la franja de aviso mide el doble
    [SerializeField] private int chargeDamage = 25;
    [SerializeField] private float chargeRecovery = 1.2f;
    [Tooltip("Opcional: capa de las paredes para que la embestida frene al chocar.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Furia")]
    [SerializeField, Range(0f, 1f)] private float enrageHealthPercent = 0.5f; // se activa con esta vida o menos
    [SerializeField] private float enrageSpeedMultiplier = 1.5f;              // avisos, pausas y recuperación más cortos

    [Header("Al morir")]
    [Tooltip("Puerta que aparece al morir (necesita el script ExitDoor y un Collider Is Trigger). Si lo dejas vacío crea una puerta simple.")]
    [SerializeField] private GameObject exitDoorPrefab;
    [SerializeField] private float doorDelay = 1.5f;

    [Header("Eventos (opcionales, para sonidos y efectos)")]
    public UnityEvent onChargeStart;
    public UnityEvent onSlam;

    private Health health;
    private Health playerHealth;
    private State state = State.Chasing;
    private int patternIndex;
    private float nextAttackTime;
    private float damageMultiplier = 1f;
    private GameObject warning;

    private static Material warningMaterial;

    private bool IsEnraged => health.CurrentHealth <= health.MaxHealth * enrageHealthPercent;
    private float EnrageFactor => IsEnraged ? Mathf.Max(0.1f, enrageSpeedMultiplier) : 1f;

    /// <summary>La usa Roomtemplates para hacer más fuerte al jefe en pisos altos.</summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
    }

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
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        if (player != null) playerHealth = player.GetComponent<Health>();

        nextAttackTime = Time.time + timeBetweenAttacks;
    }

    private void Update()
    {
        if (state != State.Chasing || player == null) return;
        if (playerHealth != null && playerHealth.IsDead) return;

        Vector3 direction = FlatDirection(player.position - transform.position, out float distance);
        if (distance > detectionRange) return;

        // Mira al jugador y se acerca hasta la distancia de parada
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (distance > stopDistance)
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        if (Time.time >= nextAttackTime)
        {
            state = State.Attacking;
            StartCoroutine(AttackRoutine(NextAttack()));
        }
    }

    private BossAttack NextAttack()
    {
        if (pattern == null || pattern.Length == 0) return BossAttack.Slam;

        BossAttack attack = pattern[patternIndex % pattern.Length];
        patternIndex++;
        return attack;
    }

    private IEnumerator AttackRoutine(BossAttack attack)
    {
        if (attack == BossAttack.Slam)
            yield return StartCoroutine(SlamRoutine());
        else
            yield return StartCoroutine(ChargeRoutine());

        if (state == State.Dead) yield break;

        state = State.Chasing;
        nextAttackTime = Time.time + timeBetweenAttacks / EnrageFactor;
    }

    // ---------- Golpe en área ----------

    private IEnumerator SlamRoutine()
    {
        ShowCircleWarning(slamRadius);
        yield return new WaitForSeconds(slamWindup / EnrageFactor);
        DestroyWarning();

        onSlam?.Invoke();

        if (player != null && playerHealth != null)
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;

            if (flat.magnitude <= slamRadius)
            {
                playerHealth.TakeDamage(ScaledDamage(slamDamage));
            }
        }

        yield return new WaitForSeconds(slamRecovery / EnrageFactor);
    }

    // ---------- Embestida ----------

    private IEnumerator ChargeRoutine()
    {
        // La dirección se fija al empezar el aviso: el jugador puede esquivar moviéndose a un lado
        Vector3 direction = FlatDirection(player.position - transform.position, out float distanceToPlayer);
        if (direction == Vector3.zero)
        {
            direction = FlatDirection(transform.forward, out distanceToPlayer);
        }

        transform.rotation = Quaternion.LookRotation(direction);
        ShowLineWarning(direction, chargeDistance, chargeHitRadius * 2f);
        onChargeStart?.Invoke();

        yield return new WaitForSeconds(chargeWindup / EnrageFactor);
        DestroyWarning();

        float traveled = 0f;
        bool hitPlayer = false;

        while (traveled < chargeDistance)
        {
            float step = Mathf.Min(chargeSpeed * Time.deltaTime, chargeDistance - traveled);

            // Frena si hay una pared adelante (solo si asignaste Obstacle Mask)
            if (obstacleMask.value != 0 &&
                Physics.SphereCast(transform.position, chargeHitRadius * 0.5f, direction, out RaycastHit hit,
                                   step + 0.1f, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                break;
            }

            transform.position += direction * step;
            traveled += step;

            // Daña una sola vez por embestida
            if (!hitPlayer && player != null && playerHealth != null)
            {
                Vector3 flat = player.position - transform.position;
                flat.y = 0f;

                if (flat.magnitude <= chargeHitRadius)
                {
                    hitPlayer = true;
                    playerHealth.TakeDamage(ScaledDamage(chargeDamage));
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(chargeRecovery / EnrageFactor);
    }

    // ---------- Muerte y puerta ----------

    private void OnDeath()
    {
        if (state == State.Dead) return;

        state = State.Dead;
        StopAllCoroutines();
        DestroyWarning();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        Vector3 deathPosition = transform.position;

        yield return new WaitForSeconds(doorDelay);

        SpawnExitDoor(deathPosition);
        Destroy(gameObject);
    }

    private void SpawnExitDoor(Vector3 position)
    {
        position.y += groundOffset;

        if (exitDoorPrefab != null)
        {
            Instantiate(exitDoorPrefab, position, Quaternion.identity);
            return;
        }

        // Puerta simple por defecto (cámbiala por tu propio prefab cuando quieras)
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "ExitDoor";
        door.transform.position = position + Vector3.up * 1.5f;
        door.transform.localScale = new Vector3(2.5f, 3f, 0.6f);
        door.GetComponent<Renderer>().material.color = new Color(0.55f, 0.25f, 1f);
        door.GetComponent<Collider>().isTrigger = true;
        door.AddComponent<ExitDoor>();
    }

    // ---------- Avisos en el suelo ----------

    private void ShowCircleWarning(float radius)
    {
        DestroyWarning();
        warning = CreateWarningObject(PrimitiveType.Cylinder);
        warning.transform.position = GroundPosition(transform.position);
        warning.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
    }

    private void ShowLineWarning(Vector3 direction, float length, float width)
    {
        DestroyWarning();
        warning = CreateWarningObject(PrimitiveType.Cube);
        warning.transform.position = GroundPosition(transform.position) + direction * (length * 0.5f);
        warning.transform.rotation = Quaternion.LookRotation(direction);
        warning.transform.localScale = new Vector3(width, 0.02f, length);
    }

    private void DestroyWarning()
    {
        if (warning != null) Destroy(warning);
        warning = null;
    }

    private Vector3 GroundPosition(Vector3 position)
    {
        return new Vector3(position.x, position.y + groundOffset + 0.05f, position.z);
    }

    private static GameObject CreateWarningObject(PrimitiveType type)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = "BossAttackWarning";

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false; // que no estorbe ni golpee a nadie

        if (warningMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default"); // sin luz, con transparencia
            if (shader != null)
            {
                warningMaterial = new Material(shader);
                warningMaterial.color = new Color(1f, 0.1f, 0.1f, 0.4f);
            }
        }

        if (warningMaterial != null)
        {
            obj.GetComponent<Renderer>().sharedMaterial = warningMaterial;
        }

        return obj;
    }

    // ---------- Utilidades ----------

    private int ScaledDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    private static Vector3 FlatDirection(Vector3 vector, out float distance)
    {
        vector.y = 0f;
        distance = vector.magnitude;
        return distance > 0.0001f ? vector / distance : Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * chargeDistance);
    }
}
