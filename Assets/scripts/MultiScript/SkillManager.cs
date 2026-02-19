using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class SkillManager : NetworkBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private VehicleSkillData skillData;
    
    // 쿨다운은 로컬(UI표시용)과 서버(검증용) 양쪽에서 관리
    private float currentCooldown = 0f; 

    // [동기화] 불도저 쉴드 상태 (서버가 쓰고 모두가 읽음)
    private NetworkVariable<bool> isShieldActive = new NetworkVariable<bool>(false);

    // [동기화] 현재 잡고 있는 대상의 NetworkObjectId (없으면 0)
    private NetworkVariable<ulong> grabbedTargetId = new NetworkVariable<ulong>(0);

    private Rigidbody rb;
    private HeavyVehicleController controller;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<HeavyVehicleController>();
    }

    void Update()
    {
        // 1. 쿨다운은 각자 돕니다 (UI 갱신용)
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        // 2. 내 캐릭터(IsOwner)만 입력을 처리
        if (IsOwner)
        {
            HandleInput();
        }
        
        // 3. [서버] 쉴드 타이머 처리
        if (IsServer && isShieldActive.Value)
        {
            // (서버에서 별도 타이머 로직이 필요하거나, Coroutine으로 처리)
            // 아래 Coroutine 방식 사용함
        }
    }

    // 서버 권한으로 잡은 대상 위치 갱신 (매 프레임)
    void FixedUpdate()
    {
        if (IsServer && grabbedTargetId.Value != 0)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(grabbedTargetId.Value, out NetworkObject targetObj))
            {
                // 위치 고정
                Vector3 holdPos = transform.position + transform.forward * 2f + Vector3.up * 2f;
                targetObj.transform.position = holdPos;
            }
            else
            {
                // 대상이 접속 끊음 등으로 사라지면 해제
                grabbedTargetId.Value = 0;
            }
        }
    }

    void HandleInput()
    {
        // 스킬 사용 요청
        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
        {
            // 잡고 있는 상태라면 던지기, 아니면 스킬 사용
            if (grabbedTargetId.Value != 0)
            {
                ThrowTargetServerRpc();
            }
            else
            {
                RequestSkillServerRpc(); // 서버에게 "나 스킬 쓸래" 요청
                currentCooldown = skillData.cooldownTime; // 로컬 쿨다운 즉시 적용 (반응성)
            }
        }
    }

    // ==================== 서버 로직 (ServerRpc) ====================

    [ServerRpc]
    void RequestSkillServerRpc()
    {
        if (skillData == null) return;

        // 타입별 스킬 실행
        switch (skillData.vehicleType)
        {
            case VehicleType.Excavator:
                StartCoroutine(ExcavatorSkillRoutine());
                break;
            case VehicleType.Bulldozer:
                StartCoroutine(BulldozerSkillRoutine());
                break;
            case VehicleType.DumpTruck:
                DumpTruckSkill();
                break;
        }
    }

    // --- 포크레인 ---
    IEnumerator ExcavatorSkillRoutine()
    {
        // 딜레이 후 잡기 시도
        yield return new WaitForSeconds(skillData.excavatorAnimationDelay);

        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * skillData.excavatorGrabRange * 0.5f,
            skillData.excavatorGrabRange * 0.5f
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // NetworkObject가 있는 대상만 잡을 수 있음
            if (hit.TryGetComponent<NetworkObject>(out NetworkObject targetNetObj))
            {
                // 플레이어 태그 확인
                if (hit.CompareTag("Player") || hit.GetComponent<HeavyVehicleController>() != null)
                {
                    grabbedTargetId.Value = targetNetObj.NetworkObjectId;
                    
                    // 잡힌 대상 물리 끄기 (ClientRpc로 전파)
                    SetTargetKinematicClientRpc(targetNetObj.NetworkObjectId, true);
                    Debug.Log($"[Server] 잡음: {targetNetObj.NetworkObjectId}");
                    break; 
                }
            }
        }
    }

    [ServerRpc]
    void ThrowTargetServerRpc()
    {
        if (grabbedTargetId.Value == 0) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(grabbedTargetId.Value, out NetworkObject targetObj))
        {
            // 1. 물리 다시 켜기
            SetTargetKinematicClientRpc(targetObj.NetworkObjectId, false);

            // 2. 던지는 힘 가하기 (Rigidbody가 있다면)
            if (targetObj.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
            {
                Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
                targetRb.linearVelocity = throwDirection.normalized * skillData.excavatorThrowForce;
            }

            // 3. 스턴 걸기 (대상에게만 ClientRpc)
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { targetObj.OwnerClientId } }
            };
            ApplyStunClientRpc(skillData.excavatorStunDuration, clientRpcParams);
        }

        grabbedTargetId.Value = 0; // 잡기 해제
    }

    // --- 불도저 ---
    IEnumerator BulldozerSkillRoutine()
    {
        isShieldActive.Value = true;
        yield return new WaitForSeconds(skillData.bulldozerShieldDuration);
        isShieldActive.Value = false;
    }

    // 충돌 처리는 서버에서만 확실하게 계산
    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; // 서버만 처리

        if (isShieldActive.Value && skillData.vehicleType == VehicleType.Bulldozer)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<HeavyVehicleController>() != null)
            {
                if (collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody otherRb))
                {
                    // 내 속도 멈춤
                    rb.linearVelocity = Vector3.zero;
                    
                    // 상대 튕겨내기
                    Vector3 pushDir = collision.transform.position - transform.position;
                    pushDir.y = 0.5f; // 약간 위로
                    otherRb.AddForce(pushDir.normalized * 20f, ForceMode.Impulse); // 강제로 밀어냄
                }
            }
        }
    }

    // --- 덤프트럭 ---
    void DumpTruckSkill()
    {
        if (skillData.dirtProjectilePrefab == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, skillData.dumpTruckDetectionRadius);
        List<NetworkObject> targets = new List<NetworkObject>();

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if ((hit.CompareTag("Player") || hit.GetComponent<HeavyVehicleController>() != null) 
                && hit.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                targets.Add(netObj);
            }
        }

        Vector3 spawnPos = transform.position - transform.forward * 0.5f + Vector3.up * 1f;

        foreach (var target in targets)
        {
            // 1. 서버에서 프리팹 생성
            GameObject projectile = Instantiate(
                skillData.dirtProjectilePrefab,
                spawnPos,
                Quaternion.identity
            );

            // 2. 네트워크 스폰 (중요! 그래야 클라이언트에도 보임)
            NetworkObject projNetObj = projectile.GetComponent<NetworkObject>();
            projNetObj.Spawn();

            // 3. 투사체 초기화 (동기화 필요하므로 컴포넌트 함수 호출)
            // 투사체 스크립트도 NetworkBehaviour여야 함
            if (projectile.TryGetComponent<NetworkDirtProjectile>(out NetworkDirtProjectile dirtScript))
            {
                dirtScript.SetTarget(target.NetworkObjectId, skillData.dumpTruckProjectileSpeed);
            }
        }
    }

    // ==================== 클라이언트 로직 (ClientRpc) ====================

    [ClientRpc]
    void SetTargetKinematicClientRpc(ulong targetId, bool isKinematic)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject targetObj))
        {
            if (targetObj.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
            {
                targetRb.isKinematic = isKinematic;
            }
        }
    }

    [ClientRpc]
    void ApplyStunClientRpc(float duration, ClientRpcParams rpcParams = default)
    {
        // 이 함수는 스턴 당하는 대상 클라이언트에서만 실행됨
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        if (controller != null) controller.enabled = false;
        Debug.Log("스턴 걸림!");

        yield return new WaitForSeconds(duration);
        // 땅에 닿을 때까지 대기 (간단화)
        yield return new WaitForSeconds(0.5f); 

        if (controller != null) controller.enabled = true;
        Debug.Log("스턴 해제!");
    }

    // ==================== UI & Gizmos ====================
    
    void OnGUI()
    {
        if (!IsOwner || skillData == null) return; // 내 화면에만 그림

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        float yOffset = 330;

        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(5, yOffset, 400, 120), "");
        GUI.color = Color.white;

        GUI.Label(new Rect(10, yOffset + 5, 400, 30), $"스킬: {skillData.skillName}", style);

        if (currentCooldown > 0)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(10, yOffset + 35, 400, 30), $"쿨다운: {currentCooldown:F1}초", style);
        }
        else
        {
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(10, yOffset + 35, 400, 30), "스킬 사용 가능! [좌클릭]", style);
        }

        style.normal.textColor = Color.white;

        if (grabbedTargetId.Value != 0)
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, yOffset + 65, 400, 30), "잡는 중! [클릭하여 던지기]", style);
        }
        else if (isShieldActive.Value)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, yOffset + 65, 400, 30), "🛡️ 쉴드 활성화됨", style);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (skillData == null) return;
        // (기존 Gizmos 코드 유지)
    }
}
