using UnityEngine;

[CreateAssetMenu(fileName = "RoundData", menuName = "CardMatch/Round Data", order = 1)]
public class RoundData : ScriptableObject
{
    [Header("Grid Size")]
    public int gridX = 2; // columns
    public int gridY = 2; // rows

    [Header("Available Card Icons")]
    public Sprite[] cardIcons;

    [Header("Other Settings")]
    public int roundTimer = 20; // in seconds
    public bool noDuplicatePairs; // if true, each icon can only appear once in the grid
    public int fullStarScore = 10;
}