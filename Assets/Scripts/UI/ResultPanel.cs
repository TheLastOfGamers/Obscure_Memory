using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Transform starsContainer; // Parent of star GameObjects

    public void Show(int score, bool won, bool hasNextRound)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        scoreText.text = $"Score: {score}";

        nextRoundButton.gameObject.SetActive(won && hasNextRound);
        retryButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);

        nextRoundButton.onClick.RemoveAllListeners();
        retryButton.onClick.RemoveAllListeners();
        menuButton.onClick.RemoveAllListeners();

        nextRoundButton.onClick.AddListener(OnNextRoundClicked);
        retryButton.onClick.AddListener(OnRetryClicked);
        menuButton.onClick.AddListener(OnMenuClicked);

        UpdateStars(score);
    }

    private void UpdateStars(int score)
    {
        // Get fullStarScore from current round
        int fullStarScore = LevelManager.Instance.SelectedLevel.rounds[LevelManager.Instance.CurrentRoundIndex].fullStarScore;

        int stars = 0;
        if (score >= fullStarScore)
            stars = 3;
        else if (score >= fullStarScore * 0.66f)
            stars = 2;
        else if (score >= fullStarScore * 0.33f)
            stars = 1;

        for (int i = 0; i < 3; i++)
        {
            var starObj = starsContainer.Find(i.ToString());
            if (starObj != null)
            {
                var img = starObj.GetComponent<Image>();
                img.color = (i < stars) ? Color.white : Color.black;
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnNextRoundClicked()
    {
        Hide();
        GameManager.Instance.ResetCards();
        LevelManager.Instance.CompleteRound();
    }

    private void OnRetryClicked()
    {
        Hide();
        GameManager.Instance.ResetCards();
        LevelManager.Instance.StartLevel(GameManager.Instance.SelectedLevel, GameManager.Instance.SelectedDifficulty);
    }

    private void OnMenuClicked()
    {
        Hide();
        // Implement menu navigation logic here
    }
}