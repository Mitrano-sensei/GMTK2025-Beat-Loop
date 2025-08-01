using System;
using System.Collections.Generic;
using UnityEngine;

public class ClapBar : MonoBehaviour
{
    private const int MAX_CLAP = 12;

    [SerializeField] private List<GameObject> clapPrefabs;

    private int ClapCount { get; set; }

    public int RemainingClap
    {
        get => _remainingClap;
        set
        {
            if (_remainingClap == value) return;
            _remainingClap = value;
            RemainingClapChanged?.Invoke(value);
        }
    }

    public event Action<int> RemainingClapChanged;

    private int _remainingClap;

    private PlayersManager _playersManager;

    public void Start()
    {
        _playersManager = PlayersManager.Instance;
        _playersManager.SetClapBar(this);

        ClapCount = _playersManager.GetMaxTurn();
        if (ClapCount > MAX_CLAP) Debug.LogError("ClapCount > MAX_CLAP");

        RemainingClap = ClapCount;
        UpdateNumberOfClap();

        for (int i = ClapCount; i < MAX_CLAP; i++)
        {
            clapPrefabs[i].SetActive(false);
        }

        RemainingClapChanged += nbClap => { UpdateNumberOfClap(); };
    }

    private void UpdateNumberOfClap()
    {
        for (int i = 0; i < ClapCount; i++)
        {
            clapPrefabs[i].SetActive((i + 1 <= RemainingClap));
        }
    }

    [ContextMenu("Clap")]
    public void Clap()
    {
        // TODO : Play clap sound
        RemainingClap--;
    }

    public void Reset()
    {
        ClapCount = _playersManager.GetMaxTurn();
        RemainingClap = ClapCount;
    }
}