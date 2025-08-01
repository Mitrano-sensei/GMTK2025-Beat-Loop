using UnityEngine;
using Utilities;

public class WinScreen : Singleton<WinScreen>
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private ParticleSystem confetti1;
    [SerializeField] private ParticleSystem confetti2;
    
    public void Show()
    {
        winScreen.SetActive(true);
        
        confetti1.Emit(500);
        confetti2.Emit(500);
    }

    public void Hide() => winScreen.SetActive(false);
}