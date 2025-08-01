using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private LockScript lockScript;
    
    public Vector3Int Position { get; private set; }
    
    private void Start()
    {
        FixPositionToGrid();
    }

    public void OnPickup()
    {
        // TODO : Animate
        lockScript.Unlock();
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        // TODO : Animate
        gameObject.SetActive(true);
        lockScript.Lock();
    }

    private void FixPositionToGrid()
    {
        var position = groundTileMap.WorldToCell(transform.position);
        transform.position = CellToWorld(groundTileMap, position);
        Position = position;
    }
    
    private Vector3 CellToWorld(Tilemap tilemap, Vector3Int position)
    {
        return tilemap.CellToWorld(position) + Vector3.right * (tilemap.cellSize.x / 2) +
               Vector3.up * (tilemap.cellSize.y / 2);
    }
}
