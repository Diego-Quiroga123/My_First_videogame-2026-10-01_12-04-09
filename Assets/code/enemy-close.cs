using UnityEngine;

public class EnemyClose : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform player;

    [Header("Movimiento")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Distancias")]
    [SerializeField] private float detectionRange = 15f; // a qué distancia empieza a perseguirte
    [SerializeField] private float stopDistance = 1.5f;  // a qué distancia se detiene (para atacar, por ejemplo)

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
    }

    private void Update()
    {
        if (player == null) return;

        // Dirección hacia el jugador (ignoramos la altura para que no se incline)
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        // Fuera de rango o ya lo suficientemente cerca: no se mueve
        if (distance > detectionRange || distance <= stopDistance) return;

        direction.Normalize();

        // Gira suavemente hacia el jugador
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Avanza hacia el jugador
        transform.position += direction * speed * Time.deltaTime;
    }

    // Dibuja los rangos en la vista Scene al seleccionar el enemigo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}