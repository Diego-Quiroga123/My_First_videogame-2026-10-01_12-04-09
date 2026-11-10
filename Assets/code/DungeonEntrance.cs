using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Entrada a la Dungeon de la escena Base.</summary>
[RequireComponent(typeof(Collider))]
public class DungeonEntrance : MonoBehaviour
{
    [SerializeField] private KeyCode enterKey = KeyCode.E;

    [Header("Mensaje flotante")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string message = "E para ir a la Dungeon";
    [SerializeField, Min(0.5f)] private float promptHeight = 1.8f;
    [SerializeField, Min(0.5f)] private float interactionRadius = 2.5f;

    private Transform floatingPrompt;
    private Transform player;
    private bool playerInside;
    private bool loadingDungeon;

    private void Awake()
    {
        if (promptText == null) CreateFloatingPrompt();
        SetPromptVisible(false);
    }

    private void Start()
    {
        FindPlayer();
        if (promptText != null) promptText.text = message;
    }

    private void Update()
    {
        if (player == null) FindPlayer();

        bool isNear = player != null && IsPlayerNear();
        if (isNear != playerInside)
        {
            playerInside = isNear;
            SetPromptVisible(playerInside);
        }

        if (!playerInside || loadingDungeon || !Input.GetKeyDown(enterKey)) return;

        if (!Application.CanStreamedLevelBeLoaded(GameSession.DungeonScene))
        {
            Debug.LogError("No se pudo entrar: la escena Dungeon no está habilitada en Build Settings.");
            return;
        }

        loadingDungeon = true;
        SetPromptVisible(false);
        GameSession.LoadDungeon();
    }

    private void LateUpdate()
    {
        if (floatingPrompt == null || !floatingPrompt.gameObject.activeInHierarchy) return;
        Camera activeCamera = Camera.main;
        if (activeCamera == null) return;

        // El frente del Canvas debe mirar hacia la cámara para que el texto se renderice.
        Vector3 towardCamera = activeCamera.transform.position - floatingPrompt.position;
        if (towardCamera.sqrMagnitude > 0.001f)
            floatingPrompt.rotation = Quaternion.LookRotation(towardCamera, Vector3.up);
    }

    private void FindPlayer()
    {
        PlayerUpgrades upgrades = FindAnyObjectByType<PlayerUpgrades>();
        if (upgrades != null)
        {
            player = upgrades.transform;
            return;
        }

        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null) player = controller.transform;
    }

    private bool IsPlayerNear()
    {
        Vector3 difference = player.position - transform.position;
        difference.y = 0f;
        return difference.sqrMagnitude <= interactionRadius * interactionRadius;
    }

    private void SetPromptVisible(bool visible)
    {
        if (floatingPrompt != null)
            floatingPrompt.gameObject.SetActive(visible);
        else if (promptText != null)
            promptText.gameObject.SetActive(visible);
    }

    private void CreateFloatingPrompt()
    {
        var canvasObject = new GameObject("DungeonPrompt", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = Vector3.up * promptHeight;
        canvasObject.transform.localScale = Vector3.one * 0.01f;
        floatingPrompt = canvasObject.transform;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        ((RectTransform)canvas.transform).sizeDelta = new Vector2(440f, 72f);

        var backgroundObject = new GameObject("Fondo", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(canvas.transform, false);
        RectTransform backgroundRect = (RectTransform)backgroundObject.transform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundObject.GetComponent<Image>().color = new Color(0.10f, 0.055f, 0.025f, 0.88f);

        var textObject = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(backgroundObject.transform, false);
        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);
        // TextMeshPro en Canvas world-space aparece espejado desde el lado de la cámara.
        textRect.localScale = new Vector3(-1f, 1f, 1f);

        promptText = textObject.GetComponent<TextMeshProUGUI>();
        promptText.text = message;
        promptText.fontSize = 28f;
        promptText.fontStyle = FontStyles.Bold;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 0.87f, 0.63f, 1f);
        promptText.raycastTarget = false;
    }
}

