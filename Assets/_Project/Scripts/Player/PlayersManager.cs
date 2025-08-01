using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public class PlayersManager : Singleton<PlayersManager>
{
    [SerializeField] private PlayerController[] playerControllers;
    
    [SerializeField] private int maxTurns = 3;

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference undoAction;
    [SerializeField] private InputActionReference restartAction;

    [SerializeField] private List<KeyLockData> keyLockDatas;

    private PlayerController CurrentPlayer => _playersMovements[_currentPlayer].Player;
    private PlayerMovements CurrentPlayerMovements => _playersMovements[_currentPlayer];
    private PlayerMovements[] PlayerBeforeCurrent => _playersMovements.Take(_currentPlayer).ToArray();

    private List<LockScript> Locks => keyLockDatas.Select(kl => kl.lockScript).ToList();
    private List<KeyScript> Keys => keyLockDatas.Select(kl => kl.key).ToList();

    private PlayerMovements[] _playersMovements;

    private List<NoteController> _notes = new();
    private int _currentPlayer = 0;
    private int _currentTurn = 0;

    private bool _isMoving = false;

    private void Start()
    {
        SetupPlayerMovements();
        SetupInputs();
    }

    private void SetupInputs()
    {
        moveAction.action.performed += async (ctx) =>
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

        restartAction.action.performed += async ctx =>
        {
            if (_isMoving) return;
            _isMoving = true;
            await Reset();
            _isMoving = false;
        };
    }

    private void SetupPlayerMovements()
    {
        _playersMovements = new PlayerMovements[playerControllers.Length];

        for (int i = 0; i < playerControllers.Length; i++)
        {
            var playerController = playerControllers[i];
            playerController.FixPlayerPositionToGrid();
            _playersMovements[i] = new PlayerMovements(playerController);
            if (i != 0)
                playerController.gameObject.SetActive(false);
        }
    }

    private async Task Reset()
    {
        _currentPlayer = 0;
        _currentTurn = 0;

        // Move Everyone
        List<Task> tasks = new();
        foreach (var playerMovement in _playersMovements)
        {
            playerMovement.Movements.Clear();
            tasks.Add(playerMovement.Player.MoveTo(playerMovement.InitialPosition));
        }

        await Task.WhenAll(tasks);
        _playersMovements[0].Player.gameObject.SetActive(true);
        for (int i = 1; i < _playersMovements.Length; i++)
        {
            _playersMovements[i].Player.gameObject.SetActive(false);
        }

        // Reset Notes
        ResetNotes();
    }

    private void ResetNotes()
    {
        foreach (var noteController in _notes)
        {
            noteController.Reset();
        }

        foreach (var playerMovement in _playersMovements)
        {
            playerMovement.TookNotes.Clear();
        }
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
        if (CurrentPlayer.IsMoving() || movement == Vector3Int.zero)
            return;

        if (!CurrentPlayer.CanMove(movement))
        {
            await CurrentPlayer.FailMove(movement);
            // TODO : Fail sound
            return;
        }

        // Check if there is another player in our destination
        foreach (PlayerMovements playerMovement in _playersMovements)
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
        
        // Check if there is a lock in our destination
        foreach (LockScript l in Locks)
        {
            if (!l.gameObject.activeSelf)
                continue;
            
            if (l.GridPosition == (CurrentPlayer.CurrentCellPosition + movement))
            {
                await CurrentPlayer.FailMove(movement);
                // TODO : Fail sound
                return;
            }
        }


        // Move all players
        List<Task> tasks = new();
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

        // Here we can move the player
        CurrentPlayerMovements.Movements.Add(movement);
        _currentTurn++;

        // Check if we took a note
        foreach (NoteController noteController in _notes)
        {
            var playerNumber = _currentPlayer;
            var player = CurrentPlayer;
            var playerMovements = CurrentPlayerMovements;

            await CheckIfTookNote(noteController, playerNumber, playerMovements);
            for (int i = 0; i < _currentPlayer; i++)
            {
                await CheckIfTookNote(noteController, i, _playersMovements[i]);
            }
        }
        
        // Check if we took a key
        foreach (KeyScript key in Keys)
        {
            if (!key.gameObject.activeSelf)
                continue;
            
            if (key.Position == CurrentPlayer.CurrentCellPosition)
            {
                key.OnPickup();
                CurrentPlayerMovements.TookKeys.Add(new TookKeyData(key, _currentTurn));
            }
        }

        if (_currentTurn != maxTurns)
            return;
        
        // Here we are at the end of a player round
        _currentTurn = 0;
        _currentPlayer += 1;

        if (_currentPlayer > playerControllers.Length - 1)
        {
            // TODO : End game
            Debug.Log("Game finished");
            if (_notes.Exists(n => n.RemainingNotes > 0))
                Debug.LogError("Game finished with remaining notes :c");
            else
                Debug.LogError("Win !!");

            return;
        }

        _playersMovements[_currentPlayer].Player.gameObject.SetActive(true);

        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            playerMovement.Player.MoveTo(playerMovement.InitialPosition);
        }

        ResetNotes();
    }

    private async Task CheckIfTookNote(NoteController noteController, int playerNumber,
        PlayerMovements playerMovements)
    {
        var player = playerMovements.Player;
        if (noteController.GridPosition != player.CurrentCellPosition)
            return;

        var noteType = playerNumber + 1; // +1 bc between 1 and 3
        var tookNote = await noteController.TakeNote(noteType);

        if (tookNote)
        {
            // TODO : Take note sound
            playerMovements.TookNotes.Add(new TookNoteData(noteController, _currentTurn - 1, noteType));
        }
    }

    private async void CancelMovements(Vector3Int movement)
    {
        Task[] tasks2 = new Task[_currentPlayer + 1];

        for (int j = 0; j < _currentPlayer; j++)
        {
            tasks2[j] = _playersMovements[j].Player.Move(-_playersMovements[j].Movements[_currentTurn]);
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

        // For current player
        List<Task> undoTasks = new();

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
        // Check items
        var lastPickedItem = playerMovement.TookNotes.LastOrDefault();
        if (lastPickedItem != null && lastPickedItem.Turn == _currentTurn - 1)
        {
            var noteController = lastPickedItem.Reference;
            noteController.ReturnNote(lastPickedItem.NoteType);
        }

        // Move Player
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

    public void RegisterNoteController(NoteController noteController)
    {
        if (_notes.Contains(noteController))
            Debug.LogError("Note controller already registered");

        _notes.Add(noteController);
    }

    private class PlayerMovements
    {
        public PlayerController Player { get; private set; }
        public List<Vector3Int> Movements { get; private set; }
        public List<TookNoteData> TookNotes { get; private set; }
        public List<TookKeyData> TookKeys { get; private set; }
        public Vector3Int InitialPosition { get; private set; }

        public PlayerMovements(PlayerController player)
        {
            Player = player;
            Movements = new();
            TookNotes = new();
            InitialPosition = player.CurrentCellPosition;
        }
    }

    private class TookNoteData
    {
        public NoteController Reference { get; private set; }
        public int NoteType { get; private set; }
        public int Turn { get; private set; }

        public TookNoteData(NoteController note, int turn, int noteType)
        {
            Reference = note;
            Turn = turn;
            NoteType = noteType;
        }
    }

    private class TookKeyData
    {
        public KeyScript key;

        public Vector3Int position => key.Position;
        public int Turn { get; private set; }

        public TookKeyData(KeyScript key, int turn)
        {
            this.key = key;
            Turn = turn;
        }
    }

    [Serializable]
    private class KeyLockData
    {
        public KeyScript key;
        public LockScript lockScript;
    }
}