using UnityEngine;

/// <summary>
/// Ponlo en cada enemigo (junto a Health). Al morir suelta los objetos de la lista,
/// sin tener que llamar a nada desde otro script.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyDrop : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject prefab;                 // Objeto que se suelta (debe tener ItemPickup)
        [Range(0f, 100f)] public float chance = 50f; // Probabilidad de soltarlo (%)
        public int minAmount = 1;                 // Cantidad mínima
        public int maxAmount = 1;                 // Cantidad máxima
    }

    [Header("Lista de objetos que puede soltar")]
    public DropItem[] drops;

    [Header("Ajustes")]
    public float scatterRadius = 0.5f;   // Qué tan separados caen los objetos (en el suelo, ejes X/Z)
    public float spawnHeight = 0.5f;     // Altura sobre el enemigo a la que aparecen

    private Health health;
    private bool alreadyDropped = false;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.onDeath.AddListener(Drop);
    }

    private void OnDisable()
    {
        health.onDeath.RemoveListener(Drop);
    }

    public void Drop()
    {
        if (alreadyDropped || drops == null) return; // Evita soltar dos veces
        alreadyDropped = true;

        foreach (DropItem item in drops)
        {
            if (item.prefab == null) continue;

            // Tirada de probabilidad
            float roll = Random.Range(0f, 100f);
            if (roll > item.chance) continue;

            int amount = Random.Range(item.minAmount, item.maxAmount + 1);

            for (int i = 0; i < amount; i++)
            {
                // Antes el desvío iba en X/Y (hacia arriba y abajo); en tu juego el suelo es X/Z
                Vector2 circle = Random.insideUnitCircle * scatterRadius;
                Vector3 position = transform.position + new Vector3(circle.x, spawnHeight, circle.y);

                Instantiate(item.prefab, position, Quaternion.identity);
            }
        }
    }
}
