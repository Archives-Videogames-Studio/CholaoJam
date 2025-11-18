using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Nombre de la escena de juego")]
    public string gameplaySceneName = "CristoRey";

    public void PlayGame()
    {
        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogError("MainMenuController: gameplaySceneName no está configurado.");
            return;
        }

        Debug.Log($"Cargando escena de juego: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void OpenSettings()
    {
        Debug.Log("Settings aún no implementado.");
    }

    public void OpenCredits()
    {
        Debug.Log("Créditos aún no implementado.");
    }
}
