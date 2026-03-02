public class VehicleSkillManager : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private VehicleSkillData skillData;
    
    private float currentCooldown = 0f;
    private bool isSkillActive = false;
    
    private GameObject grabbedPlayer;
    private bool isGrabbing = false;
    
    private float shieldTimer = 0f;
    private bool hasShield = false;
    
    private Rigidbody rb;
    private HeavyVehicleController controller;
    private RageGaugeManager rageManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<HeavyVehicleController>();
        rageManager = GetComponent<RageGaugeManager>();
    }
    
    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
        
        bool isAwakened = rageManager != null && rageManager.IsAwakened();
        
        if (isAwakened && Input.GetMouseButtonDown(0) && currentCooldown <= 0)
        {
            UseSkill();
        }
        
        if (isGrabbing && grabbedPlayer != null && Input.GetMouseButtonDown(0))
        {
            ThrowGrabbedPlayer();
        }
        
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0)
            {
                hasShield = false;
                Debug.Log("불도저 쉴드 종료!");
            }
        }
    }
    
    void UseSkill()
    {
        if (skillData == null) return;
        
        switch (skillData.vehicleType)
        {
            case VehicleType.Excavator:
                UseExcavatorSkill();
                break;
            case VehicleType.Bulldozer:
                UseBuldozerSkill();
                break;
            case VehicleType.DumpTruck:
                UseDumpTruckSkill();
                break;
        }
        
        // 쿨다운 시작
        currentCooldown = skillData.cooldownTime;
    }
    
    // ==================== 포크레인 스킬 ====================
    void UseExcavatorSkill()
    {
        Debug.Log("포크레인 스킬 발동!");
        
        // 애니메이션 딜레이 후 실행
        Invoke(nameof(ExcavatorGrab), skillData.excavatorAnimationDelay);
    }
    
    void ExcavatorGrab()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * skillData.excavatorGrabRange * 0.5f,
            skillData.excavatorGrabRange * 0.5f
        );
        
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            if (hit.CompareTag("Player") || hit.GetComponent<HeavyVehicleController>() != null)
            {
                grabbedPlayer = hit.gameObject;
                isGrabbing = true;
                
                Rigidbody targetRb = grabbedPlayer.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    targetRb.isKinematic = true;
                }
                
                Debug.Log($"플레이어 붙잡음: {grabbedPlayer.name}");
                return;
            }
        }
        
        Debug.Log("포크레인 스킬 실패 - 플레이어 없음");
    }
    
    void ThrowGrabbedPlayer()
    {
        if (grabbedPlayer == null) return;
        
        Rigidbody targetRb = grabbedPlayer.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.isKinematic = false;
            
            Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
            targetRb.velocity = throwDirection.normalized * skillData.excavatorThrowForce;
            
            Debug.Log($"플레이어 던짐: {grabbedPlayer.name}");
            
            // 던져진 플레이어 스턴 적용
            VehicleSkillManager targetSkill = grabbedPlayer.GetComponent<VehicleSkillManager>();
            if (targetSkill != null)
            {
                targetSkill.ApplyStun(skillData.excavatorStunDuration);
            }
        }
        
        grabbedPlayer = null;
        isGrabbing = false;
    }
    
    void LateUpdate()
    {
        // 포크레인이 플레이어를 붙잡고 있으면 따라오게
        if (isGrabbing && grabbedPlayer != null)
        {
            grabbedPlayer.transform.position = transform.position + transform.forward * 2f + Vector3.up * 2f;
        }
    }
    
    // 스턴 적용
    public void ApplyStun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }
    
    System.Collections.IEnumerator StunCoroutine(float duration)
    {
        // 컨트롤러 비활성화
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        Debug.Log($"{gameObject.name} 스턴 - {duration}초");
        yield return new WaitForSeconds(duration);
        
        // 지면에 닿았는지 확인 (추가 체크)
        yield return new WaitUntil(() => IsGrounded());
        
        // 컨트롤러 재활성화
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        Debug.Log($"{gameObject.name} 스턴 해제");
    }
    
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.5f);
    }
    
    // ==================== 불도저 스킬 ====================
    void UseBuldozerSkill()
    {
        Debug.Log("불도저 쉴드 발동!");
        hasShield = true;
        shieldTimer = skillData.bulldozerShieldDuration;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 불도저 쉴드 효과
        if (hasShield && skillData.vehicleType == VehicleType.Bulldozer)
        {
            // 다른 플레이어와 충돌
            if (collision.gameObject.CompareTag("Player") || 
                collision.gameObject.GetComponent<HeavyVehicleController>() != null)
            {
                Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    // 자신이 받아야 할 충격량 계산
                    Vector3 impactVelocity = collision.relativeVelocity;
                    
                    // 자신은 날아가지 않음
                    rb.velocity = Vector3.zero;
                    
                    // 상대에게 충격량 부여
                    otherRb.velocity += impactVelocity;
                    
                    Debug.Log($"불도저 쉴드 효과! 상대 밀어냄: {collision.gameObject.name}");
                }
            }
        }
    }
    
    // ==================== 덤프트럭 스킬 ====================
    void UseDumpTruckSkill()
    {
        Debug.Log("덤프트럭 스킬 발동!");
        
        if (skillData.dirtProjectilePrefab == null)
        {
            Debug.LogError("흙 투사체 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        // 범위 내 모든 플레이어 감지
        Collider[] hits = Physics.OverlapSphere(transform.position, skillData.dumpTruckDetectionRadius);
        
        System.Collections.Generic.List<GameObject> targets = new System.Collections.Generic.List<GameObject>();
        
        foreach (var hit in hits)
        {
            // 자기 자신 제외
            if (hit.gameObject == gameObject) continue;
            
            // 플레이어만 추가
            if (hit.CompareTag("Player") || hit.GetComponent<HeavyVehicleController>() != null)
            {
                targets.Add(hit.gameObject);
            }
        }
        
        Debug.Log($"감지된 플레이어 수: {targets.Count}");
        
        // 덤프트럭 뒤쪽 위 위치 계산
        Vector3 spawnPosition = transform.position 
            - transform.forward * 2f  // 뒤쪽으로 2미터
            + Vector3.up * 3f;        // 위로 3미터
        
        // 각 타겟마다 투사체 생성
        foreach (var target in targets)
        {
            GameObject projectile = Instantiate(
                skillData.dirtProjectilePrefab,
                spawnPosition,
                Quaternion.identity
            );
            
            DirtProjectile dirtScript = projectile.GetComponent<DirtProjectile>();
            if (dirtScript == null)
            {
                dirtScript = projectile.AddComponent<DirtProjectile>();
            }
            
            dirtScript.Initialize(target, skillData.dumpTruckProjectileSpeed, skillData.dumpTruckSlowPercent);
        }
    }
    
    // GUI 표시
    void OnGUI()
    {
        if (skillData == null) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        
        float yOffset = 330; // HeavyVehicleController UI 아래쪽
        
        // 각성 상태 확인
        bool isAwakened = rageManager != null && rageManager.IsAwakened();
        
        // 각성 상태가 아니면 스킬 UI 표시 안 함
        if (!isAwakened) return;
        
        // 배경
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(5, yOffset, 400, 120), "");
        GUI.color = Color.white;
        
        // 스킬 정보
        style.normal.textColor = Color.red;
        GUI.Label(new Rect(10, yOffset + 5, 400, 30), $"⚡ 스킬: {skillData.skillName}", style);
        
        style.normal.textColor = Color.white;
        
        // 쿨다운 표시
        if (currentCooldown > 0)
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, yOffset + 35, 400, 30), $"쿨다운: {currentCooldown:F1}초", style);
        }
        else
        {
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(10, yOffset + 35, 400, 30), "스킬 사용 가능! [좌클릭]", style);
        }
        
        style.normal.textColor = Color.white;
        
        // 추가 상태 표시
        if (isGrabbing && grabbedPlayer != null)
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, yOffset + 65, 400, 30), "플레이어 붙잡는 중! [좌클릭으로 던지기]", style);
        }
        else if (hasShield)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, yOffset + 65, 400, 30), $"🛡️ 쉴드 활성화: {shieldTimer:F1}초", style);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (skillData == null) return;
        
        // 스킬 범위 시각화
        switch (skillData.vehicleType)
        {
            case VehicleType.Excavator:
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(
                    transform.position + transform.forward * skillData.excavatorGrabRange * 0.5f,
                    skillData.excavatorGrabRange * 0.5f
                );
                break;
            case VehicleType.DumpTruck:
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, skillData.dumpTruckDetectionRadius);
                break;
        }
    }
}