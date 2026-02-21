using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    private void Awake()
    {
        // 1. [중요] 게임이 시작되자마자 모든 카메라와 리스너를 일단 끕니다.
        // 이렇게 하면 남의 캐릭터가 스폰될 때 카메라가 켜진 채로 들어오는 것을 막습니다.
        if (playerCamera != null) playerCamera.enabled = false;
        if (audioListener != null) audioListener.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        // 2. 이 객체의 주인이 '나'인 경우에만 다시 켭니다.
        if (IsOwner)
        {
            if (playerCamera != null) 
            {
                playerCamera.enabled = true;
                // 메인 카메라 태그 설정 (필요 시)
                playerCamera.tag = "MainCamera"; 
            }

            if (audioListener != null) 
            {
                audioListener.enabled = true;
            }
            
            Debug.Log($"[CameraSetup] {OwnerClientId}번 플레이어(나)의 카메라를 활성화했습니다.");
        }
        else
        {
            // 주인이 아닌 경우(남의 캐릭터) 확실하게 꺼져 있는지 재확인
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
            
            // 남의 카메라가 MainCamera 태그를 가지고 있으면 꼬일 수 있으므로 태그 제거
            if (playerCamera != null && playerCamera.gameObject.CompareTag("MainCamera"))
            {
                playerCamera.gameObject.tag = "Untagged";
            }
        }
    }
}
