using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class button : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public float moveDistance = 10f;      
    public float duration = 0.15f;        
    public float colorBrighten = 1.1f;    

    private RectTransform rect;
    private Vector2 originalPos;
    private Image image;
    private Color originalColor;

    private Coroutine currentCoroutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        originalPos = rect.anchoredPosition;
        originalColor = image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartHover(originalPos + Vector2.up * moveDistance,
                   originalColor * colorBrighten);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartHover(originalPos, originalColor);
    }

    void StartHover(Vector2 targetPos, Color targetColor)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(Animate(targetPos, targetColor));
    }

    IEnumerator Animate(Vector2 targetPos, Color targetColor)
    {
        Vector2 startPos = rect.anchoredPosition;
        Color startColor = image.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
    }
}
