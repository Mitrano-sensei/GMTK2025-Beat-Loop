using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider _VolumeSlider;

    [SerializeField] private TextMeshProUGUI _VolumeSliderValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _VolumeSliderValue.text = "Current Volume";
        _VolumeSlider.onValueChanged.AddListener((v) =>
        {
            _VolumeSliderValue.text = v.ToString("0") + " %";
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
