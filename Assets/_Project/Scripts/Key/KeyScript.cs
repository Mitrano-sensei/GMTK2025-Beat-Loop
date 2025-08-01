using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private LockScript lockScript;

    public Vector3Int Position { get; private set; }

    private float _baseScale;
    private SimpleAudioManager _audioManager;

    private void Awake()
    {
        _baseScale = transform.localScale.x;
    }

    private void Start()
    {
        FixPositionToGrid();
    }

    public async Task OnPickup()
    {
        if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
        _audioManager.PlayPickupKeySound();
        
        var tasks = new Task[2];
        tasks[0] = ScaleOut();
        tasks[1] = lockScript.Unlock();

        await Task.WhenAll(tasks);

        gameObject.SetActive(false);
        lockScript.gameObject.SetActive(false);
    }

    private async Task ScaleOut()
    {
        await Tween.Scale(transform, startValue: _baseScale, endValue: 0f, duration: .5f, Ease.InBounce);
    }

    public async Task Reset()
    {
        if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
        _audioManager.PlayResetSound();

        gameObject.SetActive(true);
        var tasks = new Task[2];
        tasks[0] = ScaleIn();
        tasks[1] = lockScript.Lock();

        await Task.WhenAll(tasks);
    }

    private async Task ScaleIn()
    {
        await Tween.Scale(transform, startValue: 0f, endValue: _baseScale, duration: .5f, Ease.OutBounce);
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