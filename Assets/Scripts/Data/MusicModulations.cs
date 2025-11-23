using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class MusicModulations : MonoBehaviour
{
    public StudioEventEmitter emitter;
    private EventInstance instance;

    public float targetValue;
    public float speed = 2f;

    private float currentValue;
    public float currentVolume = 1;

    private Bus masterBus;



    void Start()
    {
        masterBus = RuntimeManager.GetBus("bus:/");

        currentVolume = 1;

        if (emitter == null)
            emitter = FindFirstObjectByType<StudioEventEmitter>();
        
        // Ahora sí podemos capturar la instancia correcta
        instance = emitter.EventInstance;

        // Obtener valor inicial del parámetro
        instance.getParameterByName("Instrumentos", out currentValue);
    }

    void Update()
    {
        currentValue = Mathf.MoveTowards(currentValue, targetValue, speed * Time.deltaTime);
        instance.setParameterByName("Instrumentos", currentValue);
  
        masterBus.setVolume(currentVolume);

    }

    public void SetInst(float value)
    {
        targetValue = value;
    }

    public void SetVolume(float volume)
    {
        currentVolume = volume;
    }

}
