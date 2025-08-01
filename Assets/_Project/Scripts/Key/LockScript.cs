using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LockScript : MonoBehaviour
{
    [SerializeField] private Tilemap groundTileMap;

    public Vector3Int GridPosition { get; private set; }
    private float _baseScale;


    private void Start()
    {
        FixPositionToGrid();
        _baseScale = transform.localScale.x;
    }

    public async Task Unlock()
    {
        await Tween.Scale(transform, startValue: _baseScale, endValue: 0f, duration: .5f, Ease.InBounce);
    }

    public async Task Lock()
    {
        gameObject.SetActive(true);
        await Tween.Scale(transform, startValue: 0f, endValue: _baseScale, duration: .5f, Ease.OutBounce);
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