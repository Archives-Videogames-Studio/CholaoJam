using UnityEngine;

[CreateAssetMenu(
    menuName = "Cholao/Client Profile",
    fileName = "ClientProfile")]
public class ClientProfile : ScriptableObject
{
    [Header("Info básica")]
    public string clientName;
    public Sprite portrait;
    [TextArea] public string shortDescription;

    [Header("Animator")]
    public AnimatorOverrideController animatorOverride;  // ← NUEVO

    [Header("Diálogos OIGA")]
    [TextArea] public string[] oigaLines;

    [Header("Pistas OIGA (palabras a resaltar)")]
    public string[] oigaKeywords;

    [Header("Cholado ideal (0=Bajo,1=Medio,2=Alto)")]
    public int idealFrio;
    public int idealDulzor;
    public int idealFruta;

    [Header("Reacción al cholado")]
    [TextArea] public string[] reactionPerfectLines; 
    [TextArea] public string[] reactionOkLines;
    [TextArea] public string[] reactionBadLines;

    [Header("Sprites de reacción")]
    public Sprite reactionChimba;
    public Sprite reactionMelo;
    public Sprite reactionPaila;

    [Header("Parámetros de movimiento")]
    public float moveSpeed = 2f;
}
