using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NoteController : MonoBehaviour
{
    [SerializeField] private Tilemap groundTileMap;
    
    [SerializeField] private SpriteRenderer spriteRenderer1;
    [SerializeField] private SpriteRenderer spriteRenderer2;
    [SerializeField] private SpriteRenderer spriteRenderer3;
    
    [SerializeField] private Transform note1;
    [SerializeField] private Transform note2;
    [SerializeField] private Transform note3;

    [SerializeField] private bool initialNote1Active;
    [SerializeField] private bool initialNote2Active;
    [SerializeField] private bool initialNote3Active;
    
    private bool _isNote1Active;
    private bool _isNote2Active;
    private bool _isNote3Active;

    private Vector3 _baseScale;
    
    public Vector3Int GridPosition => groundTileMap.WorldToCell(transform.position);
    public int RemainingNotes => (_isNote1Active ? 1 : 0) + (_isNote2Active ? 1 : 0) + (_isNote3Active ? 1 : 0);

    private void Start()
    {
        _baseScale = note1.localScale;
        Reset();
        FixPlayerPositionToGrid();
        PlayersManager.Instance.RegisterNoteController(this);
    }

    private void FixPlayerPositionToGrid()
    {
        var position = groundTileMap.WorldToCell(transform.position);
        transform.position = CellToWorld(groundTileMap, position);
    }
    
    public async Task<bool> TakeNote(int noteNumber)
    {
        if (noteNumber is < 1 or > 3)
        {
            Debug.LogError("Pliz noteNumber entre 1 et 3");
            return false;
        }
        
        switch (noteNumber)
        {
            case 1:
                if (!initialNote1Active)
                {
                    Debug.LogError("Pas prenable");
                    return false;
                }
                _isNote1Active = false;
                await DeathAnimation(note1, spriteRenderer1);
                break;
            case 2:
                if (!initialNote2Active)
                {
                    Debug.LogError("Pas prenable");
                    return false;
                }
                _isNote2Active = false;
                await DeathAnimation(note2, spriteRenderer2);
                break;
            case 3:
                if (!initialNote3Active)
                {
                    Debug.LogError("Pas prenable");
                    return false;
                }
                _isNote3Active = false;
                await DeathAnimation(note3, spriteRenderer3);
                break;
        }

        return true;
    }

    public async Task ReturnNote(int type)
    {
        switch (type)
        {
            case 1:
                if (!initialNote1Active) throw new Exception("Pas déprenable");
                _isNote1Active = true;
                await ComeBackAnimation(note1, spriteRenderer1);
                break;
            case 2:
                if (!initialNote2Active) throw new Exception("Pas déprenable");
                _isNote2Active = true;
                await ComeBackAnimation(note1, spriteRenderer2);
                break;
            case 3:
                if (!initialNote3Active) throw new Exception("Pas déprenable");
                _isNote3Active = true;
                await ComeBackAnimation(note1, spriteRenderer3);
                break;
            
        }
    }

    private async Task DeathAnimation(Transform note, SpriteRenderer spriteRenderer)
    {
        // TODO: Sound
        await Tween.Scale(note, startValue:_baseScale, endValue:Vector3.zero, duration: 0.5f, Ease.OutBounce);
        spriteRenderer.enabled = false;
        note.localScale = _baseScale;
    }
    
    private async Task ComeBackAnimation(Transform note, SpriteRenderer spriteRenderer)
    {
        // TODO : Sound
        note.localScale = Vector3.zero;
        spriteRenderer.enabled = true;
        await Tween.Scale(note, startValue:Vector3.zero, endValue:_baseScale, duration: 0.5f, Ease.OutBounce);
        note.localScale = _baseScale;
    }
    
    private Vector3 CellToWorld(Tilemap tilemap, Vector3Int position)
    {
        return tilemap.CellToWorld(position) + Vector3.right * (tilemap.cellSize.x / 2) +
               Vector3.up * (tilemap.cellSize.y / 2);
    }

    
    public void Reset()
    {
        _isNote1Active = initialNote1Active;
        _isNote2Active = initialNote2Active;
        _isNote3Active = initialNote3Active;

        Tween.StopAll(note1);
        Tween.StopAll(note2);
        Tween.StopAll(note3);
        
        spriteRenderer1.enabled = _isNote1Active;
        spriteRenderer2.enabled = _isNote2Active;
        spriteRenderer3.enabled = _isNote3Active;
    }
}
