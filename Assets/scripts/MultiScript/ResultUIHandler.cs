using UnityEngine;
using TMPro; // TextMeshPro 사용 시
using System.Linq;

public class ResultUIHandler : MonoBehaviour
{
    public static ResultUIHandler Instance;
    
    [SerializeField] private GameObject resultPanel; // 결과창 부모 오브젝트
    [SerializeField] private TextMeshProUGUI scoreText; // 점수를 표시할 텍스트

    private void Awake()
    {
        Instance = this;
        resultPanel.SetActive(false); // 시작할 때는 꺼둠
    }

    public void ShowScoreBoard()
    {
        resultPanel.SetActive(true);
        
        // 씬에 있는 모든 PlayerStats를 가져와서 점수순으로 정렬
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None)
            .OrderByDescending(p => p.Score.Value)
            .ToList();

        string leaderboard = "=== RANKING ===\n";
        for (int i = 0; i < players.Count; i++)
        {
            leaderboard += $"{i + 1}위: Player {players[i].OwnerClientId} - {players[i].Score.Value}점\n";
        }

        scoreText.text = leaderboard;
    }
    public void HideScoreBoard()
    {
        resultPanel.SetActive(false);
    }
}