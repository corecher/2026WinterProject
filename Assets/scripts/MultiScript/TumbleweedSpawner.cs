using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class TumbleweedSpawner : NetworkBehaviour
{
    public GameObject tumbleweedPrefab; // NetworkObject가 부착된 프리팹
    public float spawnInterval = 3f;    // 스폰 간격
    public Vector3 spawnArea = new Vector3(20, 0, 20); // 스폰 범위
    public float lifeTime = 10f;        // 회전초 유지 시간

    public override void OnNetworkSpawn()
    {
        // 서버에서만 코루틴을 시작합니다.
        if (IsServer)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnTumbleweed();
        }
    }

    void SpawnTumbleweed()
    {
        // 1. 랜덤 위치 계산
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            100f, // 지면보다 살짝 위
            Random.Range(-spawnArea.z, spawnArea.z)
        );

        // 2. 오브젝트 생성 (Instantiate)
        GameObject go = Instantiate(tumbleweedPrefab, randomPos, Quaternion.identity);

        // 3. 네트워크 상에 복제 (Spawn)
        NetworkObject no = go.GetComponent<NetworkObject>();
        no.Spawn();

        // 4. 일정 시간 후 파괴 예약
        StartCoroutine(DespawnAfterDelay(no, lifeTime));
    }

    IEnumerator DespawnAfterDelay(NetworkObject no, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 오브젝트가 아직 존재한다면 네트워크 상에서 제거
        if (no != null && no.IsSpawned)
        {
            no.Despawn(); // 모든 클라이언트에서 파괴됨
            // Despawn(true)를 하면 메모리(Destroy)까지 한 번에 정리됩니다.
        }
    }
}
