using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;
    public TextMeshProUGUI dialogueText;

    [Header("Tiempos diálogo OIGA")]
    public float firstDelay   = 0.4f;
    public float lineDuration = 3.5f;
    public float endDelay     = 1.0f;

    [Header("Tiempo reacción (Vea)")]
    public float reactionDuration = 3.0f;

    [Header("Colores")]
    [Tooltip("Color base de TODO el texto del diálogo")]
    public Color baseTextColor = Color.black;

    [Tooltip("Color para resaltar las palabras clave de OIGA")]
    public Color highlightColor = new Color(1f, 0.83f, 0.33f); // amarillito

    ClientProfile _currentProfile;
    Coroutine _routine;

    public bool IsRunning { get; private set; }

    public event Action OnDialogueFinished;

    void Start()
    {
        Hide();

        if (dialogueText != null)
        {
            // Nos aseguramos de que TMP acepte <color>, <b>, etc.
            dialogueText.richText = true;
            dialogueText.color = baseTextColor;
        }
    }

    // ================== OIGA ==================

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
        {
            dialogueText.text = "";
            dialogueText.color = baseTextColor;   // color base configurable
            dialogueText.richText = true;         // por si algún otro script lo cambió
        }

        yield return new WaitForSeconds(firstDelay);

        for (int i = 0; i < _currentProfile.oigaLines.Length; i++)
        {
            if (dialogueText != null)
            {
                string rawLine = _currentProfile.oigaLines[i];
                string highlighted = ApplyKeywordHighlight(rawLine);
                dialogueText.text = highlighted;

                // Debug opcional para ver el texto con tags
                // Debug.Log($"[DIALOGUE] line={highlighted}");
            }

            yield return new WaitForSeconds(lineDuration);
        }

        yield return new WaitForSeconds(endDelay);

        Hide();

        OnDialogueFinished?.Invoke();
    }

    string ApplyKeywordHighlight(string line)
    {
        if (_currentProfile == null ||
            _currentProfile.oigaKeywords == null ||
            _currentProfile.oigaKeywords.Length == 0 ||
            string.IsNullOrEmpty(line))
        {
            return line;
        }

        // Convertimos el Color → "#RRGGBB"
        string hex = ColorUtility.ToHtmlStringRGB(highlightColor);
        string colorTag = $"#{hex}";

        string result = line;

        foreach (var kw in _currentProfile.oigaKeywords)
        {
            if (string.IsNullOrWhiteSpace(kw))
                continue;

            // Búsqueda case-insensitive; importante que coincida acentos, etc.
            string pattern = $"(?i){Regex.Escape(kw)}";

            result = Regex.Replace(result, pattern, match =>
                $"<color={colorTag}><b>{match.Value}</b></color>");
        }

        return result;
    }

    public void ReplayLast()
    {
        if (_currentProfile != null)
            PlayDialogue(_currentProfile);
    }

    // ================== VEA ==================

    public void PlayReaction(ClientProfile profile, string line)
    {
        _currentProfile = profile;

        if (_routine != null)
            StopCoroutine(_routine);

        IsRunning = true;
        _routine = StartCoroutine(ReactionRoutine(line));
    }

    IEnumerator ReactionRoutine(string line)
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (dialogueText != null)
        {
            dialogueText.color = baseTextColor;
            dialogueText.richText = true;
            dialogueText.text = line;
        }

        yield return new WaitForSeconds(reactionDuration);

        Hide();
        OnDialogueFinished?.Invoke();
    }

    // ================== Utilidades ==================

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
