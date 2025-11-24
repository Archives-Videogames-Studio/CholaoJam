using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialController : MonoBehaviour
{
    [Header("Panel y posiciones")]
    public RectTransform tutorialPanel;
    public Transform shownPos;
    public Transform hiddenPos;
    public float slideDuration = 0.35f;

    [Header("Páginas del tutorial")]
    public GameObject[] pages;

    [Header("Botones de navegación")]
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Lógica de juego")]
    public ClientSequenceManager clientSequence;

    [Header("Tecla para mostrar/ocultar")]
    public Key toggleKey = Key.Q;

    int  _currentPage = 0;
    bool _isVisible   = false;   // arranca oculto
    bool _isSliding   = false;
    bool _gameStarted = false;

    void Start()
    {
        // 1) Colocar el portapapeles ABAJO
        if (tutorialPanel != null && hiddenPos != null)
            tutorialPanel.position = hiddenPos.position;

        // 2) Mostrar primera página
        GoToPage(0);

        // 3) Lanzar ANIMACIÓN de subida
        ShowTutorial();   // esto internamente arranca SlidePanel hacia shownPos
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[toggleKey].wasPressedThisFrame)
        {
            ToggleTutorial();
        }
    }

    // ---------- Navegación de páginas ----------

    public void NextPage()
    {
        if (pages == null || pages.Length == 0) return;

        if (_currentPage < pages.Length - 1)
            GoToPage(_currentPage + 1);
    }

    public void PrevPage()
    {
        if (pages == null || pages.Length == 0) return;

        if (_currentPage > 0)
            GoToPage(_currentPage - 1);
    }

    void GoToPage(int index)
    {
        if (pages == null || pages.Length == 0) return;

        index = Mathf.Clamp(index, 0, pages.Length - 1);
        _currentPage = index;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == _currentPage);
        }

        UpdateNavButtons();
    }

    void UpdateNavButtons()
    {
        if (leftButton != null)
            leftButton.SetActive(_currentPage > 0);

        if (rightButton != null && pages != null && pages.Length > 0)
            rightButton.SetActive(_currentPage < pages.Length - 1);
    }

    // ---------- Mostrar / ocultar ----------

    public void ToggleTutorial()
    {
        if (_isSliding) return;

        if (_isVisible)
            HideTutorial();
        else
            ShowTutorial();
    }

    public void ShowTutorial()
    {
        if (tutorialPanel == null || shownPos == null) return;

        _isVisible = true;
        StartCoroutine(SlidePanel(shownPos.position));
    }

    public void HideTutorial()
    {
        if (tutorialPanel == null || hiddenPos == null) return;

        _isVisible = false;
        StartCoroutine(SlidePanel(hiddenPos.position));

        // La PRIMERA vez que lo ocultas, arrancas la secuencia de clientes
        if (!_gameStarted && clientSequence != null)
        {
            clientSequence.BeginSequence();
            _gameStarted = true;
        }
    }

    IEnumerator SlidePanel(Vector3 targetPos)
    {
        _isSliding = true;

        Vector3 startPos = tutorialPanel.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / slideDuration;
            tutorialPanel.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        tutorialPanel.position = targetPos;
        _isSliding = false;
    }
}
