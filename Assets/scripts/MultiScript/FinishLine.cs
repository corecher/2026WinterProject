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

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsPlayerObject)
        {
            ulong clientId = networkObject.OwnerClientId;

            if (!finishedPlayers.Contains(clientId))
            {
                finishedPlayers.Add(clientId);
                
                int rank = finishedPlayers.Count;
                int scoreToGive = (rank <= rankScores.Length) ? rankScores[rank - 1] : 5;
                if (other.TryGetComponent<PlayerStats>(out var stats))
                {
                    stats.AddScore(scoreToGive);
                }

                CheckRoundEnd();
            }
        }
    }

    private void CheckRoundEnd()
    {
        int totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;

        // 모든 플레이어가 도착했고, 아직 라운드 종료 처리 중이 아닐 때
        if (finishedPlayers.Count >= totalPlayers && !isProcessingRoundEnd)
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
            if (stats.Score.Value > 1)
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

    private void StartNextRound()
    {
        finishedPlayers.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent<PlayerStats>(out var stats))
            {
                stats.TeleportPlayerClientRpc(startPosition);
            }
        }
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