using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
    public static DontDestroyObject Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
