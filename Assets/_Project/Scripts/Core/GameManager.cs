using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState startingState = GameState.DeckSetup;

    [Header("Battle Configuration")]
    public BattleMode battleMode = BattleMode.OneVsOne;

    public GameState CurrentState { get; private set; }
    public BattleMode CurrentBattleMode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetGameState(startingState);
        SetBattleMode(battleMode);
    }

    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
    }

    public void SetBattleMode(BattleMode newBattleMode)
    {
        CurrentBattleMode = newBattleMode;
    }

    public int GetMaxCreaturesPerPlayer()
    {
        switch (CurrentBattleMode)
        {
            case BattleMode.OneVsOne:
                return 1;
            case BattleMode.ThreeVsThree:
                return 3;
            case BattleMode.SixVsSix:
                return 6;
            default:
                return 1;
        }
    }
}

public enum BattleMode
{
    OneVsOne = 1,
    ThreeVsThree = 3,
    SixVsSix = 6
}
