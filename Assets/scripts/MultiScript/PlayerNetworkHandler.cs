using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetworkHandler : NetworkBehaviour
{
    private CharacterController _cc;
    private NetworkTransform _networkTransform;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _networkTransform = GetComponent<NetworkTransform>();
    }

    [ClientRpc]
    public void TeleportPlayerClientRpc(Vector3 targetPos)
    {
        // 1. 내 캐릭터(IsOwner)이거나 서버일 때 실행
        if (IsOwner || IsManagedByServer)
        {
            // 2. CharacterController가 있다면 잠시 끔 (가장 중요!)
            if (_cc != null) _cc.enabled = false;

            // 3. 최신 NetworkTransform을 사용 중이라면 전용 메서드 사용
            if (_networkTransform != null)
            {
                // 이 함수 하나로 물리와 위치 정보가 한꺼번에 초기화됩니다.
                _networkTransform.Teleport(targetPos, Quaternion.identity, transform.localScale);
            }
            else
            {
                // NetworkTransform이 없다면 직접 수정
                transform.position = targetPos;
            }

            // 4. 다시 켜기
            if (_cc != null) _cc.enabled = true;
            
            Debug.Log($"[Teleport] {OwnerClientId}번 플레이어가 {targetPos}로 이동함");
        }
    }
    
    // 이 프로퍼티는 NetworkTransform 설정에 따라 서버/클라이언트 권한을 체크합니다.
    private bool IsManagedByServer => IsServer && !_networkTransform.CanCommitToTransform;
}
