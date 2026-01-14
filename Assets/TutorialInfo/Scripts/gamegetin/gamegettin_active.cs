using UnityEngine;
using System.Collections;
public class gamegettin_active : MonoBehaviour
{
    public GameObject imageObject;
    public RectTransform imageRect;

    public Vector2 startPos = new Vector2(1200, 0);
    public Vector2 endPos = new Vector2(0, 0);
    public float duration = 0.4f;

    Coroutine routine;


    public void ShowAndSlideDown()
    {
        if (routine != null) StopCoroutine(routine);


        imageObject.SetActive(true);


        imageRect.anchoredPosition = startPos;

        routine = StartCoroutine(SlideDown());
    }

    IEnumerator SlideDown()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float x = Mathf.Clamp01(t / duration);

            float ease = Mathf.SmoothStep(0f, 1f, x);
            imageRect.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);

            yield return null;
        }
        imageRect.anchoredPosition = endPos;
    }
}
