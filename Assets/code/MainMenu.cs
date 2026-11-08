using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú principal. Ponlo en cualquier objeto de la escena MainMenu y arrastra los dos botones.
/// "Jugar" empieza una partida nueva y te lleva a la base.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playButton != null) playButton.onClick.AddListener(Play);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
    }

    public void Play()
    {
        GameSession.StartNewGame();
        GameSession.GoToBase();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
