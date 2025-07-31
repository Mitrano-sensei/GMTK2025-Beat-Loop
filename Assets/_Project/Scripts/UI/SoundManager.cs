using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{

    [SerializeField] private Slider _VolumeSlider;
    void Start()
    {
        if(!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 40);
        }

        else
        {
            Load();
        }
    }

    public void ChangeVolume()
    {
        AudioListener.volume = _VolumeSlider.value;
        Save();
    }
    private void Load()
    {
        _VolumeSlider.value = PlayerPrefs.GetInt("musicVolume");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", _VolumeSlider.value);
    }
}