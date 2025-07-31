using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Utilities;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour
{
    [Header("Movements")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap obstacleTileMap;
    
    [Header("Animations")]
    [SerializeField] private PlayerAnimationController playerAnimationController;

    private void Start()
    {
        moveAction.action.performed += OnMove;
        FixPlayerPositionToGrid(groundTileMap);
    }

    private void FixPlayerPositionToGrid(Tilemap tilemap)
    {
        var position = tilemap.WorldToCell(transform.position);
        transform.position = CellToWorld(groundTileMap, position);
    }

    private void OnEnable()
    {
        playerAnimationController.StartIdleAnimation();
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        // playerAnimationController.StopIdleAnimation();
        moveAction.action.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector3Int movement = GetDirectionFromInput(context.ReadValue<Vector2>());
        var position = groundTileMap.WorldToCell(transform.position);
        var newPosition = position + movement;

        if (CanMove(newPosition))
        {
            // Animate
            transform.position = CellToWorld(groundTileMap, newPosition);
        }
        else
        {
            // Animate
            transform.position = CellToWorld(groundTileMap, position);
        }
    }

    private bool CanMove(Vector3Int position)
    {
        // Check if a grid has a tile at this position
        var groundHasTile = groundTileMap.HasTile(position);
        groundHasTile = true; // FIXME : Remove when we use groundtiles
        var noObstacleTile = !obstacleTileMap.HasTile(position);
        return groundHasTile && noObstacleTile;
    }

    private Vector3Int GetDirectionFromInput(Vector2 input)
    {
        float delta = .1f;
        if (input.x > delta)
            return Vector3Int.right;

        if (input.x < -delta)
            return Vector3Int.left;

        if (input.y > delta)
            return Vector3Int.up;

        if (input.y < -delta)
            return Vector3Int.down;

        return Vector3Int.zero;
    }

    private Vector3 CellToWorld(Tilemap tilemap, Vector3Int position)
    {
        return tilemap.CellToWorld(position) + Vector3.right * (tilemap.cellSize.x / 2) +
               Vector3.up * (tilemap.cellSize.y / 2);
    }
}