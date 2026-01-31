using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverLift : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public float liftAmount = 10f;   
    public float speed = 10f;           

    private RectTransform rect;
    private Vector2 originalPos;
    private Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
        targetPos = originalPos;
    }

    void Update()
    {
        rect.anchoredPosition =
            Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPos = originalPos + Vector2.up * liftAmount;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPos = originalPos;
    }
}

