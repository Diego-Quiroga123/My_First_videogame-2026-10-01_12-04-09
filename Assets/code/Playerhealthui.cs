using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida del jugador en un Canvas. Se actualiza sola con los eventos de Health.
/// Opcional: texto "vida / máxima" y un destello rojo en pantalla al recibir daño.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Jugador (si lo dejas vacío, lo busca por el tag 'Player')")]
    [SerializeField] private Health playerHealth;

    [Header("Elementos de la UI")]
    [SerializeField] private Slider slider;          // la barra
    [SerializeField] private TMP_Text healthText;    // opcional: "80 / 100"
    [SerializeField] private Image damageFlash;      // opcional: imagen roja a pantalla completa

    [Header("Ajustes")]
    [SerializeField] private float barSmoothSpeed = 8f;   // qué tan rápido baja la barra
    [SerializeField] private float flashAlpha = 0.35f;    // opacidad máxima del destello
    [SerializeField] private float flashFadeSpeed = 2f;   // qué tan rápido desaparece

    private float targetValue = 1f;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerHealth = playerObject.GetComponent<Health>();
        }

        if (playerHealth == null || slider == null)
        {
            Debug.LogError("PlayerHealthUI: falta el jugador (con Health) o el Slider.", this);
            enabled = false;
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;

        playerHealth.onHealthChanged.AddListener(OnHealthChanged);
        playerHealth.onDamaged.AddListener(OnDamaged);

        // Estado inicial (la barra aparece ya llena, sin animación)
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        slider.value = targetValue;

        SetFlashAlpha(0f);
    }

    private void OnDestroy()
    {
        if (playerHealth == null) return;
        playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
        playerHealth.onDamaged.RemoveListener(OnDamaged);
    }

    private void Update()
    {
        // La barra baja suavemente hacia el valor real
        slider.value = Mathf.Lerp(slider.value, targetValue, barSmoothSpeed * Time.deltaTime);

        // El destello rojo se desvanece
        if (damageFlash != null && damageFlash.color.a > 0f)
        {
            SetFlashAlpha(Mathf.MoveTowards(damageFlash.color.a, 0f, flashFadeSpeed * Time.deltaTime));
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        targetValue = max > 0 ? (float)current / max : 0f;

        if (healthText != null)
        {
            healthText.text = current + " / " + max;
        }
    }

    private void OnDamaged()
    {
        SetFlashAlpha(flashAlpha);
    }

    private void SetFlashAlpha(float alpha)
    {
        if (damageFlash == null) return;

        Color color = damageFlash.color;
        color.a = alpha;
        damageFlash.color = color;
    }
}