using UnityEngine;
using Unity.Netcode;
public class Grinder : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField] private float moveSpeed = 5f; // 전진 속도
    [SerializeField] private Vector3 chaserStartPosition = new Vector3(0, 1, -10); // 시작 위치

    private bool isMoving = false; // 이동 제어용

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ResetChaser(); // 생성 시 위치 초기화
        }
    }

    private void Update()
    {
        if (!IsServer || !isMoving) return;

        // 일직선(앞방향)으로 계속 전진
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
    }

    // 추격자를 시작 위치로 되돌리고 멈추는 함수
    public void ResetChaser()
    {
        if (!IsServer) return;

        transform.position = chaserStartPosition;
        isMoving = false; // 라운드 대기 중에는 멈춤
    }

    // 추격 시작 함수
    public void StartChasing()
    {
        if (!IsServer) return;
        isMoving = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<PlayerStats>(out var stats))
        {
            if (stats.isCaughtThisRound) return;

            stats.isCaughtThisRound = true;
            Debug.Log($"플레이어 {stats.OwnerClientId} 잡힘!");

            var finishLine = FindObjectOfType<FinishLine>();
            if (finishLine != null)
            {
                finishLine.CheckRoundEnd();
            }
        }
    }
}
