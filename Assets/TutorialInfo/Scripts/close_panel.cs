using UnityEngine;
using System.Collections;
public class close_panel : MonoBehaviour
{
    public GameObject closePanel;
    public RectTransform closePanelRect;


    public Vector2 openedPos = new Vector2(0, 0);


    public Vector2 closedPos = new Vector2(0, 700);

    public float duration = 0.25f;

    Coroutine routine;


    public void ClosePanel()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        float t = 0f;
        Vector2 from = closePanelRect.anchoredPosition;
        Vector2 to = closedPos;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float x = Mathf.Clamp01(t / duration);
            float ease = Mathf.SmoothStep(0f, 1f, x);

            closePanelRect.anchoredPosition = Vector2.Lerp(from, to, ease);
            yield return null;
        }

        closePanelRect.anchoredPosition = to;


        closePanel.SetActive(false);
    }
}
