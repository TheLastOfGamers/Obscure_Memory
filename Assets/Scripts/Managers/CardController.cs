using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Image imageRenderer;

    public bool IsFlipped { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;

    public event Action<CardController> OnCardFlipped;

    private bool isAnimating = false;
    private float flipSpeed = 0.15f;
    private bool cardActive = false;

    private Transform[] bounceTargets;
    private Vector3[] originalScales;

    private void Awake()
    {
        if (imageRenderer == null)
            imageRenderer = GetComponent<Image>();

        // Store all targets you want to bounce
        bounceTargets = new Transform[] { transform, imageRenderer.transform };

        // Save original scales
        originalScales = new Vector3[bounceTargets.Length];
        for (int i = 0; i < bounceTargets.Length; i++)
        {
            originalScales[i] = bounceTargets[i].localScale;
        }
        GameManager.Instance.RegisterCard(this);
        ShowBack();
    }

    public void OnMouseDown()
    {
        if (isAnimating || IsMatched || !cardActive) return;

        StartCoroutine(BounceAndFlip());
        // Notify GameManager
        OnCardFlipped?.Invoke(this);
    }

    private IEnumerator BounceAndFlip()
    {
        isAnimating = true;

        // Step 1: Bounce down slightly
        yield return StartCoroutine(ScaleAll(0.9f, 0.08f));

        // Step 2: Flip
        yield return StartCoroutine(FlipCard());

        // Step 3: Overshoot and return
        yield return StartCoroutine(ScaleAll(1.1f, 0.08f));
        yield return StartCoroutine(ScaleAll(1.0f, 0.08f));

        isAnimating = false;
    }

    public IEnumerator FlipCard()
    {
        // Animate rotation Y from 0 → 90
        for (float t = 0; t < flipSpeed; t += Time.deltaTime)
        {
            float angle = Mathf.Lerp(0f, 90f, t / flipSpeed);
            imageRenderer.transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        // Swap sprite at halfway point
        if (!IsFlipped) ShowFront();
        else ShowBack();

        IsFlipped = !IsFlipped;

        // Animate rotation Y from 90 → 0
        for (float t = 0; t < flipSpeed; t += Time.deltaTime)
        {
            float angle = Mathf.Lerp(90f, 0f, t / flipSpeed);
            imageRenderer.transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        imageRenderer.transform.rotation = Quaternion.identity;
    }

    private IEnumerator ScaleAll(float multiplier, float duration)
    {
        Vector3[] startScales = new Vector3[bounceTargets.Length];
        Vector3[] targetScales = new Vector3[bounceTargets.Length];

        for (int i = 0; i < bounceTargets.Length; i++)
        {
            startScales[i] = bounceTargets[i].localScale;
            targetScales[i] = originalScales[i] * multiplier;
        }

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            for (int i = 0; i < bounceTargets.Length; i++)
            {
                bounceTargets[i].localScale = Vector3.Lerp(startScales[i], targetScales[i], progress);
            }

            yield return null;
        }

        for (int i = 0; i < bounceTargets.Length; i++)
        {
            bounceTargets[i].localScale = targetScales[i];
        }
    }

    public void MarkMatched()
    {
        IsMatched = true;
    }

    public void SetFrontIcon(Sprite icon)
    {
        frontSprite = icon;
    }

    private void ShowFront()
    {
        imageRenderer.sprite = frontSprite;
    }

    private void ShowBack()
    {
        imageRenderer.sprite = backSprite;
    }
    public Sprite GetFrontSprite()
    {
        return frontSprite;
    }
    public void SetCardActive(bool active)
    {
        cardActive = active;
    }
}
