using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

public class SimpleAudioManager : Singleton<SimpleAudioManager>
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] [Range(0, 1f)] private float volumeScale;

    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip clapSound;
    [SerializeField] private AudioClip undoSound;
    [SerializeField] private AudioClip resetSound;
    [SerializeField] private AudioClip notPossibleSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip looseSound;
    [SerializeField] private AudioClip pickupKeySound;
    [SerializeField] private AudioClip buttonClickSound;

    [SerializeField] private AudioClip note1;
    [SerializeField] private AudioClip note2;
    [SerializeField] private AudioClip note3;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip audioClip) => audioSource.PlayOneShot(audioClip, volumeScale);

    public void Play(AudioClip audioClip, float audioRatio) =>
        audioSource.PlayOneShot(audioClip, volumeScale * audioRatio);


    public void PlayFailSound() => Play(failSound);
    public void PlayClapSound() => Play(clapSound);

    public void PlayNote(int i)
    {
        switch (i)
        {
            case 1:
                Play(note1);
                break;
            case 2:
                Play(note2);
                break;
            case 3:
                Play(note3);
                break;
        }
    }

    public void PlayPickupKeySound() => Play(pickupKeySound);
    public void PlayUndoSound() => Play(undoSound);

    public void PlayResetSound() => Play(resetSound, 0.7f);
    public void PlayNotPossibleSound() => Play(notPossibleSound);
    public void PlayWinSound() => Play(winSound);
    public void PlayLooseSound() => Play(looseSound);
    public void PlayButtonClick() => Play(buttonClickSound);
    
    public void SetAudioVolume(float v) => volumeScale = v;
}