using UnityEngine;
using System.Collections;

public class TutorialCameraFocus : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform followTarget;         
    public Vector3 followOffset = new Vector3(0f, 1.2f, -3.5f); 
    public bool offsetInTargetSpace = true; 

    [Header("Smoothing")]
    public float positionSmooth = 10f;     
    public float rotationSmooth = 12f;     

    [Header("Look At")]
    public Vector3 lookAtOffset = new Vector3(0f, 1.0f, 0f);   

    private bool focusing;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Coroutine blendRoutine;

    void Awake()
    {
        SaveOriginal();
    }

    void LateUpdate()
    {
        if (!focusing || followTarget == null)
        {
            Debug.Log("½ÇÆÐ");
            return;
        }

       
        Vector3 desiredPos = offsetInTargetSpace
            ? followTarget.TransformPoint(followOffset)
            : followTarget.position + followOffset;

        
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            1f - Mathf.Exp(-positionSmooth * Time.deltaTime)
        );

        
        Vector3 lookPoint = followTarget.position + lookAtOffset;
        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            1f - Mathf.Exp(-rotationSmooth * Time.deltaTime)
        );
    }

    void SaveOriginal()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    
    public void StartFocus(Transform target)
    {
        followTarget = target;
        if (!focusing) SaveOriginal();
        focusing = true;
    }

    
    public void StopFocus()
    {
        focusing = false;

        if (blendRoutine != null) StopCoroutine(blendRoutine);
        blendRoutine = StartCoroutine(BlendBack(0.25f)); 
    }

    IEnumerator BlendBack(float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            transform.position = Vector3.Lerp(startPos, originalPos, t);
            transform.rotation = Quaternion.Slerp(startRot, originalRot, t);
            yield return null;
        }
    }
}

