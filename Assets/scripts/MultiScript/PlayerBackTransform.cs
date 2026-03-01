using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가

public class PlayerBoundaryCheck : NetworkBehaviour
{
    [Header("설정")]
    public float maxDistance = 10f;
    public string anchorTag = "TeleportAnchor"; // 찾을 오브젝트의 태그
    private Transform targetObject;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // 1. 처음 스폰되었을 때 찾기
        FindAnchor();

        // 2. 이후 씬이 바뀔 때마다 실행되도록 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        // 메모리 누수 방지를 위해 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 호출되는 콜백 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsServer) return;
        
        Debug.Log($"새로운 씬 로드됨: {scene.name}. 앵커를 다시 찾습니다.");
        FindAnchor();
    }

    private void FindAnchor()
    {
        GameObject anchor = GameObject.FindGameObjectWithTag(anchorTag);
        if (anchor != null)
        {
            targetObject = anchor.transform;
            Debug.Log("Teleport Anchor를 찾았습니다.");
        }
        else
        {
            Debug.LogWarning("Teleport Anchor를 찾지 못했습니다. 씬에 태그가 설정된 오브젝트가 있는지 확인하세요.");
        }
    }

    void Update()
    {
        if (!IsServer || targetObject == null) return;

        if (Vector3.Distance(transform.position, targetObject.position) > maxDistance)
        {
            TeleportToTarget();
        }
    }

    private void TeleportToTarget()
    {
        transform.position = targetObject.position;
        // NetworkTransform 동기화를 위해 필요한 경우 추가 로직(예: RequestTeleport)이 필요할 수 있습니다.
    }
}