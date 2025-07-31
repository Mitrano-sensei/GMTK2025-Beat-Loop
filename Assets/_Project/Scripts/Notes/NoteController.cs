using System;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer1;
    [SerializeField] private SpriteRenderer spriteRenderer2;
    [SerializeField] private SpriteRenderer spriteRenderer3;

    [SerializeField] private bool initialNote1Active;
    [SerializeField] private bool initialNote2Active;
    [SerializeField] private bool initialNote3Active;
    
    private bool _isNote1Active;
    private bool _isNote2Active;
    private bool _isNote3Active;

    private void Start()
    {
        Reset();
    }
    
    public void TakeNote(int noteNumber)
    {
        if (noteNumber is < 1 or > 3)
        {
            Debug.LogError("Pliz noteNumber entre 1 et 3");
            return;
        }
        
        switch (noteNumber)
        {
            case 1:
                if (!initialNote1Active) Debug.LogError("Pas prenable");
                _isNote1Active = false;
                // TODO : Animation/Sound
                spriteRenderer1.enabled = false;
                break;
            case 2:
                if (!initialNote2Active) Debug.LogError("Pas prenable");
                _isNote2Active = false;
                // TODO : Animation/Sound
                spriteRenderer2.enabled = false;
                break;
            case 3:
                if (!initialNote3Active) Debug.LogError("Pas prenable");
                _isNote3Active = false;
                // TODO : Animation/Sound
                spriteRenderer3.enabled = false;
                break;
        }
    }
    
    public void Reset()
    {
        _isNote1Active = initialNote1Active;
        _isNote2Active = initialNote2Active;
        _isNote3Active = initialNote3Active;
        
        spriteRenderer1.enabled = _isNote1Active;
        spriteRenderer2.enabled = _isNote2Active;
        spriteRenderer3.enabled = _isNote3Active;
    }
}
