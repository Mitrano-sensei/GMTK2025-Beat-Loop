using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Movements")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap obstacleTileMap;
    
    [Header("Animations")]
    [SerializeField] private PlayerAnimationController playerAnimationController;
    
    public Vector3Int CurrentCellPosition => groundTileMap.WorldToCell(transform.position);

    private void Start()
    {
        if (groundTileMap == null) Debug.LogError("Ground tilemap is null");
        if (obstacleTileMap == null) Debug.LogError("Obstacle tilemap is null");
    }
    
    private void OnEnable()
    {
        playerAnimationController.StartIdleAnimation();
    }

    public void FixPlayerPositionToGrid()
    {
        var position = groundTileMap.WorldToCell(transform.position);
        transform.position = CellToWorld(groundTileMap, position);
    }

    public async Task Move(Vector3Int movement)
    {
        var position = groundTileMap.WorldToCell(transform.position);
        var newPosition = position + movement;

        await playerAnimationController.MoveTo(CellToWorld(groundTileMap, newPosition));
    }
    
    public async Task MoveTo(Vector3Int newPosition)
    {
        await playerAnimationController.MoveTo(CellToWorld(groundTileMap, newPosition));
    }
    
    public async Task FailMove(Vector3Int movement)
    {
        await playerAnimationController.MoveTo(CellToWorld(groundTileMap, CurrentCellPosition + movement));
        await playerAnimationController.MoveTo(CellToWorld(groundTileMap, CurrentCellPosition - movement));
    }

    // Check if a grid has a tile at this position
    public bool CanMove(Vector3Int movement)
    {
        var position = groundTileMap.WorldToCell(transform.position);
        var newPosition = position + movement;
        
        return !obstacleTileMap.HasTile(newPosition);
    }

    public bool IsMoving()
    {
        return playerAnimationController.IsMoving;
    }

    public void PickupItem()
    {
        //playerAnimationController.OnPickup();
    }
    
    private Vector3 CellToWorld(Tilemap tilemap, Vector3Int position)
    {
        return tilemap.CellToWorld(position) + Vector3.right * (tilemap.cellSize.x / 2) +
               Vector3.up * (tilemap.cellSize.y / 2);
    }
}