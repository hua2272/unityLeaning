using System;
using UnityEngine;

public enum GameState
{
    Prelude,
    Finale,
    Normal,
    Gameplay,
    Paused,
    GameOver,
    Dialogue
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance { get; private set; }
    
    private GameState currentState;
    public GameState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            OnGameStateChanged?.Invoke(currentState);
        }
    }
    
    public event Action<GameState> OnGameStateChanged;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool CantOpenMenu()
    {
        // 定义哪些状态下不可以打开菜单
        return currentState == GameState.Prelude || currentState == GameState.Finale;
    }
}