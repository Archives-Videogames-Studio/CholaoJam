using Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public string SceneName;

    public void SceneLoadByName()
    {
        SceneManager.LoadScene(SceneName);
    }
}