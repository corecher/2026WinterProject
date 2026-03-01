using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요

public class PlayerStats : NetworkBehaviour
{
    // 점수 관리 (NetworkVariable 사용)
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);

    [Header("설정")]
    public string resetTargetSceneName = "GameScene"; // 점수를 초기화하고 싶은 씬 이름

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
            // CharacterController를 사용 중이라면 .enabled = false가 필요할 수 있습니다.
            // 여기서는 코드에 적어주신 BoxCollider 예시를 유지합니다.
            var cc = GetComponent<BoxCollider>();
            if (cc != null) cc.enabled = false;

            transform.position = targetPos;

            if (cc != null) cc.enabled = true;
            
            Debug.Log($"플레이어가 {targetPos}로 이동되었습니다.");
        }
    }

    public void AddScore(int amount)
    {
        if (IsServer) Score.Value += amount;
    }
}
