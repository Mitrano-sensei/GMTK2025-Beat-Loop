using UnityEngine;
using UnityEngine.Tilemaps;

public class LockScript : MonoBehaviour
{
    [SerializeField] private Tilemap groundTileMap;
    
    public Vector3Int GridPosition { get; private set; }
    
    private void Start()
    {
        FixPositionToGrid();
    }
    
    public void Unlock()
    {
        // TODO : Animate
        gameObject.SetActive(false);
    }

    public void Lock()
    {
        // TODO : Animate
        gameObject.SetActive(true);
    }

    private void FixPositionToGrid()
    {
        var position = groundTileMap.WorldToCell(transform.position);
        transform.position = CellToWorld(groundTileMap, position);
        GridPosition = position;
    }
    
    private Vector3 CellToWorld(Tilemap tilemap, Vector3Int position)
    {
        return tilemap.CellToWorld(position) + Vector3.right * (tilemap.cellSize.x / 2) +
               Vector3.up * (tilemap.cellSize.y / 2);
    }
}
