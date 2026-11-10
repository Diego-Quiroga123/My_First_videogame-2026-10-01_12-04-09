using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Barra de vida medieval con color y opacidad ligados a la vida actual.</summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Jugador (si lo dejas vacío, lo busca por el tag 'Player')")]
    [SerializeField] private Health playerHealth;

    [Header("Elementos de la UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image damageFlash;

    [Header("Colores de vida")]
    [SerializeField] private Color healthyColor = new Color(0.22f, 0.68f, 0.28f, 1f);
    [SerializeField] private Color warningColor = new Color(0.9f, 0.68f, 0.16f, 1f);
    [SerializeField] private Color criticalColor = new Color(0.72f, 0.13f, 0.1f, 1f);
    [SerializeField, Range(0f, 1f)] private float opacityFadeThreshold = 0.22f;
    [SerializeField, Range(0.1f, 1f)] private float opacityAtCritical = 0.42f;

    [Header("Ajustes")]
    [SerializeField] private float barSmoothSpeed = 8f;
    [SerializeField] private float flashAlpha = 0.35f;
    [SerializeField] private float flashFadeSpeed = 2f;

    private float targetValue = 1f;
    private Image fillImage;

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

        if (slider.fillRect != null) fillImage = slider.fillRect.GetComponent<Image>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;

        playerHealth.onHealthChanged.AddListener(OnHealthChanged);
        playerHealth.onDamaged.AddListener(OnDamaged);

        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        slider.value = targetValue;
        UpdateFillAppearance();
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
        slider.value = Mathf.Lerp(slider.value, targetValue, barSmoothSpeed * Time.deltaTime);
        UpdateFillAppearance();

        if (damageFlash != null && damageFlash.color.a > 0f)
            SetFlashAlpha(Mathf.MoveTowards(damageFlash.color.a, 0f, flashFadeSpeed * Time.deltaTime));
    }

    private void UpdateFillAppearance()
    {
        if (fillImage == null || slider == null) return;

        float healthPercent = Mathf.Clamp01(slider.value);
        Color color = healthPercent >= 0.5f
            ? Color.Lerp(warningColor, healthyColor, (healthPercent - 0.5f) * 2f)
            : Color.Lerp(criticalColor, warningColor, healthPercent * 2f);

        float opacity = opacityFadeThreshold > 0f
            ? Mathf.Lerp(opacityAtCritical, 1f, Mathf.Clamp01(healthPercent / opacityFadeThreshold))
            : 1f;
        color.a = opacity;
        fillImage.color = color;
    }

    private void OnHealthChanged(int current, int max)
    {
        targetValue = max > 0 ? (float)current / max : 0f;
        if (healthText != null) healthText.text = current + " / " + max;
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
