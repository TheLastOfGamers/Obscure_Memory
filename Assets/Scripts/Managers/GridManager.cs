using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private RectTransform boardContainer;

    private List<CardController> activeCards = new List<CardController>();
    public event Action OnAllCardsMatched;

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

        if (roundData.noDuplicatePairs)
        {
            if (pairsNeeded > roundData.cardIcons.Length)
            {
                Debug.LogError("Not enough unique icons in RoundData for 'noDuplicatePairs'.");
                return;
            }

            // Pick unique icons
            List<Sprite> available = new List<Sprite>(roundData.cardIcons);
            for (int i = 0; i < pairsNeeded; i++)
            {
                int rand = UnityEngine.Random.Range(0, available.Count);
                Sprite chosen = available[rand];
                available.RemoveAt(rand);

                icons.Add(chosen);
                icons.Add(chosen);
            }
        }
        else
        {
            // Allow duplicates
            for (int i = 0; i < pairsNeeded; i++)
            {
                Sprite chosen = roundData.cardIcons[UnityEngine.Random.Range(0, roundData.cardIcons.Length)];
                icons.Add(chosen);
                icons.Add(chosen);
            }
        }

        // Shuffle
        for (int i = 0; i < icons.Count; i++)
        {
            Sprite temp = icons[i];
            int rand = UnityEngine.Random.Range(i, icons.Count);
            icons[i] = icons[rand];
            icons[rand] = temp;
        }

        // Calculate card size
        Vector2 boardSize = boardContainer.rect.size;
        float cardWidth = boardSize.x / roundData.gridX;
        float cardHeight = boardSize.y / roundData.gridY;
        float cardSize = Mathf.Min(cardWidth, cardHeight) * 0.9f;

        // Spawn cards
        for (int y = 0; y < roundData.gridY; y++)
        {
            for (int x = 0; x < roundData.gridX; x++)
            {
                GameObject cardObj = Instantiate(cardPrefab, boardContainer);

                RectTransform rt = cardObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cardSize, cardSize);

                float posX = (x - (roundData.gridX - 1) / 2f) * cardWidth;
                float posY = -(y - (roundData.gridY - 1) / 2f) * cardHeight;
                rt.anchoredPosition = new Vector2(posX, posY);

                CardController card = cardObj.GetComponent<CardController>();
                card.SetFrontIcon(icons[y * roundData.gridX + x]);
                activeCards.Add(card);
            }
        }
        SoundManager.Instance.PlaySFX("GameStart");
        StartCoroutine(InitiateCards());
    }

    private IEnumerator InitiateCards()
    {
        // Flip open all cards
        yield return StartCoroutine(FlipAllCards());

        // Keep them open for 2 seconds
        yield return new WaitForSeconds(2f);

        // Flip them all back
        yield return StartCoroutine(FlipAllCards());

        foreach (var card in activeCards)
        {
            card.SetCardActive(true);
        }
        
        GameManager.Instance.StartRoundTimer();
    }

    private IEnumerator FlipAllCards()
    {
        // Start all flips
        foreach (var card in activeCards)
        {
            StartCoroutine(card.FlipCard());
        }
        yield return new WaitForSeconds(0.5f);
    }


    private void ClearBoard()
    {
        foreach (Transform child in boardContainer)
        {
            Destroy(child.gameObject);
        }
        activeCards.Clear();
    }
    public void RemoveCard(CardController card)
    {
        activeCards.Remove(card);
        if (activeCards.Count == 0)
        {
            OnAllCardsMatched?.Invoke();
        }
    }
}
