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

    [Header("Dificultad por piso (sube cada 2 pisos)")]
    [Tooltip("Vida extra de los enemigos por cada nivel de dificultad (0.5 = +50%)")]
    public float healthIncreasePerTier = 0.5f;

    [Tooltip("Daño extra de los enemigos por cada nivel de dificultad (0.25 = +25%)")]
    public float damageIncreasePerTier = 0.25f;

    // Antes se esperaban 3 segundos fijos: si el mapa tardaba más en generarse,
    // los enemigos y el jefe quedaban en un mapa a medias. Ahora se espera a que
    // deje de crecer la lista de salas.
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

    void SpawnEnemies()
    {
        // Quita salas que se hayan destruido (evita errores por referencias vacías)
        rooms.RemoveAll(room => room == null);

        if (rooms.Count == 0) return;

        if (Boss != null)
        {
            ApplyDifficulty(Instantiate(Boss, rooms[rooms.Count - 1].transform.position, Quaternion.identity));
        }

        if (enemy == null) return;

        int firstRoom = skipFirstRoom ? 1 : 0;

        for (int i = firstRoom; i < rooms.Count - 1; i++)
        {
            ApplyDifficulty(Instantiate(enemy, rooms[i].transform.position, Quaternion.identity));
        }
    }

    // Hace más fuertes a los enemigos según el piso (GameSession.DifficultyTier)
    void ApplyDifficulty(GameObject enemyObject)
    {
        int tier = GameSession.DifficultyTier;
        if (tier <= 0 || enemyObject == null) return;

        float healthMultiplier = 1f + healthIncreasePerTier * tier;
        float damageMultiplier = 1f + damageIncreasePerTier * tier;

        Health enemyHealth = enemyObject.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.SetMaxHealth(Mathf.RoundToInt(enemyHealth.MaxHealth * healthMultiplier), true);
        }

        EnemyClose enemyClose = enemyObject.GetComponent<EnemyClose>();
        if (enemyClose != null) enemyClose.SetDamageMultiplier(damageMultiplier);

        BossController bossController = enemyObject.GetComponent<BossController>();
        if (bossController != null) bossController.SetDamageMultiplier(damageMultiplier);
    }
}
