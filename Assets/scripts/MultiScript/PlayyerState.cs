using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요

public class PlayerStats : NetworkBehaviour
{
    // 점수 관리 (NetworkVariable 사용)
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);

    [Header("설정")]
    public string resetTargetSceneName = "GameScene"; // 점수를 초기화하고 싶은 씬 이름
    // PlayerStats.cs 에 추가
    public bool isCaughtThisRound = false; // 이번 라운드 탈락 여부

    // 라운드 시작 시 상태를 초기화하기 위한 함수
    public void ResetRoundStatus()
    {
        if (IsServer)
        {
            isCaughtThisRound = false;
        }
    }
    public override void OnNetworkSpawn()
    {
        // 서버에서만 씬 전환 이벤트를 구독합니다.
        if (IsServer)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (IsServer)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // 씬이 로드될 때 실행되는 함수 (서버 전용)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsServer) return;

        // 현재 로드된 씬 이름이 설정한 이름과 일치하면 점수 초기화
        if (scene.name == resetTargetSceneName)
        {
            Score.Value = 0;
            Debug.Log($"[Server] {scene.name} 씬 진입: 점수가 초기화되었습니다.");
        }
    }

    // [중요] 위치 이동을 위한 ClientRpc
    [ClientRpc]
    public void TeleportPlayerClientRpc(Vector3 targetPos)
    {
        if (IsOwner) 
        {
            // 1. CharacterController가 있다면 반드시 비활성화 (가장 흔한 원인)
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 2. Rigidbody가 있다면 속도(Velocity) 초기화
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 3. 위치 이동
            transform.position = targetPos;
            
            // 4. (중요) 위치 동기화를 위해 Physics 엔진에 위치 갱신 알림
            // 물리 연산이 텔레포트된 위치를 '이전 위치'로 인식하게 만듭니다.
            Physics.SyncTransforms();

            // 5. 컴포넌트 재활성화
            if (cc != null) cc.enabled = true;

            Debug.Log($"[Client] {targetPos}로 위치 고정 완료.");
        }
    }

    public void AddScore(int amount)
    {
        if (IsServer) Score.Value += amount;
    }
}
