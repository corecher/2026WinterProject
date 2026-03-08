using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;

public class FinalRankingUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Transform rankingContainer; // 프리팹들이 생성될 부모 객체 (Vertical Layout Group)
    [SerializeField] private GameObject playerRankRowPrefab; // 만들어뒀던 한 줄짜리 UI 프리팹

    private void Start()
    {
        // 씬 로딩 후 클라이언트들의 데이터 동기화가 완벽히 끝날 시간을 살짝 줍니다.
        Invoke("ShowFinalRanking", 0.5f);
    }

    private void ShowFinalRanking()
    {
        // 1. Finish 씬으로 넘어온 모든 PlayerStats 찾기 및 점수순 정렬
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None)
            .OrderByDescending(p => p.Score.Value)
            .ToList();
        // 2. 순위대로 UI 프리팹 생성 및 데이터 세팅
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            GameObject rowObj = Instantiate(playerRankRowPrefab, rankingContainer);
            PlayerRankUIRow rowUI = rowObj.GetComponent<PlayerRankUIRow>();
            if (rowUI != null)
            {
                // 이름과 점수, 게이지 바 계산
                string playerName = $"Player {player.OwnerClientId}";
                
                // PlayerRankUIRow 스크립트에 데이터 전달
                rowUI.SetName(playerName);
            }
        }
    }
    public void QuitGame()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        Application.Quit(); // 빌드된 게임 종료
    }
}
