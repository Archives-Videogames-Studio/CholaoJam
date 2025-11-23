using UnityEngine;

public class CholadoVisual : MonoBehaviour
{
    [Header("Capas principales")]
    public SpriteRenderer vasoRenderer;
    public SpriteRenderer hieloRenderer;
    public SpriteRenderer frutaRenderer;

    [Header("Capas de sirope por tipo de hielo")]
    public SpriteRenderer[] siropeLayers = new SpriteRenderer[3];

    [Header("Sprites")]
    public Sprite vasoSprite;

    // HIELO:
    public Sprite[] hieloSprites = new Sprite[3];

    // FRUTA:
    public Sprite[] frutaSprites = new Sprite[3];

    // SIROPE:
    [System.Serializable]
    public class SiropeSet
    {
        public Sprite[] byDulzor = new Sprite[3]; 
    }

    public SiropeSet[] siropeByHielo = new SiropeSet[3];

    void Start()
    {
        RefreshFromState();
    }
    public void ClearAll()
    {
        // Vaso
        if (vasoRenderer != null)
            vasoRenderer.sprite = vasoSprite;

        // HIELO
        if (hieloRenderer != null)
        {
            hieloRenderer.sprite = null;
            hieloRenderer.enabled = false;
        }

        // SIROPE
        if (siropeLayers != null)
        {
            for (int i = 0; i < siropeLayers.Length; i++)
            {
                if (siropeLayers[i] != null)
                {
                    siropeLayers[i].sprite = null;
                    siropeLayers[i].gameObject.SetActive(false);
                }
            }
        }

        // FRUTA
        if (frutaRenderer != null)
        {
            frutaRenderer.sprite = null;
            frutaRenderer.enabled = false;
        }
    }

    public void RefreshFromState()
    {
        var state = CholadoGameState.Instance;
        if (state == null) return;

        // --- VASO ---
        if (vasoRenderer != null && vasoSprite != null)
            vasoRenderer.sprite = vasoSprite;

        // --- HIELO ---
        if (hieloRenderer != null && state.hasFrio)
        {
            int f = Mathf.Clamp(state.resultFrio, 0, 2);
            if (f >= 0 && f < hieloSprites.Length && hieloSprites[f] != null)
                hieloRenderer.sprite = hieloSprites[f];
            hieloRenderer.enabled = true;
        }
        else if (hieloRenderer != null)
        {
            hieloRenderer.enabled = false;
        }

        // --- SIROPE ---
        for (int i = 0; i < siropeLayers.Length; i++)
            if (siropeLayers[i] != null)
                siropeLayers[i].gameObject.SetActive(false);

        if (state.hasDulzor)
        {
            int hieloLevel  = Mathf.Clamp(state.resultFrio,   0, 2); // qué hielo quedó
            int dulzorLevel = Mathf.Clamp(state.resultDulzor, 0, 2); // qué dulzor quedó

            SpriteRenderer targetLayer = null;
            if (hieloLevel >= 0 && hieloLevel < siropeLayers.Length)
                targetLayer = siropeLayers[hieloLevel];

            if (targetLayer != null &&
                hieloLevel >= 0 && hieloLevel < siropeByHielo.Length &&
                siropeByHielo[hieloLevel] != null &&
                dulzorLevel >= 0 && dulzorLevel < siropeByHielo[hieloLevel].byDulzor.Length)
            {
                Sprite s = siropeByHielo[hieloLevel].byDulzor[dulzorLevel];
                if (s != null)
                {
                    targetLayer.sprite = s;
                    targetLayer.gameObject.SetActive(true);
                }
            }
        }

        // --- FRUTA ---
        if (frutaRenderer != null && state.hasFruta)
        {
            int fr = Mathf.Clamp(state.resultFruta, 0, 2);
            if (fr >= 0 && fr < frutaSprites.Length && frutaSprites[fr] != null)
                frutaRenderer.sprite = frutaSprites[fr];
            frutaRenderer.enabled = true;
        }
        else if (frutaRenderer != null)
        {
            frutaRenderer.enabled = false;
        }
    }
}
