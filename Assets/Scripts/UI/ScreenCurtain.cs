using System;
using System.Collections;
using UnityEngine;

public class ScreenCurtain : MonoBehaviour
{
    public static ScreenCurtain Instance { get; private set; }

    [Header("RectTransform de la cortina (Image verde)")]
    public RectTransform curtain;

    [Header("Velocidad de despliegue")]
    public float animationSpeed = 4f;

    bool _isRunning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (curtain != null)
        {
            Vector3 s = curtain.localScale;
            s.y = 0f;
            curtain.localScale = s;
            curtain.gameObject.SetActive(false);
        }
    }

    public void RunTransition(Action midAction)
    {
        if (!_isRunning && curtain != null)
        {
            StartCoroutine(DoTransition(midAction));
        }
        else
        {
            midAction?.Invoke();
        }
    }

    IEnumerator DoTransition(Action midAction)
    {
        _isRunning = true;

        yield return StartCoroutine(CloseCurtain());

        yield return new WaitForSeconds(0.15f);

        midAction?.Invoke();

        yield return null;

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(OpenCurtain());

        _isRunning = false;
    }

    IEnumerator CloseCurtain()
    {
        curtain.gameObject.SetActive(true);

        Vector3 scale = curtain.localScale;
        scale.y = 0f;
        curtain.localScale = scale;

        while (curtain.localScale.y < 1f)
        {
            scale = curtain.localScale;
            scale.y += animationSpeed * Time.deltaTime;
            if (scale.y > 1f) scale.y = 1f;
            curtain.localScale = scale;
            yield return null;
        }
    }

    IEnumerator OpenCurtain()
    {
        Vector3 scale = curtain.localScale;
        scale.y = 1f;
        curtain.localScale = scale;

        while (curtain.localScale.y > 0f)
        {
            scale = curtain.localScale;
            scale.y -= animationSpeed * Time.deltaTime;
            if (scale.y < 0f) scale.y = 0f;
            curtain.localScale = scale;
            yield return null;
        }

        if (curtain.localScale.y <= 0f)
        {
            curtain.gameObject.SetActive(false);
        }
    }
}
