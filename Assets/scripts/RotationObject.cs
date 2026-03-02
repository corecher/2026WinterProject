using UnityEngine;

public class RotationObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    void Update()
    {
        transform.Rotate(Vector3.right*rotateSpeed*Time.deltaTime);
    }
}
