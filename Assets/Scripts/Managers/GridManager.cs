using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab; 
    [SerializeField] private Transform boardContainer; 

    private List<CardController> activeCards = new List<CardController>();

    public void GenerateGrid(RoundData roundData)
    {
        ClearBoard();

        int totalCards = roundData.gridX * roundData.gridY;
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Grid must have an even number of cards.");
            return;
        }

        int pairsNeeded = totalCards / 2;
        List<Sprite> icons = new List<Sprite>();

        for (int i = 0; i < pairsNeeded; i++)
        {
            // pick a random icon from available pool
            Sprite chosenIcon = roundData.cardIcons[Random.Range(0, roundData.cardIcons.Length)];

            // add it twice for the pair
            icons.Add(chosenIcon);
            icons.Add(chosenIcon);
        }

        // Shuffle
        for (int i = 0; i < icons.Count; i++)
        {
            Sprite temp = icons[i];
            int rand = Random.Range(i, icons.Count);
            icons[i] = icons[rand];
            icons[rand] = temp;
        }

        // Spawn cards
        float spacing = 1.2f; 
        for (int y = 0; y < roundData.gridY; y++)
        {
            for (int x = 0; x < roundData.gridX; x++)
            {
                Vector3 pos = new Vector3(x * spacing, y * -spacing, 0);
                GameObject cardObj = Instantiate(cardPrefab, pos, Quaternion.identity, boardContainer);

                CardController card = cardObj.GetComponent<CardController>();
                card.SetFrontIcon(icons[y * roundData.gridX + x]);
                activeCards.Add(card);
            }
        }

        AutoScaleBoard(roundData.gridX, roundData.gridY);
    }

    private void AutoScaleBoard(int cols, int rows)
    {
        float scaleFactor = 5f / Mathf.Max(cols, rows);
        boardContainer.localScale = Vector3.one * scaleFactor;
        boardContainer.localPosition = new Vector3(-(cols - 1) * 0.6f, (rows - 1) * 0.6f, 0);
    }

    private void ClearBoard()
    {
        foreach (Transform child in boardContainer)
        {
            Destroy(child.gameObject);
        }
        activeCards.Clear();
    }
}
