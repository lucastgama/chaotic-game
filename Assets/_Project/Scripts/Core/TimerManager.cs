using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public float deckSetupDuration = 60f;
    public float battlePhaseDuration = 30f;

    private float currentTime;
    private bool timerRunning = false;

    private TextMeshProUGUI timerText;

    private void Awake()
    {
        timerText = this.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        StartTimerBasedOnGameState();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            timerRunning = false;
            HandleTimeUp();
        }

        UpdateTimerText();
    }

    private void StartTimerBasedOnGameState()
    {
        switch (GameManager.Instance.CurrentState)
        {
            case GameState.DeckSetup:
                currentTime = deckSetupDuration;
                break;
            case GameState.BattlePhase:
                currentTime = battlePhaseDuration;
                break;
        }

        timerRunning = true;
    }

    private void UpdateTimerText()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timerText.text = seconds.ToString();
    }

    private void HandleTimeUp()
    {
        timerText.text = "00";
        Debug.Log("Time's up for: " + GameManager.Instance.CurrentState);
    }

    public void RestartTimer()
    {
        StartTimerBasedOnGameState();
    }

    public void StopTimer()
    {
        timerRunning = false;
    }
}
