using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class ResultPanel : MonoBehaviour
{
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Transform starsContainer; // Parent of star GameObjects
    [SerializeField] private Animator animator; // Assign in inspector

    private const string OPEN_ANIM = "ResultPanel_BG_Anim_Open";
    private const string CLOSE_ANIM = "ResultPanel_BG_Anim_Close";

    public void Show(int score, bool won, bool hasNextRound)
    {
        gameObject.SetActive(true);
        StartCoroutine(PlayOpenAnimation(score, won, hasNextRound));
    }

    private IEnumerator PlayOpenAnimation(int score, bool won, bool hasNextRound)
    {
        if (animator != null)
        {
            animator.Play(OPEN_ANIM);
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        // Now activate the child after animation
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX("Result");
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

    public void Hide()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        if (animator != null)
        {
            animator.Play(CLOSE_ANIM);
        }
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
        else if (score >= fullStarScore * 0.1f)
        stars = 0;

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
        LevelManager.Instance.LoadRound(); // Just reload the current round
    }

    private void OnMenuClicked()
    {
        Hide();
        GameManager.Instance.ResetCards();
        SceneManager.LoadScene("MainScene");
        SoundManager.Instance.PlayMusic("FLIPPER_BGM");
        // Implement menu navigation logic here
    }
}