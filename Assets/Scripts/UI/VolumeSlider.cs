using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider slider;
    public MusicModulations mm;

    private void OnEnable()
    {
        slider.value = mm.currentVolume;
    }

    public void SetMasterVolume()
    {
        mm.SetVolume(slider.value);
    }

}