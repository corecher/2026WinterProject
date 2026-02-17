using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform listContainer; 
    [SerializeField] private PlayerListItem itemPrefab; 

    // 동기화되는 플레이어 리스트
    private NetworkList<PlayerData> netPlayers;

    private void Awake()
    {
        netPlayers = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        // 1. [Server] 호스트는 리스트를 초기화하고 채우는 역할을 합니다.
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // ★★★ [핵심 수정] 이미 접속해 있는 플레이어들을 리스트에 추가 ★★★
            // 씬이 로드될 때 나(Host)와 이미 들어와있는 게스트들을 모두 등록합니다.
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // 중복 방지 (혹시 모르니)
                if (!IsPlayerInList(uid))
                {
                    netPlayers.Add(new PlayerData(uid, $"Player {uid}"));
                }
            }
        }

        // 2. [Client & Server] 리스트 변경 감지 연결
        netPlayers.OnListChanged += OnListChanged;
        
        // 3. UI 최초 1회 그리기
        UpdatePlayerListUI();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
        netPlayers.OnListChanged -= OnListChanged;
    }

    // --- [Server Only] 접속 관리 ---
    
    // 나중에 들어오는 사람 처리
    private void OnClientConnected(ulong clientId)
    {
        if (!IsPlayerInList(clientId))
        {
            netPlayers.Add(new PlayerData(clientId, $"Player {clientId}"));
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < netPlayers.Count; i++)
        {
            if (netPlayers[i].ClientId == clientId)
            {
                netPlayers.RemoveAt(i);
                break;
            }
        }
    }

    // 이미 리스트에 있는지 확인하는 헬퍼 함수
    private bool IsPlayerInList(ulong clientId)
    {
        foreach (var p in netPlayers)
        {
            if (p.ClientId == clientId) return true;
        }
        return false;
    }

    // --- [Client & Server] UI 갱신 ---
    private void OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        UpdatePlayerListUI();
    }

    private void UpdatePlayerListUI()
    {
        // 기존 목록 삭제
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        // 리스트 재생성
        foreach (PlayerData data in netPlayers)
        {
            PlayerListItem newItem = Instantiate(itemPrefab, listContainer);
            newItem.SetInfo(data.ClientId, data.PlayerName.ToString());
        }
    }
}
