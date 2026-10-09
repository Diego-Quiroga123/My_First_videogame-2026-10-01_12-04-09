using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente de vida reutilizable. Ponlo en el jugador y en cada enemigo.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;

    [Header("Invulnerabilidad tras recibir daño (0 = desactivada)")]
    [SerializeField] private float invulnerabilityTime = 0.5f;

    [Header("Eventos (opcionales, para UI, sonidos, etc.)")]
    public UnityEvent<int, int> onHealthChanged; // (vidaActual, vidaMaxima)
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    private float lastDamageTime = -999f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        // Para que la UI se inicialice con la vida completa
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        if (Time.time - lastDamageTime < invulnerabilityTime) return;

        lastDamageTime = Time.time;
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);

        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
        onDamaged?.Invoke();

        if (CurrentHealth == 0)
        {
            onDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // Útil para mejoras de la herrería (+vida máxima)
    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = Mathf.Max(1, newMax);
        CurrentHealth = healToFull ? maxHealth : Mathf.Min(CurrentHealth, maxHealth);
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}