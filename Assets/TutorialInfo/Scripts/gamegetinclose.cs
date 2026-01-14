using UnityEngine;
using System.Collections;
public class gamegetinclose : MonoBehaviour
{
    public GameObject gamegetinPanel;
    public RectTransform gamegetinRect;


    public Vector2 openedPos = new Vector2(0, 0);


    public Vector2 closedPos = new Vector2(-1000, 0);

    public float duration = 0.25f;

    Coroutine routine;
    public void ClosegamegetinPanel()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        float t = 0f;
        Vector2 from = gamegetinRect.anchoredPosition;
        Vector2 to = closedPos;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float x = Mathf.Clamp01(t / duration);
            float ease = Mathf.SmoothStep(0f, 1f, x);

            gamegetinRect.anchoredPosition = Vector2.Lerp(from, to, ease);
            yield return null;
        }

        gamegetinRect.anchoredPosition = to;


        gamegetinPanel.SetActive(false);
    }
}
