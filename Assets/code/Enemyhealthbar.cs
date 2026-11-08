using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida flotante sobre el enemigo. Ponlo en el enemigo (junto a Health).
/// No necesita crear nada en el Canvas: el script construye la barra por código
/// y la hace mirar siempre a la cámara.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Posición y tamaño")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f); // altura sobre el enemigo (ajústala a tu modelo)
    [SerializeField] private float width = 1.2f;                        // ancho en unidades del mundo
    [SerializeField] private float height = 0.15f;                      // alto en unidades del mundo

    [Header("Colores")]
    [SerializeField] private Color backColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color fullColor = Color.green;  // vida llena
    [SerializeField] private Color lowColor = Color.red;     // vida casi vacía

    [Header("Comportamiento")]
    [SerializeField] private bool hideWhenFull = false;      // true = solo aparece cuando le hacen daño
    [SerializeField] private float smoothSpeed = 10f;

    private Health health;
    private GameObject root;
    private RectTransform fillRect;
    private Image fillImage;
    private Camera cam;

    private float targetValue = 1f;
    private float shownValue = 1f;
    private bool dead;

    private void Awake()
    {
        health = GetComponent<Health>();
        BuildBar();
    }

    private void OnEnable()
    {
        health.onHealthChanged.AddListener(OnHealthChanged);
        health.onDeath.AddListener(OnDeath);

        // Estado actual (por si Health ya avisó antes de que nos suscribiéramos)
        OnHealthChanged(health.CurrentHealth, health.MaxHealth);
        shownValue = targetValue;
    }

    private void OnDisable()
    {
        health.onHealthChanged.RemoveListener(OnHealthChanged);
        health.onDeath.RemoveListener(OnDeath);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root);
    }

    private void LateUpdate()
    {
        if (root == null) return;

        // La barra no es hija del enemigo: así no hereda su escala ni su rotación
        root.transform.position = transform.position + offset;

        if (cam == null) cam = Camera.main;
        if (cam != null) root.transform.rotation = cam.transform.rotation;

        bool visible = !dead && !(hideWhenFull && targetValue >= 1f);
        if (root.activeSelf != visible) root.SetActive(visible);
        if (!visible) return;

        shownValue = Mathf.Lerp(shownValue, targetValue, smoothSpeed * Time.deltaTime);
        fillRect.anchorMax = new Vector2(shownValue, 1f);
        fillImage.color = Color.Lerp(lowColor, fullColor, shownValue);
    }

    private void OnHealthChanged(int current, int max)
    {
        targetValue = max > 0 ? (float)current / max : 0f;
    }

    private void OnDeath()
    {
        dead = true;
    }

    private void BuildBar()
    {
        root = new GameObject(name + " HealthBar", typeof(RectTransform), typeof(Canvas));
        root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

        // El Canvas mide 100 unidades de ancho; la escala lo ajusta al tamaño en el mundo
        float canvasWidth = 100f;
        float canvasHeight = canvasWidth * height / width;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(canvasWidth, canvasHeight);
        root.transform.localScale = Vector3.one * (width / canvasWidth);

        // Fondo
        GameObject back = new GameObject("Back", typeof(RectTransform), typeof(Image));
        Stretch(back, root.transform);
        back.GetComponent<Image>().color = backColor;

        // Relleno (se encoge cambiando su ancla, no necesita sprite)
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillRect = Stretch(fill, root.transform);
        fillImage = fill.GetComponent<Image>();
        fillImage.color = fullColor;
    }

    private static RectTransform Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }
}