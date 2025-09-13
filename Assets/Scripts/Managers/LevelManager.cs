using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private LevelData currentLevel;
    private Difficulty currentDifficulty;

    private int currentRoundIndex = 0;
    private float roundTimer;

    [SerializeField] private GridManager gridManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartLevel(LevelData level, Difficulty difficulty)
    {
        currentLevel = level;
        currentDifficulty = difficulty;
    }

    public void LoadRound()
    {
        if (currentRoundIndex >= currentLevel.rounds.Length)
        {
            Debug.Log("Level complete!");
            return;
        }

        RoundData round = currentLevel.rounds[currentRoundIndex];

        // Apply difficulty modifiers to round timer
        roundTimer = round.roundTimer;
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                roundTimer *= 1.1f; // +10% time
                break;
            case Difficulty.Hard:
                roundTimer *= 0.9f; // -10% time
                break;
        }

        // Generate grid for this round
        gridManager.GenerateGrid(round);
        Debug.Log($"Round {currentRoundIndex + 1} started, Timer: {roundTimer}, Difficulty: {currentDifficulty}, Grid: X{round.gridX}, Y{round.gridY}");
    }

    public void CompleteRound()
    {
        currentRoundIndex++;
        LoadRound();
    }

    // Combo scoring logic will hook here later
    public bool IsTimedCombo()
    {
        return currentDifficulty != Difficulty.Easy;
    }

    public LevelData SelectedLevel => currentLevel;
    public int CurrentRoundIndex => currentRoundIndex;

    public float ScoreModifier()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return 0.9f;   // -10%
            case Difficulty.Hard: return 1.1f;   // +10%
            default: return 1.0f;
        }
    }
    public bool HasNextRound()
    {
        return currentRoundIndex < currentLevel.rounds.Length - 1;
    }
    public float GetRoundTimer()
    {
        return roundTimer;
    }
    public void SetCurrentRound(int roundIndex)
    {
        currentRoundIndex = Mathf.Clamp(roundIndex, 0, currentLevel.rounds.Length - 1);
        LoadRound();
    }
}
