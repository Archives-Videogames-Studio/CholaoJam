using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigamesScene : MonoBehaviour
{
    public ChangeMachineButton machines;
    public string[] scenes;

    public void SceneLoad()
    {
        SceneManager.LoadScene(scenes[machines.index]);
    }


}
