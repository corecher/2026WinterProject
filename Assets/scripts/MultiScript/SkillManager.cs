using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SkillManager : NetworkBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private VehicleSkillData skillData;
    
    // 네트워크 동기화 변수들
    public NetworkVariable<ChrState> currentVehicleType = new NetworkVariable<ChrState>(ChrState.excavator, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> n_isGrabbing = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> n_hasShield = new NetworkVariable<bool>(false);
    private NetworkVariable<NetworkObjectReference> n_grabbedPlayerRef = new NetworkVariable<NetworkObjectReference>();

    public float CurrentCooldown { get; private set; } = 0f;
    private Rigidbody rb;
    private PlayerMove controller; // 기존 차량 이동 스크립트

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerMove>();
    }

    void Update()
    {
        // 내 로컬 플레이어만 입력을 처리함
        if (!IsOwner) return;

        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= Time.deltaTime;
        }

        // 스킬 사용 입력 (마우스 좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            if (n_isGrabbing.Value)
            {
                // 누군가 잡고 있다면 던지기 실행
                ThrowGrabbedPlayerServerRpc();
            }
            else if (CurrentCooldown <= 0)
            {
                // 쿨타임이 끝났다면 스킬 사용 요청
                RequestUseSkillServerRpc();
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (skillData == null) return;
    
        // 포크레인 잡기 범위 시각화 (빨간색)
        Gizmos.color = Color.red;
        Vector3 grabPos = transform.position + -transform.forward * skillData.excavatorGrabRange;
        Gizmos.DrawWireSphere(grabPos, skillData.excavatorGrabRange * 0.5f);
    }
    [ServerRpc]
    private void RequestUseSkillServerRpc()
    {
        if (skillData == null) return;

        switch (currentVehicleType.Value)
        {
            case ChrState.excavator: UseExcavatorSkill(); break;
            case ChrState.bulldozer: UseBulldozerSkill(); break;
            case ChrState.dtruck:    UseDumpTruckSkill(); break;
        }

        // 쿨다운 시작 알림 (ClientRpc)
        SetCooldownClientRpc(skillData.cooldownTime);
    }

    [ClientRpc]
    private void SetCooldownClientRpc(float time)
    {
        if (IsOwner) CurrentCooldown = time;
    }

    [ServerRpc]
    public void UpdateVehicleTypeServerRpc(ChrState newState)
    {
        currentVehicleType.Value = newState;
        Debug.Log($"[서버] 플레이어 클래스 변경: {newState}");
    }

    #region 포크레인 (Excavator)
    private void UseExcavatorSkill()
    {
        StartCoroutine(ExcavatorGrabRoutine());
    }

    IEnumerator ExcavatorGrabRoutine()
    {
        yield return new WaitForSeconds(skillData.excavatorAnimationDelay);

        Collider[] hits = Physics.OverlapSphere(
            transform.position + -transform.forward * skillData.excavatorGrabRange,
            skillData.excavatorGrabRange*0.5f
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            if (hit.TryGetComponent<NetworkObject>(out var netObj))
            {
                if (hit.CompareTag("Player") || hit.GetComponent<PlayerMove>() != null)
                {
                    n_grabbedPlayerRef.Value = netObj;
                    n_isGrabbing.Value = true;
                    
                    if (hit.TryGetComponent<Rigidbody>(out var targetRb))
                    {
                        targetRb.isKinematic = true;
                    }
                    yield break; // 성공적으로 잡았으므로 코루틴 종료
                }
            }
        }
    }

    [ServerRpc]
    private void ThrowGrabbedPlayerServerRpc()
    {
        if (!n_isGrabbing.Value) return;

        if (n_grabbedPlayerRef.Value.TryGet(out NetworkObject targetNetObj))
        {
            // 1. 서버에서 잡기 상태 해제 (LateUpdate 위치 고정 중단)
            n_isGrabbing.Value = false;

            if (targetNetObj.TryGetComponent<SkillManager>(out var targetSkill))
            {
                // 2. 타겟 플레이어(Owner)에게 직접 던져지라고 명령
                Vector3 throwDir = -(transform.forward + Vector3.up * 0.5f).normalized;
                float force = skillData.excavatorThrowForce;
                
                targetSkill.ApplyThrowClientRpc(throwDir * force, skillData.excavatorStunDuration);
            }
        }
    }

    // targetSkillManager 내부에 추가
    [ClientRpc]
    public void ApplyThrowClientRpc(Vector3 velocity, float stunDuration)
    {
        if (!IsOwner) return; // 오직 던져지는 당사자만 실행

        // 물리 복구 및 힘 가하기
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocity; // 직접 속도 주입
        }

        // 스턴 코루틴 실행
        StartCoroutine(StunCoroutine(stunDuration));
    }
    #endregion

    #region 불도저 (Bulldozer)
    private void UseBulldozerSkill()
    {
        n_hasShield.Value = true;
        Invoke(nameof(DisableShield), skillData.bulldozerShieldDuration);
    }

    private void DisableShield() => n_hasShield.Value = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (n_hasShield.Value && currentVehicleType.Value == ChrState.bulldozer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collision.gameObject.TryGetComponent<Rigidbody>(out var otherRb))
                {
                    rb.linearVelocity = Vector3.zero;
                    otherRb.linearVelocity += collision.relativeVelocity;
                }
            }
        }
    }
    #endregion

    #region 덤프트럭 (DumpTruck)
    private void UseDumpTruckSkill()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, skillData.dumpTruckDetectionRadius);
        Vector3 spawnPosition = transform.position - transform.forward * 0.5f + Vector3.up * 1f;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.CompareTag("Player"))
            {
                GameObject projectile = Instantiate(skillData.dirtProjectilePrefab, spawnPosition, Quaternion.identity);
                projectile.GetComponent<NetworkObject>().Spawn();
                
                if (projectile.TryGetComponent<DirtProjectile>(out var dirtScript))
                {
                    dirtScript.Initialize(hit.gameObject, skillData.dumpTruckProjectileSpeed, skillData.dumpTruckSlowPercent);
                }
            }
        }
    }
    #endregion

    [ClientRpc]
    public void ApplyStunClientRpc(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        if (controller != null) controller.enabled = false;
        yield return new WaitForSeconds(duration);
        yield return new WaitUntil(() => IsGrounded());
        if (controller != null) controller.enabled = true;
    }

    private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, 1.5f);

    void LateUpdate()
    {
        // 잡혀있는 플레이어 위치를 동기화 (서버에서 계산)
        if (IsServer && n_isGrabbing.Value)
        {
            if (n_grabbedPlayerRef.Value.TryGet(out NetworkObject target))
            {
                target.transform.position = transform.position + transform.forward * 2f + Vector3.up * 2f;
            }
        }
    }
}
