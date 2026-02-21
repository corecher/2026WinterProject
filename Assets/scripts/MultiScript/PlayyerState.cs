using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    // 점수 관리 (NetworkVariable 사용)
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);

    // [중요] 위치 이동을 위한 ClientRpc
    [ClientRpc]
    public void TeleportPlayerClientRpc(Vector3 targetPos)
    {
        // 내 캐릭터일 때만 위치를 강제로 옮깁니다.
        if (IsOwner) 
        {
            var cc = GetComponent<BoxCollider>();
            if (cc != null) cc.enabled = false;

            transform.position = targetPos;

            if (cc != null) cc.enabled = true;
            
            Debug.Log($"플레이어가 {targetPos}로 이동되었습니다.");
        }
    }

    public void AddScore(int amount)
    {
        if (IsServer) Score.Value += amount;
    }
}
