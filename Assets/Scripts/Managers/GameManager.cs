using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    [SerializeField] private LevelData[] allLevels;

    public LevelData SelectedLevel { get; private set; }
    public Difficulty SelectedDifficulty { get; private set; }

    private Queue<CardController> flippedCards = new Queue<CardController>();

    private const string PLAYER_PREF_KEY = "LevelPlayed_"; // + level index

    private GridManager gridManager;
    private ResultPanel resultPanel;
    //Combo
    private TMP_Text comboText;
    private int comboCount = 0;
    private float comboMultiplier = 1f;
    private Coroutine comboTimerCoroutine;
    //Score
    private int score = 0;

    //Timer
    private float roundTimer;
    private float timerRemaining;
    private TMP_Text timerText;
    private Coroutine timerCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            gridManager = FindObjectOfType<GridManager>();
            resultPanel = FindObjectOfType<ResultPanel>();
            comboText = GameObject.Find("Combo_Text")?.GetComponent<TMP_Text>();
            timerText = GameObject.Find("Timer_Text")?.GetComponent<TMP_Text>();

            if (gridManager != null)
                gridManager.OnAllCardsMatched += HandleRoundComplete;

                
            LevelManager.Instance.StartLevel(SelectedLevel, SelectedDifficulty);
        }
    }

    // public void Start()
    // {
    //     SelectLevel(0);
    //     SelectDifficulty(Difficulty.Medium);
    // }

    // Called when player selects a level
    public bool HasPlayerPlayedLevel(int levelIndex)
    {
        return PlayerPrefs.GetInt(PLAYER_PREF_KEY + levelIndex, 0) == 1;
    }

    public void MarkLevelAsPlayed(int levelIndex)
    {
        PlayerPrefs.SetInt(PLAYER_PREF_KEY + levelIndex, 1);
        PlayerPrefs.Save();
    }

    public void SelectLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= allLevels.Length)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        SelectedLevel = allLevels[levelIndex];
    }

    public void SelectDifficulty(Difficulty difficulty)
    {
        SelectedDifficulty = difficulty;

        SceneManager.LoadScene("GameScene");

        SoundManager.Instance.PlayMusic("FLIPPER_BGM_2");
    }

    public void RegisterCard(CardController card)
    {
        card.OnCardFlipped += HandleCardFlipped;
    }

    private void HandleCardFlipped(CardController card)
    {
        // If player flips back the first card (already flipped)
        if (flippedCards.Contains(card) && flippedCards.Count == 1)
        {
            flippedCards.Clear();
            return;
        }

        if (!flippedCards.Contains(card))
            flippedCards.Enqueue(card);

        // Process matches as soon as two cards are flipped
        if (flippedCards.Count >= 2)
        {
            var first = flippedCards.Dequeue();
            var second = flippedCards.Dequeue();
            StartCoroutine(CheckMatch(first, second));
        }
    }

    private IEnumerator CheckMatch(CardController card1, CardController card2)
    {
        bool isMatch = card1.GetFrontSprite() == card2.GetFrontSprite();

        if (isMatch)
        {
            card1.MarkMatched();
            card2.MarkMatched();

            // Combo logic
            comboCount++;
            comboMultiplier = 1f + (comboCount - 1) * 0.2f; // Example: 1.0, 1.2, 1.4, ...

            // Hard difficulty: reset combo timer
            if (SelectedDifficulty == Difficulty.Hard)
            {
                if (comboTimerCoroutine != null)
                    StopCoroutine(comboTimerCoroutine);
                comboTimerCoroutine = StartCoroutine(ComboTimer());
            }

            score += Mathf.RoundToInt(LevelManager.Instance.ScoreModifier() * 10 * comboMultiplier);

            yield return new WaitForSeconds(0.5f);
            SoundManager.Instance.PlaySFX("Match");
            gridManager.RemoveCard(card1);
            gridManager.RemoveCard(card2);

            Destroy(card1.gameObject);
            Destroy(card2.gameObject);
        }
        else
        {
            // Combo cancel logic
            comboCount = 1;
            comboMultiplier = 1f;

            // Hard difficulty: stop combo timer
            if (SelectedDifficulty == Difficulty.Hard && comboTimerCoroutine != null)
            {
                StopCoroutine(comboTimerCoroutine);
                comboTimerCoroutine = null;
            }

            yield return new WaitForSeconds(0.7f);
            SoundManager.Instance.PlaySFX("Invalid");
            card1.StartCoroutine(card1.FlipCard());
            card2.StartCoroutine(card2.FlipCard());
            score -= Mathf.RoundToInt(LevelManager.Instance.ScoreModifier() * 2);
        }
        UpdateCombo(comboCount, comboMultiplier);
        print($"Score: {score} | Combo: {comboCount} | Multiplier: {comboMultiplier}");
    }

    private IEnumerator ComboTimer()
    {
        float timer = 3f; // Combo expires after 3 seconds (adjust as needed)
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        // Combo expired
        comboCount = 1;
        comboMultiplier = 1f;
        comboTimerCoroutine = null;
        UpdateCombo(comboCount, comboMultiplier);
        print("Combo expired!");
    }

    private void HandleRoundComplete()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        if (resultPanel != null)
            resultPanel.Show(score, true, LevelManager.Instance.HasNextRound());
        else
            Debug.LogError("ResultPanel reference is missing in GameManager!");
    }
    public void ResetCards()
    {
        flippedCards.Clear();
        score = 0;
        comboCount = 1;
        comboMultiplier = 1f;
        if (comboTimerCoroutine != null)
        {
            StopCoroutine(comboTimerCoroutine);
            comboTimerCoroutine = null;
        }
        UpdateCombo(comboCount, comboMultiplier);
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        timerRemaining = 0;
        UpdateTimerUI();
    }
    public void UpdateCombo(int comboCount, float comboMultiplier)
    {
        if (comboText != null)
        {
            if (comboCount > 1)
                comboText.text = $"Combo: x{comboCount} ({comboMultiplier:0.0}x)";
            else
                comboText.text = "";
        }
    }

    public void StartRoundTimer()
    {
        timerText.gameObject.SetActive(true);
        roundTimer = LevelManager.Instance.GetRoundTimer();
        timerRemaining = roundTimer;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
    }
    private IEnumerator TimerRoutine()
    {
        bool countdownSFXPlayed = false;
        while (timerRemaining > 0)
        {
            timerRemaining -= Time.deltaTime;
            if (!countdownSFXPlayed && timerRemaining < 4)
            {
                SoundManager.Instance.PlaySFX("Countdown");
                countdownSFXPlayed = true;
            }
            UpdateTimerUI();
            yield return null;
        }
        timerRemaining = 0;
        UpdateTimerUI();
        HandleTimerEnd();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(timerRemaining)}";
    }

    private void HandleTimerEnd()
    {
        // Show result panel with failure
        resultPanel.Show(score, false, LevelManager.Instance.HasNextRound());
        timerText.gameObject.SetActive(false);
    }
    public LevelData[] GetAllLevels()
    {
        return allLevels;
    }
}
