using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Save player's current round in a level
    public void SaveProgress(int levelIndex, int roundIndex)
    {
        PlayerPrefs.SetInt(GetProgressKey(levelIndex), roundIndex);
        PlayerPrefs.Save();
    }

    // Load player's current round in a level
    public int LoadProgress(int levelIndex)
    {
        return PlayerPrefs.GetInt(GetProgressKey(levelIndex), 0);
    }

    // Save best score for a round in a level
    public void SaveBestScore(int levelIndex, int roundIndex, int score)
    {
        string key = GetBestScoreKey(levelIndex, roundIndex);
        int prevBest = PlayerPrefs.GetInt(key, 0);
        if (score > prevBest)
        {
            PlayerPrefs.SetInt(key, score);
            PlayerPrefs.Save();
        }
    }

    // Load best score for a round in a level
    public int LoadBestScore(int levelIndex, int roundIndex)
    {
        return PlayerPrefs.GetInt(GetBestScoreKey(levelIndex, roundIndex), 0);
    }

    // Save best time for a round in a level
    public void SaveBestTime(int levelIndex, int roundIndex, float time)
    {
        string key = GetBestTimeKey(levelIndex, roundIndex);
        float prevBest = PlayerPrefs.GetFloat(key, float.MaxValue);
        if (time < prevBest)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
        }
    }

    // Load best time for a round in a level
    public float LoadBestTime(int levelIndex, int roundIndex)
    {
        return PlayerPrefs.GetFloat(GetBestTimeKey(levelIndex, roundIndex), float.MaxValue);
    }

    private string GetProgressKey(int levelIndex) => $"Progress_Level_{levelIndex}";
    private string GetBestScoreKey(int levelIndex, int roundIndex) => $"BestScore_Level_{levelIndex}_Round_{roundIndex}";
    private string GetBestTimeKey(int levelIndex, int roundIndex) => $"BestTime_Level_{levelIndex}_Round_{roundIndex}";

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}