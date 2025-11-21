using System;                    
using System.Collections;        
using UnityEngine;               
using UnityEngine.SceneManagement;
using TMPro;                     

public class MinigamesScene : MonoBehaviour
{
    public enum MachineKind
    {
        Hielo,
        Sirope,
        Fruta
    }

    [Header("Selección de máquina")]
    public ChangeMachineButton machines;
    public string[] scenes;               
    public MachineKind[] machineKinds;    

    [Header("Avisos")]
    public TextMeshProUGUI warningText;
    public float warningDuration = 1.8f;

    Coroutine _warningRoutine;

    public void SceneLoad()
    {
        if (machines == null || scenes == null || scenes.Length == 0)
            return;

        int idx = machines.index;
        if (idx < 0 || idx >= scenes.Length)
            return;

        string sceneName = scenes[idx];

        MachineKind kind = MachineKind.Hielo;
        if (machineKinds != null && idx < machineKinds.Length)
            kind = machineKinds[idx];

        var state = CholadoGameState.Instance;

        if (state != null)
        {
            if (kind == MachineKind.Hielo && state.hasFrio)
            {
                ShowWarning("Ya le echaste HIELO al cholao de este cliente.");
                return;
            }
            if (kind == MachineKind.Sirope && state.hasDulzor)
            {
                ShowWarning("Ya le echaste SIROPE al cholao de este cliente.");
                return;
            }
            if (kind == MachineKind.Fruta && state.hasFruta)
            {
                ShowWarning("Ya le echaste FRUTA al cholao de este cliente.");
                return;
            }
        }

        Action midAction = () =>
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            if (state != null && state.cristoReyRoot != null)
            {
                state.cristoReyRoot.SetActive(false);
            }
        };

        if (ScreenCurtain.Instance != null)
            ScreenCurtain.Instance.RunTransition(midAction);
        else
            midAction();
    }

    void ShowWarning(string msg)
    {
        Debug.Log("[MINIGAME] " + msg);

        if (warningText == null)
            return;

        if (_warningRoutine != null)
            StopCoroutine(_warningRoutine);

        _warningRoutine = StartCoroutine(WarningRoutine(msg));
    }

    IEnumerator WarningRoutine(string msg)
    {
        warningText.text = msg;
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        warningText.gameObject.SetActive(false);
    }
}
