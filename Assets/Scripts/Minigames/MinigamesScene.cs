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
    public ChangeMachineButton machines;   // MachinesManager con ChangeMachineButton
    public string[] scenes;                // Nombres de las escenas de minijuegos
    public MachineKind[] machineKinds;     // Tipo de cada máquina en el mismo orden

    [Header("Avisos UI")]
    public GameObject warningPanel;        // Panel (Image) del feedback
    public TextMeshProUGUI warningText;    // Texto dentro del panel
    public float warningDuration = 1.8f;

    Coroutine _warningRoutine;

    void Start()
    {
        // Asegurarte que arranque oculto
        if (warningPanel != null)
            warningPanel.SetActive(false);
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    // Este es el método que llama tu ÚNICO botón de "jugar minijuego"
    public void SceneLoad()
    {
        if (machines == null || scenes == null || scenes.Length == 0)
            return;

        int idx = machines.index;
        if (idx < 0 || idx >= scenes.Length)
            return;

        string sceneName = scenes[idx];

        // Tipo de máquina actual (por defecto Hielo)
        MachineKind kind = MachineKind.Hielo;
        if (machineKinds != null && idx < machineKinds.Length)
            kind = machineKinds[idx];

        var state = CholadoGameState.Instance;

        // ============================
        //   1) PRIMERO SIEMPRE HIELO
        // ============================
        if (state != null && !state.hasFrio && kind != MachineKind.Hielo)
        {
            ShowWarning("Primero necesitas echarle HIELO al cholao.");
            return;
        }

        // ====================================
        //   2) NO REPETIR MISMO INGREDIENTE
        // ====================================
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

        // Si pasa todas las validaciones, SÍ carga el minijuego
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

    // ======================
    //   AVISOS EN PANTALLA
    // ======================
    void ShowWarning(string msg)
    {
        Debug.Log("[MINIGAME] " + msg);

        if (warningText == null && warningPanel == null)
            return;

        if (_warningRoutine != null)
            StopCoroutine(_warningRoutine);

        _warningRoutine = StartCoroutine(WarningRoutine(msg));
    }

    IEnumerator WarningRoutine(string msg)
    {
        if (warningPanel != null)
            warningPanel.SetActive(true);

        if (warningText != null)
        {
            warningText.text = msg;
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(warningDuration);

        if (warningText != null)
            warningText.gameObject.SetActive(false);
        if (warningPanel != null)
            warningPanel.SetActive(false);
    }
}
