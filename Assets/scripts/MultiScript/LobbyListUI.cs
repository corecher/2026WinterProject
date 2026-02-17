using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent; // ScrollView의 Content
    [SerializeField] private LobbyItem lobbyItemPrefab; // 위에서 만든 프리팹

    private float refreshTimer = 0f;

    private void Update()
    {
        // 2초마다 자동으로 방 목록 새로고침 (실제 게임에선 수동 버튼 추천)
        refreshTimer += Time.deltaTime;
        if (refreshTimer > 2f)
        {
            refreshTimer = 0f;
            RefreshLobbyList();
        }
    }

    // 1. 로비 목록 가져오기
    public async void RefreshLobbyList()
    {
        try
        {
            // 검색 옵션 설정
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 20; // 상위 20개만
            
            // 필터: 빈 자리가 있는 방만 검색 (AvailableSlots > 0)
            options.Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            };

            // 검색 요청
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            // UI 갱신
            UpdateUI(response.Results);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("로비 검색 실패 (아직 로그인이 안됐거나 인터넷 문제): " + e.Message);
        }
    }

    // 2. UI 그리기
    private void UpdateUI(List<Lobby> lobbies)
    {
        // 기존 목록 삭제 (초기화)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 새 목록 생성
        foreach (Lobby lobby in lobbies)
        {
            LobbyItem newItem = Instantiate(lobbyItemPrefab, contentParent);
            newItem.Initialize(lobby, this);
        }
    }

    // 3. 방 입장 로직 (LobbyItem 스크립트에서 호출됨)
    public async void JoinRoom(Lobby lobby)
    {
        Debug.Log($"방 입장 시도: {lobby.Name}");

        try
        {
            // A. 선택한 로비에 참가 요청
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            RoomManager.CurrentLobbyId = joinedLobby.Id;
            // B. 로비 데이터에서 'JoinCode' 키를 찾아냄 (방장이 저장해둔 값)
            string joinCode = joinedLobby.Data["JoinCode"].Value;

            // C. JoinCode로 Relay 서버 정보 얻기
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // D. NetworkManager에 Relay 접속 정보 입력
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            // E. Netcode 클라이언트 시작!
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError("방 입장 실패: " + e.Message);
        }
    }
}
