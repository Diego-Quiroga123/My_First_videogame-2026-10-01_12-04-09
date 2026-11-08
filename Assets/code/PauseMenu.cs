using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú de pausa con Esc. Ponlo en un objeto SIEMPRE ACTIVO del Canvas (por ejemplo el propio Canvas)
/// y arrastra el panel y los botones. Va en las escenas Base y Dungeon.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Elementos de la UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button baseButton;      // se oculta solo si ya estás en la base
    [SerializeField] private Button mainMenuButton;

    [Header("Control")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool paused;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (baseButton != null) baseButton.onClick.AddListener(GameSession.GoToBase);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GameSession.GoToMainMenu);
    }

    private void Start()
    {
        if (baseButton != null && SceneManager.GetActiveScene().name == GameSession.BaseScene)
        {
            baseButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(pauseKey)) return;

        if (paused)
        {
            Resume();
        }
        else if (!ExitMenu.IsOpen && !GameOverMenu.IsOpen)
        {
            Pause();
        }
    }

    public void Pause()
    {
        paused = true;
        if (panel != null) panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        paused = false;
        if (panel != null) panel.SetActive(false);

        Time.timeScale = 1f;
    }
}
