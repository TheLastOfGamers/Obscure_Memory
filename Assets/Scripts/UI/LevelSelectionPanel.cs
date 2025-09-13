using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    private LevelData[] levels;
    private int currentIndex = 0;

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

        difficultyContainer.gameObject.SetActive(true);

        // Clear previous buttons
        foreach (Transform child in difficultyContainer)
            Destroy(child.gameObject);

        foreach (Difficulty diff in System.Enum.GetValues(typeof(Difficulty)))
        {
            var btnObj = Instantiate(difficultyBtnPrefab, difficultyContainer);

            var btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = diff.ToString();

            btnObj.GetComponent<Button>().onClick.AddListener(() => OnDifficultySelected(diff));
        }
    }

    private void OnDifficultySelected(Difficulty diff)
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        GameManager.Instance.SelectDifficulty(diff);
    }
}