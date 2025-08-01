using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider _VolumeSlider;

    [SerializeField] private TextMeshProUGUI _VolumeSliderValue;

    void Start()
    {
        _VolumeSliderValue.text = "Current Volume";
        _VolumeSlider.onValueChanged.AddListener((v) =>
        {
            _VolumeSliderValue.text = (int)(v * 100) + " %";
        });
    }
}
