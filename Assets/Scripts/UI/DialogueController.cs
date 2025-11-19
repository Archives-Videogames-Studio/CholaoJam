using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;          
    public TextMeshProUGUI dialogueText;  

    [Header("Tiempos")]
    public float firstDelay = 0.1f;
    public float lineDuration = 2.0f;
    public float endDelay = 0.5f;

    ClientProfile _currentProfile;
    Coroutine _routine;

    public bool IsRunning { get; private set; }

    public event Action OnDialogueFinished;   

    void Start()
    {
        Hide();
    }

    public void PlayDialogue(ClientProfile profile)
    {
        _currentProfile = profile;

        if (_routine != null)
            StopCoroutine(_routine);

        IsRunning = true;
        _routine = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        if (_currentProfile == null ||
            _currentProfile.oigaLines == null ||
            _currentProfile.oigaLines.Length == 0)
        {
            Hide();
            yield break;
        }

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "";

        yield return new WaitForSeconds(firstDelay);

        for (int i = 0; i < _currentProfile.oigaLines.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text = _currentProfile.oigaLines[i];

            yield return new WaitForSeconds(lineDuration);
        }

        yield return new WaitForSeconds(endDelay);

        Hide();

        OnDialogueFinished?.Invoke();
    }

    public void ReplayLast()
    {
        if (_currentProfile != null)
        {
            PlayDialogue(_currentProfile);
        }
    }

    public void Hide()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        IsRunning = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";
    }
}
