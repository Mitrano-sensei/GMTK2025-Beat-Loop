using UnityEngine;
using Utilities;

public class DeathScreen : Singleton<DeathScreen>
{
    public GameObject deathScreen;
    
    public void Show() => deathScreen.SetActive(true);
    public void Hide() => deathScreen.SetActive(false);
    
}
