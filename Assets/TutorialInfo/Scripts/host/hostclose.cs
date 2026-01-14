using UnityEngine;
using System.Collections;
public class hostclose : MonoBehaviour
{
    public GameObject hostPanel;          
    public RectTransform hostPanelRect;   

    
    public Vector2 openedPos = new Vector2(0, 0);

    
    public Vector2 closedPos = new Vector2(0, 700);

    public float duration = 0.25f;

    Coroutine routine;

   
    public void CloseHostPanel()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        float t = 0f;
        Vector2 from = hostPanelRect.anchoredPosition;
        Vector2 to = closedPos;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float x = Mathf.Clamp01(t / duration);
            float ease = Mathf.SmoothStep(0f, 1f, x);

            hostPanelRect.anchoredPosition = Vector2.Lerp(from, to, ease);
            yield return null;
        }

        hostPanelRect.anchoredPosition = to;

     
        hostPanel.SetActive(false);
    }
}
