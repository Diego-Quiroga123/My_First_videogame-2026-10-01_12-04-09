using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Datos que sobreviven al cambiar de escena (piso actual, materiales, nivel del equipo)
/// y la lista de escenas del juego. No se pone en ningún objeto: es estático.
/// </summary>
public static class GameSession
{
    // ⚠ Si tus escenas se llaman distinto, cambia los nombres aquí
    //   (y agrégalas todas en File > Build Settings).
    public static string MainMenuScene = "MainMenu";
    public static string BaseScene = "Base";
    public static string DungeonScene = "Dungeon";

    /// <summary>Cada cuántos pisos los enemigos suben de nivel de dificultad.</summary>
    public const int FloorsPerTier = 2;

    public static int Floor = 1;

    /// <summary>0 en los pisos 1-2, 1 en los pisos 3-4, 2 en los 5-6...</summary>
    public static int DifficultyTier => (Floor - 1) / FloorsPerTier;

    // Progreso del jugador (lo guarda PlayerUpgrades)
    public static bool HasPlayerData;
    public static int Materials;
    public static int EquipmentLevel;

    // Si tienes desactivado "Reload Domain" en Enter Play Mode, los estáticos no se
    // reinician solos; esto asegura una partida limpia cada vez que das Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        StartNewGame();
    }

    public static void StartNewGame()
    {
        Floor = 1;
        HasPlayerData = false;
        Materials = 0;
        EquipmentLevel = 0;
    }

    public static void GoToMainMenu()
    {
        Floor = 1;
        LoadScene(MainMenuScene);
    }

    /// <summary>Vuelve a la base: se acaba la expedición (el piso vuelve a 1) pero conservas materiales y mejoras.</summary>
    public static void GoToBase()
    {
        Floor = 1;
        LoadScene(BaseScene);
    }

    /// <summary>Carga el calabozo en el piso actual (entrar desde la base o reintentar).</summary>
    public static void LoadDungeon()
    {
        LoadScene(DungeonScene);
    }

    /// <summary>Sube un piso y vuelve a generar el calabozo.</summary>
    public static void NextFloor()
    {
        Floor++;
        LoadScene(DungeonScene);
    }

    private static void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("GameSession: la escena '" + sceneName + "' no está en File > Build Settings, " +
                           "o se llama distinto. Revisa los nombres al inicio de GameSession.cs.");
            return;
        }

        Time.timeScale = 1f; // por si veníamos de un menú que pausó el juego
        SceneManager.LoadScene(sceneName);
    }
}
