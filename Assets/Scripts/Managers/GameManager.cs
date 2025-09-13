using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

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
    private int score = 0;

    private const string PLAYER_PREF_KEY = "LevelPlayed_"; // + level index

    [SerializeField] private GridManager gridManager;
    [SerializeField] private ResultPanel resultPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        gridManager.OnAllCardsMatched += HandleRoundComplete;
    }

    public void Start()
    {
        SelectLevel(0);
        SelectDifficulty(Difficulty.Medium);
    }

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

        // Pass info to LevelManager
        LevelManager.Instance.StartLevel(SelectedLevel, SelectedDifficulty);
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
        if (card1.GetFrontSprite() == card2.GetFrontSprite())
        {
            card1.MarkMatched();
            card2.MarkMatched();
            score += (int)LevelManager.Instance.ScoreModifier() * 10;

            yield return new WaitForSeconds(0.5f);

            gridManager.RemoveCard(card1);
            gridManager.RemoveCard(card2);

            Destroy(card1.gameObject);
            Destroy(card2.gameObject);
        }
        else
        {
            yield return new WaitForSeconds(0.7f);
            card1.StartCoroutine(card1.FlipCard());
            card2.StartCoroutine(card2.FlipCard());
            score -= (int)math.round(LevelManager.Instance.ScoreModifier() * 2) ;
        }
        print("Score: " + score);
    }

    private void HandleRoundComplete()
    {
        resultPanel.Show(score, true, LevelManager.Instance.HasNextRound());
    }
    public void ResetCards()
    {
        flippedCards.Clear();
        score = 0;
    }
}
