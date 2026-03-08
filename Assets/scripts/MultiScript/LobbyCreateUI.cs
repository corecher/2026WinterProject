using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button[] playerCountButtons; // 인원수 선택 버튼들
    [SerializeField] private Button createButton;

    [Header("Settings")]
    private int maxPlayers = 4; // 기본값

    private async void Start()
    {
        // 1. Unity 서비스 초기화 및 익명 로그인
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        statusText.text = "방을 생성하세요.";
        
        // 버튼 리스너 연결
        createButton.onClick.AddListener(CreateRoom);
    }

    // 인원수 버튼에 연결할 함수 (인스펙터나 코드로 연결)
    public void SetMaxPlayers(int count)
    {
        maxPlayers = count;
        statusText.text = $"선택된 인원: {maxPlayers}명";
        
        // (선택사항) 버튼 시각적 피드백 로직을 여기에 추가 가능
    }

    public async void CreateRoom()
    {
        string roomName = roomNameInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            statusText.text = "방 이름을 입력해주세요!";
            return;
        }

        statusText.text = "방 생성 중...";

        try
        {
            // 1. Relay 할당 생성 (호스트 포함이므로 maxPlayers)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            
            // 2. Relay 접속 코드(JoinCode) 생성
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 3. NGO Transport에 Relay 정보 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 4. 로비 옵션 설정 (Relay JoinCode를 데이터에 포함)
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member, 
                        value: joinCode)
                }
            };

            // 5. 로비 서비스에 방 생성 요청
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);

            RoomManager.CurrentLobbyId = lobby.Id;
            
            statusText.text = $"방 생성 성공! ({lobby.Name})";

            // 6. Netcode 호스트 시작
            NetworkManager.Singleton.StartHost();
        }
        catch (System.Exception e)
        {
            statusText.text = "방 생성 실패: " + e.Message;
            Debug.LogError(e);
        }
    }
}
