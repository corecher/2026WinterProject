using Unity.Netcode;
using UnityEngine;

public class NetworkUISwitcher : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject lobbyUI; // 꺼질 UI (로비, 방 만들기 등)
    [SerializeField] private GameObject gameUI;  // 켜질 UI (게임 화면, 조이스틱 등)

    private void Start()
    {
        // 초기 상태 설정: 로비는 켜고, 게임 UI는 끄기
        lobbyUI.SetActive(true);
        gameUI.SetActive(false);

        // NetworkManager가 준비되면 이벤트 연결
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 연결 해제 (메모리 누수 방지)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    // 누군가(나 포함) 방에 들어왔을 때 호출됨
    private void OnClientConnected(ulong clientId)
    {
        // 접속한 사람이 '나(Local Client)'일 경우에만 UI 변경
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("방 입장 성공! UI를 전환합니다.");
            lobbyUI.SetActive(false);
            gameUI.SetActive(true);
        }
    }

    // 방에서 나갔거나 끊겼을 때 호출됨
    private void OnClientDisconnected(ulong clientId)
    {
        // 나 자신이 끊긴 경우 로비로 복귀
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("방에서 나갔습니다. 로비로 돌아갑니다.");
            lobbyUI.SetActive(true);
            gameUI.SetActive(false);
        }
    }
}
