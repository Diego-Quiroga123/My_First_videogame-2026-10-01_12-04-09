using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú que sale al cruzar la puerta del jefe: volver a la base o seguir al siguiente piso.
/// Ponlo en un objeto SIEMPRE ACTIVO del Canvas (por ejemplo el propio Canvas) y arrastra
/// el panel y los botones. El script esconde el panel solo al empezar.
/// </summary>
public class ExitMenu : MonoBehaviour
{
    public static ExitMenu Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [Header("Elementos de la UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button baseButton;
    [SerializeField] private Button nextFloorButton;

    [Header("Textos (opcionales)")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text hintText;

    private void Awake()
    {
        Instance = this;
        IsOpen = false;

        if (panel != null) panel.SetActive(false);
        if (baseButton != null) baseButton.onClick.AddListener(GoToBase);
        if (nextFloorButton != null) nextFloorButton.onClick.AddListener(NextFloor);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsOpen = false;
        }
    }

    public void Show()
    {
        if (IsOpen) return;
        IsOpen = true;

        int floor = GameSession.Floor;

        if (titleText != null)
        {
            titleText.text = "¡Piso " + floor + " completado!";
        }

        if (hintText != null)
        {
            // El siguiente piso empieza un nivel nuevo de dificultad cuando este es el último de su grupo
            bool nextIsStronger = floor % GameSession.FloorsPerTier == 0;
            hintText.text = nextIsStronger ? "¡Cuidado! Los enemigos del siguiente piso serán más fuertes." : "";
        }

        if (panel != null) panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void GoToBase()
    {
        GameSession.GoToBase();
    }

    public void NextFloor()
    {
        GameSession.NextFloor();
    }
}
