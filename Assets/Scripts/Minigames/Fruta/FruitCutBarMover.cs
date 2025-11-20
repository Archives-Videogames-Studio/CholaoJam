using UnityEngine;

public class FruitCutBarMover : MonoBehaviour
{
    [Header("Límites de movimiento")]
    public Transform leftLimit;
    public Transform rightLimit;

    [Header("Velocidad de la barra")]
    public float speed = 1.5f;

    float t;

    void Update()
    {
        if (!leftLimit || !rightLimit) return;

        t += Time.deltaTime * speed;

        float pingPong = Mathf.PingPong(t, 1f);

        transform.position = Vector3.Lerp(leftLimit.position, rightLimit.position, pingPong);
    }
}
