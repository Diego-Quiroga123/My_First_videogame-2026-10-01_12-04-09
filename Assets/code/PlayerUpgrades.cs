using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Inventario de materiales + mejora de equipo del jugador.
/// Ponlo en el jugador (junto a Health y PlayerAttack).
/// Los materiales se GASTAN al mejorar (normalmente desde una UpgradeZone) y el progreso
/// se conserva al cambiar de escena (base, siguiente piso...) mediante GameSession.
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerUpgrades : MonoBehaviour
{
    [Header("Inventario (solo cuenta la primera vez que empieza la partida)")]
    [SerializeField] private int materials = 0;          // materiales que tiene ahora el jugador

    [Header("Costo de la mejora")]
    [SerializeField] private int baseCost = 5;            // materiales para pasar del nivel 0 al 1
    [SerializeField] private int costIncreasePerLevel = 2; // cuánto sube el costo en cada nivel
    [SerializeField] private int maxEquipmentLevel = 10;

    [Header("Qué mejora por nivel")]
    [SerializeField] private int damagePerLevel = 3;      // daño extra por cada nivel
    [SerializeField] private int healthPerLevel = 20;     // vida máxima extra por cada nivel

    [Header("Eventos (opcionales, para UI, sonidos, etc.)")]
    public UnityEvent<int> onItemsChanged;      // (materialesActuales)
    public UnityEvent<int> onEquipmentLevelUp;  // (nuevoNivel)

    public int Materials => materials;
    public int EquipmentLevel { get; private set; }
    public int MaxEquipmentLevel => maxEquipmentLevel;
    public int DamagePerLevel => damagePerLevel;
    public int HealthPerLevel => healthPerLevel;

    public bool IsMaxLevel => EquipmentLevel >= maxEquipmentLevel;
    public int NextLevelCost => baseCost + costIncreasePerLevel * EquipmentLevel;
    public bool CanUpgrade => !IsMaxLevel && materials >= NextLevelCost;

    private Health health;
    private PlayerAttack attack;
    private int baseMaxHealth;

    private void Awake()
    {
        health = GetComponent<Health>();
        attack = GetComponent<PlayerAttack>();
        baseMaxHealth = health.MaxHealth;

        if (GameSession.HasPlayerData)
        {
            // Viene de otra escena: recupera lo que tenía
            materials = GameSession.Materials;
            EquipmentLevel = Mathf.Min(GameSession.EquipmentLevel, maxEquipmentLevel);
        }
        else
        {
            // Primera vez: se queda con los valores del Inspector
            GameSession.HasPlayerData = true;
            Save();
        }
    }

    private void Start()
    {
        ApplyLevel(EquipmentLevel, false);
    }

    /// <summary>Lo llama ItemPickup al recoger un material.</summary>
    public void AddItems(int amount)
    {
        if (amount <= 0) return;

        materials += amount;
        Save();
        onItemsChanged?.Invoke(materials);
    }

    /// <summary>Gasta los materiales y sube un nivel. Devuelve false si no alcanza o ya está al máximo.</summary>
    public bool TryUpgrade()
    {
        if (!CanUpgrade) return false;

        materials -= NextLevelCost; // el costo se calcula con el nivel ACTUAL, antes de subirlo
        onItemsChanged?.Invoke(materials);

        ApplyLevel(EquipmentLevel + 1, true);
        Save();
        return true;
    }

    private void Save()
    {
        GameSession.Materials = materials;
        GameSession.EquipmentLevel = EquipmentLevel;
    }

    private void ApplyLevel(int level, bool announce)
    {
        EquipmentLevel = level;

        // Daño: PlayerAttack suma este bonus a su daño base
        if (attack != null)
        {
            attack.SetBonusDamage(damagePerLevel * level);
        }

        // Vida: sube el máximo y cura justo lo que subió
        int oldMax = health.MaxHealth;
        int newMax = baseMaxHealth + healthPerLevel * level;
        health.SetMaxHealth(newMax);
        if (newMax > oldMax)
        {
            health.Heal(newMax - oldMax);
        }

        if (announce)
        {
            onEquipmentLevelUp?.Invoke(level);
        }
    }
}
