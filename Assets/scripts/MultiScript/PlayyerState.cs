using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    // 점수를 네트워크를 통해 동기화 (모든 클라이언트가 읽기 가능, 서버만 쓰기 가능)
    public NetworkVariable<int> Score = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void AddScore(int points)
    {
        if (!IsServer) return; // 서버에서만 점수를 수정할 수 있음
        Score.Value += points;
        Debug.Log($"Player {OwnerClientId} 점수 획득! 현재 점수: {Score.Value}");
    }
    [ClientRpc]
    public void TeleportPlayerClientRpc(Vector3 targetPosition)
    {
        // 내 캐릭터일 때만 실행 (Owner 권한 존중)
        if (IsOwner)
        {
            // 만약 CharacterController가 있다면 잠시 꺼야 합니다.
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = targetPosition;

            if (cc != null) cc.enabled = true;
            
            Debug.Log("클라이언트: 시작 지점으로 이동 완료");
        }
    }
}
