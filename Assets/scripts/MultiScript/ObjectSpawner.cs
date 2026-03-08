using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectSpawner : NetworkBehaviour
{
    [Header("스폰 설정")]
    public GameObject objectPrefab; // 생성할 프리팹 (NetworkObject 필수)
    public int spawnCount = 10;     // 생성할 개수
    public Vector3 spawnAreaCenter; // 생성 범위 중심
    public Vector3 spawnAreaSize;   // 생성 범위 크기 (가로, 세로, 높이)

    public override void OnNetworkSpawn()
    {
        // 반드시 서버에서만 실행 (클라이언트가 생성하면 동기화 안 됨)
        if (IsServer)
        {
            SpawnObjects();
        }
    }

    private void SpawnObjects()
    {
        // x축으로 -90도 회전된 값 계산
        Quaternion spawnRotation = Quaternion.Euler(-90f, 0, 0);

        for (int i = 0; i < spawnCount; i++)
        {
            // 1. 범위 내 랜덤 위치 계산
            Vector3 randomPos = new Vector3(
                Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2),
                Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2), 
                Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2)
            );

            // 2. 오브젝트 생성 (계산한 spawnRotation 적용)
            GameObject go = Instantiate(objectPrefab, randomPos, spawnRotation);

            // 3. 네트워크 상에 스폰 (서버가 명령하면 모든 클라이언트에 이 회전값으로 생성됨)
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}
