using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NoteController : MonoBehaviour
{
    [SerializeField] private Tilemap groundTileMap;
    
    [SerializeField] private SpriteRenderer spriteRenderer1;
    [SerializeField] private SpriteRenderer spriteRenderer2;
    [SerializeField] private SpriteRenderer spriteRenderer3;

    [SerializeField] private bool initialNote1Active;
    [SerializeField] private bool initialNote2Active;
    [SerializeField] private bool initialNote3Active;
    
    private bool _isNote1Active;
    private bool _isNote2Active;
    private bool _isNote3Active;
    
    public Vector3Int GridPosition => groundTileMap.WorldToCell(transform.position);
    public int RemainingNotes => (_isNote1Active ? 1 : 0) + (_isNote2Active ? 1 : 0) + (_isNote3Active ? 1 : 0);

    private void Start()
    {
        Reset();
        FixPlayerPositionToGrid();
        PlayersManager.Instance.RegisterNoteController(this);
    }
    
    public void FixPlayerPositionToGrid()
    {
        var position = groundTileMap.WorldToCell(transform.position);
        transform.position = CellToWorld(groundTileMap, position);
    }
    
    public bool TakeNote(int noteNumber)
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
                // TODO : Animation/Sound
                spriteRenderer1.enabled = false;
                break;
            case 2:
                if (!initialNote2Active)
                {
                    Debug.LogError("Pas prenable");
                    return false;
                }
                _isNote2Active = false;
                // TODO : Animation/Sound
                spriteRenderer2.enabled = false;
                break;
            case 3:
                if (!initialNote3Active)
                {
                    Debug.LogError("Pas prenable");
                    return false;
                }
                _isNote3Active = false;
                // TODO : Animation/Sound
                spriteRenderer3.enabled = false;
                break;
        }

        return true;
    }

    public void ReturnNote(int type)
    {
        // TODO : Animate + Sound
        switch (type)
        {
            case 1:
                if (!initialNote1Active) throw new Exception("Pas déprenable");
                _isNote1Active = true;
                spriteRenderer1.enabled = true;
                break;
            case 2:
                if (!initialNote2Active) throw new Exception("Pas déprenable");
                _isNote2Active = true;
                spriteRenderer2.enabled = true;
                break;
            case 3:
                if (!initialNote3Active) throw new Exception("Pas déprenable");
                _isNote3Active = true;
                spriteRenderer3.enabled = true;
                break;
            
        }
        
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
        
        spriteRenderer1.enabled = _isNote1Active;
        spriteRenderer2.enabled = _isNote2Active;
        spriteRenderer3.enabled = _isNote3Active;
    }
}
