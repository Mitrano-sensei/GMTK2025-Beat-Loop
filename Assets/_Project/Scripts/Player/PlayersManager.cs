using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

public class PlayersManager : Singleton<PlayersManager>
{
    [SerializeField] private PlayerController[] playerControllers;

    [SerializeField] private int maxTurns = 3;

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference undoAction;
    [SerializeField] private InputActionReference restartAction;

    [SerializeField] private List<KeyLockData> keyLockDatas;

    [SerializeField] private Button restartButton;

    private PlayerController CurrentPlayer => _playersMovements[_currentPlayer]?.Player;
    private PlayerMovements CurrentPlayerMovements => _playersMovements[_currentPlayer];
    private PlayerMovements[] PlayerBeforeCurrent => _playersMovements.Take(_currentPlayer).ToArray();

    private List<LockScript> Locks => keyLockDatas.Select(kl => kl.lockScript).ToList();
    private List<KeyScript> Keys => keyLockDatas.Select(kl => kl.key).ToList();

    private PlayerMovements[] _playersMovements;

    private DeathScreen _deathScreen;
    private WinScreen _winScreen;

    private int CurrentTurn
    {
        get => _currentTurn;
        set
        {
            if (_currentTurn == value) return;

            _currentTurn = value;
            if (_clapBar == null) return;
            _clapBar.RemainingClap = maxTurns - _currentTurn;
        }
    }

    private List<NoteController> _notes = new();
    private int _currentPlayer = 0;
    private int _currentTurn;

    private bool _isMoving = false;
    private ClapBar _clapBar;
    private SimpleAudioManager _audioManager;

    private void Start()
    {
        SetupPlayerMovements();
        SetupInputs();

        _deathScreen = DeathScreen.Instance;
        _winScreen = WinScreen.Instance;

        restartButton.onClick.AddListener(async () => await Reset());
    }

    private void SetupInputs()
    {
        moveAction.action.performed += MoveInput();
        undoAction.action.performed += UndoInput();
        restartAction.action.performed += RestartInput();
    }

    private void OnDestroy()
    {
        moveAction.action.performed -= MoveInput();
        undoAction.action.performed -= UndoInput();
        restartAction.action.performed -= RestartInput();
    }

    private Action<InputAction.CallbackContext> RestartInput()
    {
        return async ctx =>
        {
            if (_isMoving) return;
            _isMoving = true;
            await Reset();
            _isMoving = false;
        };
    }

    private Action<InputAction.CallbackContext> UndoInput()
    {
        return async ctx =>
        {
            if (_isMoving) return;

            _isMoving = true;
            await Undo();
            _isMoving = false;
        };
    }

    private Action<InputAction.CallbackContext> MoveInput()
    {
        return async (ctx) =>
        {
            if (_isMoving) return;

            _isMoving = true;
            await OnMove(GetDirectionFromInput(ctx.ReadValue<Vector2>()));
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

    public async Task Reset()
    {
        _deathScreen.Hide();
        _winScreen.Hide();
        _currentPlayer = 0;
        CurrentTurn = 0;

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
        ResetKeys();

        await Task.Yield();
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

    private void ResetKeys()
    {
        foreach (var key in Keys)
        {
            key.Reset();
        }

        foreach (var playerMovement in _playersMovements)
        {
            playerMovement.TookKeys.Clear();
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
        if (CurrentPlayer == null || !CurrentPlayer.isActiveAndEnabled || CurrentPlayer.IsMoving() || movement == Vector3Int.zero)
            return;

        if (!CurrentPlayer.CanMove(movement))
        {
            await CurrentPlayer.FailMove(movement);
            if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
            _audioManager.PlayFailSound();
            return;
        }

        // Check if there is another player in our destination
        foreach (PlayerMovements playerMovement in _playersMovements)
        {
            if (playerMovement == CurrentPlayerMovements)
                continue;

            var playerIsActive = playerMovement.Player.isActiveAndEnabled;
            var playerIsInOurWay = playerMovement.Player.CurrentCellPosition ==
                                   (CurrentPlayer.CurrentCellPosition + movement);

            if (playerIsActive && playerIsInOurWay)
            {
                var playerIsGoingOurWay = playerMovement.Movements[CurrentTurn] == -movement;
                if (playerIsGoingOurWay)
                {
                    List<Task> fails = new();
                    fails.Add(CurrentPlayer.FailMove(movement));
                    fails.Add(playerMovement.Player.FailMove(-movement));
                    
                    if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
                    _audioManager.PlayFailSound();
                    return;
                }
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
                if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
                _audioManager.PlayFailSound();
                return;
            }
        }


        // Move all players
        List<Task> tasks = new();
        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            tasks.Add(playerMovement.Player.Move(playerMovement.Movements[CurrentTurn]));
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
                if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
                _audioManager.PlayFailSound();
                return;
            }
        }

        // Here we can move the player
        CurrentPlayerMovements.Movements.Add(movement);
        CurrentTurn++;

        // Check if we took a note
        List<Task> pickupTasks = new();
        foreach (NoteController noteController in _notes)
        {
            var playerNumber = _currentPlayer;
            var playerMovements = CurrentPlayerMovements;

            pickupTasks.Add(CheckIfTookNote(noteController, playerNumber, playerMovements));

            // Check if another player took a note
            for (int i = 0; i < _currentPlayer; i++)
            {
                pickupTasks.Add(CheckIfTookNote(noteController, i, _playersMovements[i]));
            }
        }

        // Check if we took a key

        foreach (KeyScript key in Keys)
        {
            if (!key.gameObject.activeSelf)
                continue;

            if (key.Position == CurrentPlayer.CurrentCellPosition)
            {
                pickupTasks.Add(key.OnPickup());
                CurrentPlayer.PickupItem();
                CurrentPlayerMovements.TookKeys.Add(new TookKeyData(key, CurrentTurn - 1));
            }

            // Check if another player took a key
            for (int i = 0; i < _currentPlayer; i++)
            {
                if (key.Position == _playersMovements[i].Player.CurrentCellPosition)
                {
                    pickupTasks.Add(key.OnPickup());
                    _playersMovements[i].Player.PickupItem();
                    _playersMovements[i].TookKeys.Add(new TookKeyData(key, CurrentTurn - 1));
                }
            }
        }

        await Task.WhenAll(pickupTasks);

        if (CurrentTurn != maxTurns)
            return;

        // Here we are at the end of a player round

        CurrentTurn = 0;
        _currentPlayer += 1;

        if (_currentPlayer > playerControllers.Length - 1)
        {
            ManageEndGame();
            return;
        }

        _playersMovements[_currentPlayer].Player.gameObject.SetActive(true);

        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            playerMovement.Player.MoveTo(playerMovement.InitialPosition);
        }

        ResetNotes();
        ResetKeys();
    }

    private void ManageEndGame()
    {
        // To enable undo
        _currentPlayer--;
        CurrentTurn = maxTurns;

        Debug.Log("Game finished");
        if (_notes.Exists(n => n.RemainingNotes > 0))
        {
            LooseScreen();
        }
        else
        {
            ShowWinScreen();
        }
    }

    private void LooseScreen()
    {
        if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
        _audioManager.PlayLooseSound();
        _deathScreen.Show();
    }

    [ContextMenu("Win")]
    private void ShowWinScreen()
    {
        if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
        _audioManager.PlayWinSound();
        _winScreen.Show();
    }

    private async Task CheckIfTookNote(NoteController noteController, int playerNumber,
        PlayerMovements playerMovements)
    {
        var player = playerMovements.Player;
        if (noteController.GridPosition != player.CurrentCellPosition)
            return;

        var noteType = playerNumber + 1; // +1 bc between 1 and 3
        if (!noteController.IsNoteAvailable(noteType))
            return;

        await noteController.TakeNote(noteType);

        playerMovements.TookNotes.Add(new TookNoteData(noteController, CurrentTurn - 1, noteType));
        playerMovements.Player.PickupItem();

        await Task.Yield();
    }

    private async void CancelMovements(Vector3Int movement)
    {
        Task[] tasks2 = new Task[_currentPlayer + 1];

        for (int j = 0; j < _currentPlayer; j++)
        {
            tasks2[j] = _playersMovements[j].Player.Move(-_playersMovements[j].Movements[CurrentTurn]);
        }

        tasks2[_currentPlayer] = CurrentPlayer.Move(-movement);
        await Task.WhenAll(tasks2);
        return;
    }

    private async Task Undo()
    {
        _deathScreen.Hide();
        _winScreen.Hide();

        // Allow undo of very last move
        var lastMove = _currentPlayer == _playersMovements.Length - 1 && CurrentTurn == maxTurns;

        if (CurrentTurn == 0 && !lastMove)
        {
            Debug.LogWarning("Can't undo");
            if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
            _audioManager.PlayNotPossibleSound();
            return;
        }

        if (_audioManager == null) _audioManager = SimpleAudioManager.Instance;
        _audioManager.PlayUndoSound();

        await UndoEveryone();

        CurrentTurn--;
    }

    private async Task UndoEveryone()
    {
        // For current player
        List<Task> undoTasks = new();

        undoTasks.Add(UndoPlayer(CurrentPlayerMovements, true));

        // For players before
        foreach (PlayerMovements playerMovement in PlayerBeforeCurrent)
        {
            undoTasks.Add(UndoPlayer(playerMovement));
        }

        await Task.WhenAll(undoTasks);
    }

    private async Task UndoPlayer(PlayerMovements playerMovement, bool removeFromMovement = false)
    {
        List<Task> undoTasks = new();

        // Check notes
        var lastPickedItem = playerMovement.TookNotes.LastOrDefault();
        if (lastPickedItem != null && lastPickedItem.Turn == CurrentTurn - 1)
        {
            var noteController = lastPickedItem.Reference;
            undoTasks.Add(noteController.ReturnNote(lastPickedItem.NoteType));
            playerMovement.TookNotes.Remove(lastPickedItem);
        }

        // Check keys
        var lastPickedKey = playerMovement.TookKeys.LastOrDefault();
        if (lastPickedKey != null && lastPickedKey.Turn == CurrentTurn - 1)
        {
            var key = lastPickedKey.key;
            undoTasks.Add(key.Reset());
            playerMovement.TookKeys.Remove(lastPickedKey);
        }

        // Move Player
        undoTasks.Add(playerMovement.Player.Move(-playerMovement.Movements[CurrentTurn - 1]));
        if (removeFromMovement) playerMovement.Movements.RemoveAt(CurrentTurn - 1);

        await Task.WhenAll(undoTasks);
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

    public int GetMaxTurn()
    {
        return maxTurns;
    }

    public void SetClapBar(ClapBar clapBar)
    {
        _clapBar = clapBar;
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
            TookKeys = new();
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