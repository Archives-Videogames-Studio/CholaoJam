using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameExit : MonoBehaviour
{
    public void ExitMinigame()
    {
        if (CholadoGameState.Instance != null &&
            CholadoGameState.Instance.cristoReyRoot != null)
        {
            CholadoGameState.Instance.cristoReyRoot.SetActive(true);
        }
        Scene thisScene = gameObject.scene;
        Debug.Log($"[MinigameExit] Unloading scene: {thisScene.name}");
        SceneManager.UnloadSceneAsync(thisScene);
    }
}
