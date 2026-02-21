using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        // 이 객체가 로컬 플레이어(나)의 것인지 확인
        if (IsOwner)
        {
            playerCamera.enabled = true;
            // 필요하다면 시네머신이나 오디오 리스너 설정도 여기서 처리합니다.
        }
    }
}
