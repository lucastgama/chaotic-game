using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameState currentGameState = GameState.DeckSetup;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameState GetCurrentState()
    {
        return currentGameState;
    }

    public void SetGameState(GameState newState)
    {
        GameState previousState = currentGameState;
        currentGameState = newState;

        Debug.Log($"Estado do jogo mudou de {previousState} para {newState}");

        // Aqui você pode adicionar eventos ou callbacks quando o estado mudar
        OnGameStateChanged(previousState, newState);
    }

    void OnGameStateChanged(GameState previousState, GameState newState)
    {
        // Aqui você pode adicionar lógica específica para mudanças de estado
        switch (newState)
        {
            case GameState.CardInspector:
                // Pausar outras interações, etc.
                break;
            case GameState.DeckSetup:
                // Lógica específica para setup do deck
                break;
            case GameState.BattlePhase:
                // Lógica específica para fase de batalha
                break;
            // Adicione outros casos conforme necessário
        }
    }

    public bool IsInspectorOpen()
    {
        return currentGameState == GameState.CardInspector;
    }

    public bool CanInteractWithCards()
    {
        // Impede interações com cartas quando o inspetor está aberto
        return currentGameState != GameState.CardInspector;
    }
}
