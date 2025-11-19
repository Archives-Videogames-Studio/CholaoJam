using UnityEngine;

public class BoteMovement : MonoBehaviour
{
    public GameObject[] waypoints;
    public GameObject Bote;

    void Start()
    {
        Bote.transform.position = waypoints[0].transform.position;
    }


}
