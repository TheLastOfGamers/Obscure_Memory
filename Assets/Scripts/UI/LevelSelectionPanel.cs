using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class LevelSelectionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image levelIcon;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button prevBtn;
    [SerializeField] private Button selectBtn;
    [SerializeField] private Transform difficultyContainer;
    [SerializeField] private GameObject difficultyBtnPrefab;
    [SerializeField] private Sprite questionMarkIcon;
    [SerializeField] private GameObject diffInfoPanel;
    [SerializeField] private TMP_Text diffInfoTitleText;
    [SerializeField] private TMP_Text diffInfoDescText;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private GameObject mainMenuPanel;

    private LevelData[] levels;
    private int currentIndex = 0;
    private Difficulty selectedDifficulty;

    private void Start()
    {
        titleText.text = "Select Level";
        levels = GameManager.Instance.GetAllLevels();
        ShowLevel(currentIndex);
        nextBtn.onClick.AddListener(OnNextClicked);
        prevBtn.onClick.AddListener(OnPrevClicked);
        selectBtn.onClick.AddListener(OnSelectLevelClicked);
    }

    private void ShowLevel(int index)
    {
        if (levels == null || levels.Length == 0) return;

        backBtn.gameObject.SetActive(true);
        if (index < levels.Length)
        {
            var level = levels[index];
            levelIcon.sprite = level.levelThumbnail;
            levelNameText.text = level.levelName;
            selectBtn.gameObject.SetActive(true);
        }
        else
        {
            // Coming soon case
            levelIcon.sprite = questionMarkIcon;
            levelNameText.text = "Coming Soon!";
            selectBtn.gameObject.SetActive(false);
        }
    }
    private void OnNextClicked()
    {
        if (levels == null) return;
        // Allow cycling to "coming soon" slot
        currentIndex = (currentIndex + 1) % (levels.Length + 1);
        ShowLevel(currentIndex);
    }

    private void OnPrevClicked()
    {
        if (levels == null) return;
        currentIndex = (currentIndex - 1 + (levels.Length + 1)) % (levels.Length + 1);
        ShowLevel(currentIndex);
    }

    private void OnSelectLevelClicked()
    {
        if (currentIndex < levels.Length)
        {
            GameManager.Instance.SelectLevel(currentIndex);
            ShowDifficultySelection();
        }
    }

    private void ShowDifficultySelection()
    {
        titleText.text = "Select Difficulty";
        levelIcon.gameObject.SetActive(false);
        levelNameText.gameObject.SetActive(false);
        nextBtn.gameObject.SetActive(false);
        prevBtn.gameObject.SetActive(false);
        selectBtn.gameObject.SetActive(false);
        
        backBtn.gameObject.SetActive(true);
        difficultyContainer.gameObject.SetActive(true);
        Debug.Log("Showing difficulty selection");
        // Clear previous buttons
        foreach (Transform child in difficultyContainer)
            Destroy(child.gameObject);

        foreach (Difficulty diff in System.Enum.GetValues(typeof(Difficulty)))
        {
            var btnObj = Instantiate(difficultyBtnPrefab, difficultyContainer);

            var btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = diff.ToString();

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedDifficulty = diff;
                ShowDiffInfo(selectedDifficulty);
            });
        }
    }

    private void ShowDiffInfo(Difficulty diff)
    {
        diffInfoPanel.SetActive(true);
        diffInfoTitleText.text = diff.ToString();
        diffInfoDescText.text = GetDifficultyDescription(diff);
        playBtn.onClick.RemoveAllListeners();
        playBtn.onClick.AddListener(() => OnDifficultySelected(diff));
    }

    public string GetDifficultyDescription(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return "Easy:\n-10% score modifier\n+20% round time\nCombo: Untimed (combo only breaks on invalid match).";
            case Difficulty.Normal:
                return "Normal:\nNo score modifier\nStandard round time\nCombo: Untimed (combo only breaks on invalid match).";
            case Difficulty.Hard:
                return "Hard:\n+10% score modifier\n-20% round time\nCombo: Timed (combo breaks on invalid match or after a short time).";
            default:
                return "Unknown difficulty.";
        }
    }

    private void OnDifficultySelected(Difficulty diff)
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        GameManager.Instance.SelectDifficulty(diff);
    }

    public void OnBackButtonClicked()
    {
        diffInfoPanel.SetActive(false);
        if (difficultyContainer.gameObject.activeSelf)
        {
            Debug.Log("Back to level selection");
            // Go back to level selection
            difficultyContainer.gameObject.SetActive(false);
            titleText.text = "Select Level";
            levelIcon.gameObject.SetActive(true);
            levelNameText.gameObject.SetActive(true);
            nextBtn.gameObject.SetActive(true);
            prevBtn.gameObject.SetActive(true);
            ShowLevel(currentIndex);
        }
        else
        {
            Debug.Log("Back to main menu");
            backBtn.gameObject.SetActive(false);
            mainMenuPanel.SetActive(true);
            levelSelectionPanel.SetActive(false);
        }
    }
}