using UnityEngine;
using System.Collections;

public class TileView : MonoBehaviour
{
    public int X;
    public int Y;
    public SpriteRenderer plantIconRenderer;
    public float tileWorldSize = 1f;

    public SpriteRenderer fillRenderer;
    private Color baseFillColor;
    public bool IsOccupied { get; private set; }

    private int clickCount = 0;
    private Coroutine resetCoroutine;
    private Coroutine shakeCoroutine;
    private Vector3 originalLocalPosition;

    private void Start()
    {
        originalLocalPosition = this.transform.localPosition;
    }

    public string GetLabel()
    {
        char letter = (char)('A' + X);
        return $"{letter}{Y + 1}";
    }

    public void SetFillColor(Color color)
    {
        if (fillRenderer != null)
        {
            fillRenderer.color = color;
        }
        baseFillColor = color;
    }

    public void SetPlant(Sprite icon)
    {
        plantIconRenderer.sprite = icon;
        plantIconRenderer.gameObject.SetActive(true);
        IsOccupied = true;

        float targetSize = tileWorldSize * 0.95f;
        Vector3 spriteSize = icon.bounds.size;
        float scaleX = targetSize / spriteSize.x;
        float scaleY = targetSize / spriteSize.y;
        plantIconRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    public bool HandleOverwriteClick(Sprite newIcon)
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        clickCount++;

        if (clickCount == 1)
        {
            if (fillRenderer != null)
            {
                fillRenderer.color = new Color(baseFillColor.r * 0.5f, baseFillColor.g * 0.5f, baseFillColor.b * 0.5f, baseFillColor.a);
            }
        }
        else if (clickCount == 2)
        {
            if (shakeCoroutine == null)
            {
                shakeCoroutine = StartCoroutine(ShakeRoutine());
            }
        }
        else if (clickCount >= 3)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            transform.localPosition = originalLocalPosition;
            if (fillRenderer != null)
            {
                fillRenderer.color = baseFillColor;
            }
            SetPlant(newIcon);
            clickCount = 0;
            return true;
        }

        resetCoroutine = StartCoroutine(ResetAfterDelay());
        return false;
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        clickCount = 0;
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        transform.localPosition = originalLocalPosition;
        if (fillRenderer != null)
        {
            fillRenderer.color = baseFillColor;
        }
    }

    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            float rx = Random.Range(-0.05f, 0.05f);
            float ry = Random.Range(-0.05f, 0.05f);
            transform.localPosition = originalLocalPosition + new Vector3(rx, ry, 0f);
            yield return new WaitForSeconds(0.03f);
        }
    }
}