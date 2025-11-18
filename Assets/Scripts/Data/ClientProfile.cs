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

    [Header("Diálogos OIGA")]
    [TextArea] public string[] oigaLines;

    [Header("Cholado ideal (0=Bajo,1=Medio,2=Alto)")]
    [Range(0, 2)] public int idealFrio;
    [Range(0, 2)] public int idealDulzor;
    [Range(0, 2)] public int idealAcidez;
    [Range(0, 2)] public int idealCremosidad;
    [Range(0, 2)] public int idealFruta;

    [Header("Parámetros de movimiento")]
    public float moveSpeed = 2f;
}
