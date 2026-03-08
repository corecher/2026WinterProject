using System.Collections; // 코루틴을 위해 추가
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    private List<ulong> finishedPlayers = new List<ulong>();
    [SerializeField] private int[] rankScores = { 100, 50, 30, 10 };
    [SerializeField] private Vector3 startPosition = new Vector3(0, 1, 0);
    
    private bool isProcessingRoundEnd = false; // 중복 실행 방지
    // FinishLine.cs 의 OnTriggerEnter 부분 수정
    public override void OnNetworkSpawn()
    {
        // 서버가 네트워크 상에 생성되면 첫 라운드 로직을 실행합니다.
        if (IsServer)
        {
            // 약간의 대기 시간을 주어 플레이어들이 접속할 틈을 주는 것이 좋습니다.
            Invoke("StartNextRound", 2.0f); 
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsPlayerObject)
        {
            ulong clientId = networkObject.OwnerClientId;

            if (!finishedPlayers.Contains(clientId))
            {
                finishedPlayers.Add(clientId);
                
                if (other.TryGetComponent<PlayerStats>(out var stats))
                {
                    // [수정] 잡힌 상태라면 0점, 아니면 등수 점수 부여
                    int rank = finishedPlayers.Count;
                    int scoreToGive = (rank <= rankScores.Length) ? rankScores[rank - 1] : 5;

                    if (stats.isCaughtThisRound) 
                    {
                        scoreToGive = 0; 
                        Debug.Log($"플레이어 {clientId}는 추격자에게 잡혔으므로 0점 처리됩니다.");
                    }

                    stats.AddScore(scoreToGive);
                }

                CheckRoundEnd();
            }
        }
    }

    // StartNextRound 에서 상태 초기화 추가
    // FinishLine.cs 의 StartNextRound 부분 수정
    private void StartNextRound()
    {
        finishedPlayers.Clear();

        // 1. 추격자 리셋 및 출발
        var chaser = FindObjectOfType<Grinder>();
        if (chaser != null)
        {
            chaser.ResetChaser();   // 위치 이동
            chaser.StartChasing();  // 다시 달리기 시작
        }

        // 2. 플레이어들 리셋
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent<PlayerStats>(out var stats))
            {
                stats.ResetRoundStatus();
                stats.TeleportPlayerClientRpc(startPosition);
            }
        }
    }

    public void CheckRoundEnd()
    {
        if (!IsServer) return;

        int totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int completedCount = 0;

        // 모든 연결된 플레이어를 확인
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent<PlayerStats>(out var stats))
            {
                // 1. 이미 결승선을 통과했는가?
                bool hasFinished = finishedPlayers.Contains(client.ClientId);
                
                // 2. 추격자에게 잡혔는가?
                bool isCaught = stats.isCaughtThisRound;

                // 둘 중 하나라도 해당되면 이번 라운드 '완료' 상태로 간주
                if (hasFinished || isCaught)
                {
                    completedCount++;
                }
            }
        }

        Debug.Log($"현재 완료 상태: {completedCount} / {totalPlayers}");

        // 완료된 인원이 전체 인원과 같으면 라운드 종료
        if (completedCount >= totalPlayers && !isProcessingRoundEnd)
        {
            StartCoroutine(RoundEndRoutine());
        }
    }

    private IEnumerator RoundEndRoutine()
    {
        isProcessingRoundEnd = true;
        ShowResultUIClientRpc(true);

        yield return new WaitForSeconds(3.0f);

        bool isGameOver = false;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent<PlayerStats>(out var stats))
            {
                if (stats.Score.Value >= 20)
                {
                    isGameOver = true;
                    break;
                }
            }
        }

        if (isGameOver)
        {
            Debug.Log("게임 최종 종료 - 결과 씬으로 이동합니다.");
            // [핵심] 모든 연결된 클라이언트를 'GameEndScene'으로 이동시킴
            // 문자열은 유니티 Build Settings에 등록된 씬 이름과 정확히 일치해야 합니다.
            NetworkManager.Singleton.SceneManager.LoadScene("Finish", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("다음 라운드 준비");
            ShowResultUIClientRpc(false);
            StartNextRound();
        }

        isProcessingRoundEnd = false;
    }

    

    [ClientRpc]
    private void ShowResultUIClientRpc(bool show)
    {
        if (show)
            ResultUIHandler.Instance.ShowScoreBoard();
        else
            ResultUIHandler.Instance.HideScoreBoard(); // UI 숨기기 함수 호출
    }
}