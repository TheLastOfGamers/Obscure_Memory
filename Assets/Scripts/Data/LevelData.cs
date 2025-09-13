using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "CardMatch/Level Data", order = 2)]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public Sprite levelThumbnail;

    [Header("Rounds in this level")]
    public RoundData[] rounds;
}
