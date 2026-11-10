using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roomtemplates : MonoBehaviour
{
//salas
    public GameObject[] BottomDoor;
    public GameObject[] TopDoor;
    public GameObject[] LeftDoor;
    public GameObject[] RightDoor;

    public GameObject ClosedRoom;

//lista
    public List<GameObject> rooms;

//enemigos
    public GameObject Boss;
    public GameObject enemy;

    [Header("Generación de enemigos")]
    [Tooltip("Segundos sin que aparezcan salas nuevas para dar el mapa por terminado")]
    public float settleTime = 1f;

    [Tooltip("No pone enemigos en la primera sala de la lista (normalmente donde empieza el jugador)")]
    public bool skipFirstRoom = true;

    [Header("Altura de aparición")]
    [Tooltip("Altura desde la que se busca el suelo debajo del centro de cada sala")]
    [SerializeField, Min(1f)] private float groundCheckHeight = 25f;
    [Tooltip("Distancia máxima de búsqueda vertical hacia el suelo")]
    [SerializeField, Min(1f)] private float groundCheckDistance = 100f;
    [Tooltip("Pequeño margen para evitar que el enemigo aparezca dentro del suelo")]
    [SerializeField] private float groundSpawnOffset = 0.03f;

    [Header("Dificultad por piso (sube cada 2 pisos)")]
    [Tooltip("Vida extra de los enemigos por cada nivel de dificultad (0.5 = +50%)")]
    public float healthIncreasePerTier = 0.5f;

    [Tooltip("Daño extra de los enemigos por cada nivel de dificultad (0.25 = +25%)")]
    public float damageIncreasePerTier = 0.25f;

    private IEnumerator Start()
    {
        int lastCount = -1;
        float stableTime = 0f;

        while (stableTime < settleTime)
        {
            if (rooms.Count == lastCount)
            {
                stableTime += Time.deltaTime;
            }
            else
            {
                lastCount = rooms.Count;
                stableTime = 0f;
            }

            yield return null;
        }

        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        rooms.RemoveAll(room => room == null);
        if (rooms.Count == 0) return;

        if (Boss != null)
        {
            Vector3 bossPosition = FindGroundSpawnPosition(rooms[rooms.Count - 1].transform.position);
            ApplyDifficulty(Instantiate(Boss, bossPosition, Quaternion.identity));
        }

        if (enemy == null) return;

        int firstRoom = skipFirstRoom ? 1 : 0;
        for (int i = firstRoom; i < rooms.Count - 1; i++)
        {
            Vector3 enemyPosition = FindGroundSpawnPosition(rooms[i].transform.position);
            ApplyDifficulty(Instantiate(enemy, enemyPosition, Quaternion.identity));
        }
    }

    private Vector3 FindGroundSpawnPosition(Vector3 roomPosition)
    {
        Vector3 rayOrigin = roomPosition + Vector3.up * groundCheckHeight;
        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.GetComponentInParent<EnemyClose>() != null) continue;
            if (hit.collider.GetComponentInParent<BossController>() != null) continue;
            if (hit.collider.GetComponentInParent<PlayerController>() != null) continue;

            return new Vector3(roomPosition.x, hit.point.y + groundSpawnOffset, roomPosition.z);
        }

        // Respaldo para salas sin collider de suelo: conserva el nivel del jugador en vez
        // de dejar al enemigo en la altura del marcador central de la sala.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.LogWarning("No se encontró el suelo bajo una sala; se usó la altura del jugador para el spawn.", this);
            return new Vector3(roomPosition.x, player.transform.position.y, roomPosition.z);
        }

        Debug.LogWarning("No se encontró el suelo ni al jugador para calcular la altura del spawn.", this);
        return new Vector3(roomPosition.x, groundSpawnOffset, roomPosition.z);
    }

    private void ApplyDifficulty(GameObject enemyObject)
    {
        int tier = GameSession.DifficultyTier;
        if (tier <= 0 || enemyObject == null) return;

        float healthMultiplier = 1f + healthIncreasePerTier * tier;
        float damageMultiplier = 1f + damageIncreasePerTier * tier;

        Health enemyHealth = enemyObject.GetComponent<Health>();
        if (enemyHealth != null)
            enemyHealth.SetMaxHealth(Mathf.RoundToInt(enemyHealth.MaxHealth * healthMultiplier), true);

        EnemyClose enemyClose = enemyObject.GetComponent<EnemyClose>();
        if (enemyClose != null) enemyClose.SetDamageMultiplier(damageMultiplier);

        BossController bossController = enemyObject.GetComponent<BossController>();
        if (bossController != null) bossController.SetDamageMultiplier(damageMultiplier);
    }
}
