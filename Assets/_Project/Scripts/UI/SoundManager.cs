using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{

    [SerializeField] private Slider _VolumeSlider;
    void Start()
    {
        _VolumeSlider.value = 1f;
    }

    public void ChangeVolume()
    {
        AudioListener.volume = _VolumeSlider.value;
    }
}