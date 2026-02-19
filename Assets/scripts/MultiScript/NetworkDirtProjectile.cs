using UnityEngine;
using Unity.Netcode;

public class NetworkDirtProjectile : NetworkBehaviour
{
    private ulong targetId;
    private float speed;
    private float slowAmount; // 이전 기획에 있던 슬로우 수치 추가
    private bool isInitialized = false;

    // 서버에서 호출 (투사체 생성 직후)
    public void Initialize(ulong targetNetworkId, float projectileSpeed, float slow)
    {
        if (!IsServer) return;

        targetId = targetNetworkId;
        speed = projectileSpeed;
        slowAmount = slow;
        isInitialized = true;
        
        // 데이터 전파를 위해 ClientRpc를 쓰거나, 
        // 혹은 단순히 서버 이동 + NetworkTransform 조합을 씁니다.
    }

    void Update()
    {
        // 서버에서만 이동 및 충돌 판정 수행
        if (!IsServer || !isInitialized) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject targetObj))
        {
            // 1. 방향 계산
            Vector3 targetPos = targetObj.transform.position + Vector3.up * 0.5f; // 타겟의 중심점(허리쯤) 조준
            Vector3 direction = (targetPos - transform.position).normalized;

            // 2. 이동
            transform.position += direction * speed * Time.deltaTime;

            // 3. 회전 (타겟을 바라보게 함)
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // 4. 거리 체크 (또는 OnTriggerEnter 권장)
            if (Vector3.Distance(transform.position, targetPos) < 0.6f)
            {
                HitTarget(targetObj);
            }
        }
        else
        {
            // 타겟이 게임에서 나가거나 파괴됨 -> 투사체 정리
            GetComponent<NetworkObject>().Despawn();
        }
    }

    void HitTarget(NetworkObject target)
    {
        // 상대방 SkillManager에 슬로우/스턴 적용
        if (target.TryGetComponent<SkillManager>(out var targetSkill))
        {
            // 예: targetSkill.ApplySlow(slowAmount, 2f);
            Debug.Log($"{target.name}에게 흙 투사체 적중! 슬로우 적용.");
        }
        
        // 투사체 제거
        GetComponent<NetworkObject>().Despawn();
    }
}
