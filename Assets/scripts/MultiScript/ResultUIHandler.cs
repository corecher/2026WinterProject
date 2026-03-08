using UnityEngine;
using TMPro; 
using System.Linq;
using System.Collections.Generic;

public class ResultUIHandler : MonoBehaviour
{
    public static ResultUIHandler Instance;
    
    [Header("UI 연결")]
    [SerializeField] private GameObject resultPanel; 
    [SerializeField] private Transform rankingContainer; // 프리팹들이 생성될 부모 객체 (Vertical Layout Group 사용 권장)
    [SerializeField] private GameObject playerRankRowPrefab; // 방금 만든 UI 한 줄짜리 프리팹
    
    [Header("게임 설정")]
    [SerializeField] private float maxWinScore = 20f; // FinishLine과 동일한 승리 목표 점수

    private List<GameObject> spawnedRows = new List<GameObject>(); // 생성된 UI들을 추적하기 위한 리스트

    private void Awake()
    {
        Instance = this;
        resultPanel.SetActive(false); 
    }

    public void ShowScoreBoard()
    {
        resultPanel.SetActive(true);
        
        // 1. 기존에 띄워둔 리스트가 있다면 모두 삭제 (초기화)
        foreach(var row in spawnedRows)
        {
            Destroy(row);
        }
        spawnedRows.Clear();

        // 2. 씬에 있는 모든 PlayerStats를 가져와서 점수순으로 정렬
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None)
            .OrderByDescending(p => p.Score.Value)
            .ToList();

        // 3. 순위대로 UI 프리팹 생성 및 데이터 세팅
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            
            // 프리팹 생성
            GameObject rowObj = Instantiate(playerRankRowPrefab, rankingContainer);
            spawnedRows.Add(rowObj);

            // 데이터 전달
            PlayerRankUIRow rowUI = rowObj.GetComponent<PlayerRankUIRow>();
            if (rowUI != null)
            {
                string playerName = $"{i + 1}위: Player {player.OwnerClientId}";
                
                // 점수를 0~1 사이의 비율로 계산 (예: 10점 / 20점 = 0.5)
                float fillAmount = Mathf.Clamp01((float)player.Score.Value / maxWinScore);
                
                rowUI.Setup(playerName, player.Score.Value, fillAmount);
            }
        }
    }

    public void HideScoreBoard()
    {
        resultPanel.SetActive(false);
    }
}