using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de Game Over: aparece cuando muere el jugador.
/// Ponlo en un objeto SIEMPRE ACTIVO del Canvas (por ejemplo el propio Canvas) y arrastra
/// el panel y los botones. Va en las escenas Dungeon (y Base si ahí también se puede morir).
/// </summary>
public class GameOverMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Jugador (si lo dejas vacío, busca el tag 'Player')")]
    [SerializeField] private Health playerHealth;

    [Header("Elementos de la UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button baseButton;

    [Header("Ajustes")]
    [SerializeField] private float delay = 1.5f; // pausa antes de mostrar el menú al morir

    private void Awake()
    {
        IsOpen = false;

        if (panel != null) panel.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (baseButton != null) baseButton.onClick.AddListener(GameSession.GoToBase);
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerHealth = playerObject.GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("GameOverMenu: no encuentro al jugador con el script Health.", this);
            enabled = false;
            return;
        }

        playerHealth.onDeath.AddListener(OnPlayerDeath);
    }

    private void OnDestroy()
    {
        IsOpen = false;

        if (playerHealth != null) playerHealth.onDeath.RemoveListener(OnPlayerDeath);
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        IsOpen = true;
        if (panel != null) panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Reintentar: vuelve a generar el calabozo en el MISMO piso
    public void Retry()
    {
        GameSession.LoadDungeon();
    }
}
