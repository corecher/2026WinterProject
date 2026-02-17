using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;

    private Lobby _lobby; // 이 버튼이 담고 있는 로비 정보
    private LobbyListUI _listManager; // 클릭 시 연락할 매니저

    // 데이터 세팅 함수
    public void Initialize(Lobby lobby, LobbyListUI listManager)
    {
        _lobby = lobby;
        _listManager = listManager;

        roomNameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";
    }

    // 버튼 OnClick 이벤트에 연결할 함수
    public void OnJoinClick()
    {
        if (_listManager != null)
        {
            _listManager.JoinRoom(_lobby);
        }
    }
}
