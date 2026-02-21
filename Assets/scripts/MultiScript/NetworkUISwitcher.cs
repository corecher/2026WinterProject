using Unity.Netcode;
using UnityEngine;

public class NetworkUISwitcher : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject titleUI; 
    [SerializeField] private GameObject lobbyUI;

    private void Start()
    {
        bool isConnected = NetworkManager.Singleton != null && 
                       (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer);

        // 2. 연결되어 있다면 로비 UI를, 아니라면 타이틀 UI를 보여줌
        titleUI.SetActive(!isConnected);
        lobbyUI.SetActive(isConnected);

        // NetworkManager 이벤트 연결
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
            titleUI.SetActive(false);
            lobbyUI.SetActive(true);
        }
    }

    // 방에서 나갔거나 끊겼을 때 호출됨
    private void OnClientDisconnected(ulong clientId)
    {
        // 나 자신이 끊긴 경우 로비로 복귀
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("방에서 나갔습니다. 로비로 돌아갑니다.");
            titleUI.SetActive(true);
            lobbyUI.SetActive(false);
        }
    }
}
