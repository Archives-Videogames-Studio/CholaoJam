using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider slider;
    public MusicModulations mm;

    public void Awake()
    {
        slider.value = mm.currentVolume;
    }
}