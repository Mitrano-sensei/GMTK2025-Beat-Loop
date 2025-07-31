using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersManager : MonoBehaviour
{
    [SerializeField] private PlayerController[] playerControllers;
    [SerializeField] private int maxTurns = 3;
    
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference undoAction;

    private PlayerController CurrentPlayer => _playerMovements[_currentPlayer].Player;
    private PlayerMovements CurrentPlayerMovements => _playerMovements[_currentPlayer];
    private PlayerMovements[] PlayerBeforeCurrent => _playerMovements.Take(_currentPlayer).ToArray();
    
    private PlayerMovements[] _playerMovements;
    private int _currentPlayer = 0;
    private int _currentTurn = 0;

    private bool _isMoving = false;

    private void Start()
    {
        _playerMovements = new PlayerMovements[playerControllers.Length];

        for (int i = 0; i < playerControllers.Length; i++)
        {
            var playerController = playerControllers[i];
            playerController.FixPlayerPositionToGrid();
            _playerMovements[i] = new PlayerMovements(playerController);
            if (i!=0)
                playerController.gameObject.SetActive(false);
        }

        moveAction.action.performed +=  async (ctx) =>
        {
            if (_isMoving) return;

            _isMoving = true;
            await OnMove(GetDirectionFromInput(ctx.ReadValue<Vector2>()));
            _isMoving = false;
        };

        undoAction.action.performed += async ctx =>
        {
            if (_isMoving) return;

            _isMoving = true;
            await Undo();
            _isMoving = false;
        };
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }
    
    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private async Task OnMove(Vector3Int movement)
    {
        Debug.Log("Movement Call");
        if (CurrentPlayer.IsMoving() || movement == Vector3Int.zero)
            return;
        
        if (!CurrentPlayer.CanMove(movement))
        {
            await CurrentPlayer.FailMove(movement);
            // TODO : Fail sound
            return;
        }
        
        // Check if there is another player in our destination
        foreach (PlayerMovements playerMovement in _playerMovements)
        {
            if (playerMovement == CurrentPlayerMovements)
                continue;
            
            if (playerMovement.Player.CurrentCellPosition == (CurrentPlayer.CurrentCellPosition + movement))
            {
                await CurrentPlayer.FailMove(movement);
                // TODO : Fail sound
                return;
            }
        }
        

        // Move all players
        List<Task> tasks = new ();
        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            tasks.Add(playerMovement.Player.Move(playerMovement.Movements[_currentTurn]));
        }
        tasks.Add(CurrentPlayer.Move(movement));
        // Wait for all tasks to finish
        await Task.WhenAll(tasks);
        
        // Check if there is another player in our destination
        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            if (playerMovement.Player.CurrentCellPosition == CurrentPlayer.CurrentCellPosition)
            {
                CancelMovements(movement);
                // TODO : Fail sound
                return;
            }
        }
        
        CurrentPlayerMovements.Movements.Add(movement);
        _currentTurn++;
        
        if (_currentTurn == maxTurns)
        {
            _currentTurn = 0;
            _currentPlayer += 1;
            
            if (_currentPlayer > playerControllers.Length - 1)
            {
                // TODO : End game
                Debug.LogError("Game finished");
                return;
            }
            
            _playerMovements[_currentPlayer].Player.gameObject.SetActive(true);

            foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
            {
                playerMovement.Player.MoveTo(playerMovement.InitialPosition);
            }
        }
    }

    private async void CancelMovements(Vector3Int movement)
    {
        Task[] tasks2 = new Task[_currentPlayer + 1];
                
        for (int j = 0; j < _currentPlayer; j++)
        {
            tasks2[j] = _playerMovements[j].Player.Move(-_playerMovements[j].Movements[_currentTurn]);
        }
        tasks2[_currentPlayer] = CurrentPlayer.Move(-movement);
        await Task.WhenAll(tasks2);
        return;
    }

    private async Task Undo()
    {
        if (_currentTurn == 0)
        {
            Debug.LogWarning("Can't undo");
            // TODO : Fail sound
            return;
        }
        
        Debug.Log("Undoing");

        // For current player
        List<Task> undoTasks = new ();
        
        undoTasks.Add(UndoPlayer(CurrentPlayerMovements, true));

        // For players before
        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            undoTasks.Add(UndoPlayer(playerMovement));
        }
        
        await Task.WhenAll(undoTasks);
        
        _currentTurn--;
    }
    
    private async Task UndoPlayer(PlayerMovements playerMovement, bool removeFromMovement = false)
    {
        await playerMovement.Player.Move(-playerMovement.Movements[_currentTurn - 1]);
        if (removeFromMovement) playerMovement.Movements.RemoveAt(_currentTurn - 1);
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
    
    private class PlayerMovements
    {
        public PlayerController Player { get; private set; }
        public List<Vector3Int> Movements { get; private set; }
        public Vector3Int InitialPosition { get; private set; }

        public PlayerMovements(PlayerController player)
        {
            Player = player;
            Movements = new();
            InitialPosition = player.CurrentCellPosition;
        }
    }
}
