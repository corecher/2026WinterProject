using UnityEngine;
using Unity.Netcode;

public class NetworkDirtProjectile : NetworkBehaviour
{
    private ulong targetId;
    private float speed;
    private bool isInitialized = false;

    // 서버에서 생성 직후 호출됨
    public void SetTarget(ulong targetNetworkId, float projectileSpeed)
    {
        targetId = targetNetworkId;
        speed = projectileSpeed;
        isInitialized = true;
        
        // 클라이언트들에게도 타겟 정보 전파 (위치 동기화 외에 로직 동기화용)
        SetTargetClientRpc(targetNetworkId, projectileSpeed);
    }

    [ClientRpc]
    void SetTargetClientRpc(ulong targetNetworkId, float projectileSpeed)
    {
        if (IsServer) return; // 서버는 이미 알고 있음
        targetId = targetNetworkId;
        speed = projectileSpeed;
        isInitialized = true;
    }

    void Update()
    {
        // 이동 로직은 서버에서만 처리하고 NetworkTransform으로 동기화하는 것이 가장 깔끔함
        if (!IsServer || !isInitialized) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject targetObj))
        {
            Vector3 direction = (targetObj.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetObj.transform.position) < 0.5f)
            {
                // 적중!
                HitTarget(targetObj);
            }
        }
        else
        {
            // 타겟 사라짐 -> 자폭
            GetComponent<NetworkObject>().Despawn();
        }
    }

    void HitTarget(NetworkObject target)
    {
        // 데미지나 슬로우 효과 처리 (여기서는 예시로 디스폰만)
        Debug.Log("흙 투사체 적중!");
        
        // 투사체 삭제 (Despawn)
        GetComponent<NetworkObject>().Despawn();
    }
}
