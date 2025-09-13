using UnityEngine;

[CreateAssetMenu(fileName = "RoundData", menuName = "CardMatch/Round Data", order = 1)]
public class RoundData : ScriptableObject
{
    [Header("Grid Size")]
    public int gridX = 2; // columns
    public int gridY = 2; // rows

    [Header("Available Card Icons")]
    public Sprite[] cardIcons;
}