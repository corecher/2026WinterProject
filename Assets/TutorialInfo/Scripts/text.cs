using UnityEngine;
public class text : MonoBehaviour
{
    [Header("Å©°í ÀÛ¾ÆÁü")]
    public float scaleAmount = 0.1f;
    public float scaleSpeed = 2f;

    [Header("À§¾Æ·¡")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 1.5f;

    [Header("Á©¸®")]
    public float stretchAmount = 0.3f;
    public float stretchSpeed = 6f;

    Vector3 startPos;
    Vector3 startScale;
    float seed;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;
        seed = Random.Range(0f, 10f);
    }

    void Update()
    {
        float t = Time.time + seed;

        
        float scalePulse = 1 + Mathf.Sin(t * scaleSpeed) * scaleAmount;

       
        float stretchX = 1 + Mathf.Sin(t * stretchSpeed) * stretchAmount;

        Vector3 finalScale = new Vector3(
            startScale.x * scalePulse * stretchX,
            startScale.y * scalePulse * (1 - stretchAmount * 0.25f),
            startScale.z
        );

        transform.localScale = finalScale;

        
        float yOffset = Mathf.Sin(t * floatSpeed) * floatHeight;
        transform.localPosition = startPos + Vector3.up * yOffset;
    }
}
