using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class setting : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float spinDuration = 0.18f;   
    public float returnDuration = 0.12f; 

    private RectTransform rect;
    private Coroutine routine;
    private float baseZ;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseZ = rect.localEulerAngles.z;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartAnim(SpinOnce());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartAnim(ReturnToBase());
    }

    void StartAnim(IEnumerator anim)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(anim);
    }

    IEnumerator SpinOnce()
    {
        float startZ = rect.localEulerAngles.z;
        float endZ = startZ + 180f;   
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, spinDuration);
            float z = Mathf.Lerp(startZ, endZ, t);
            rect.localEulerAngles = new Vector3(0f, 0f, z);
            yield return null;
        }
    }

    IEnumerator ReturnToBase()
    {
        float startZ = rect.localEulerAngles.z;
        float endZ = baseZ;

        
        startZ = NormalizeAngle(startZ);
        endZ = NormalizeAngle(endZ);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, returnDuration);
            float z = Mathf.LerpAngle(startZ, endZ, t);
            rect.localEulerAngles = new Vector3(0f, 0f, z);
            yield return null;
        }
    }

    static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }
}
