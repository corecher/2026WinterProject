using Unity.Netcode;
using UnityEngine;

public class Tumbleweed : NetworkBehaviour
{
    public float windStrength = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 물리 연산은 서버에서만 계산하여 위치를 동기화합니다.
    void FixedUpdate()
    {
        if (!IsServer) return; 

        // 간단한 바람 효과 (예: X축 방향)
        Vector3 windDirection = Vector3.right + (Vector3.forward * Mathf.Sin(Time.time));
        rb.AddForce(windDirection * windStrength);
    }
}
