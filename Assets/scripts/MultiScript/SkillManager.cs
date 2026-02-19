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
    private HeavyVehicleController controller; // 기존 차량 이동 스크립트

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<HeavyVehicleController>();
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
            transform.position + transform.forward * skillData.excavatorGrabRange * 0.5f,
            skillData.excavatorGrabRange * 0.5f
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            if (hit.TryGetComponent<NetworkObject>(out var netObj))
            {
                if (hit.CompareTag("Player") || hit.GetComponent<HeavyVehicleController>() != null)
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
            if (targetNetObj.TryGetComponent<Rigidbody>(out var targetRb))
            {
                targetRb.isKinematic = false;
                Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
                targetRb.linearVelocity = throwDirection.normalized * skillData.excavatorThrowForce;
                
                if (targetNetObj.TryGetComponent<SkillManager>(out var targetSkill))
                {
                    targetSkill.ApplyStunClientRpc(skillData.excavatorStunDuration);
                }
            }
        }
        n_isGrabbing.Value = false;
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
