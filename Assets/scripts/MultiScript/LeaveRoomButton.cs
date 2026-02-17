using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;

public class LeaveRoomButton : MonoBehaviour
{
    public async void OnLeaveClicked()
    {
        // 1. 방장(Host)이라면 로비 서비스에서 방을 완전히 삭제
        if (NetworkManager.Singleton.IsHost)
        {
            try
            {
                if (!string.IsNullOrEmpty(RoomManager.CurrentLobbyId))
                {
                    await LobbyService.Instance.DeleteLobbyAsync(RoomManager.CurrentLobbyId);
                    Debug.Log("방장이 나가서 로비를 삭제했습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"로비 삭제 실패: {e.Message}");
            }
        }
        else
        {
            // (선택사항) 클라이언트라면 로비에서 내 플레이어 정보 제거
            // 보통 Shutdown만 해도 일정 시간 후 자동으로 사라지지만, 즉시 반영하려면 RemovePlayerAsync 사용
        }

        // 2. ID 초기화
        RoomManager.CurrentLobbyId = null;

        // 3. 넷코드 연결 종료 (이게 실행되면 NetworkUISwitcher가 UI를 바꿉니다)
        NetworkManager.Singleton.Shutdown();
        
        Debug.Log("방에서 나갔습니다.");
    }
}
