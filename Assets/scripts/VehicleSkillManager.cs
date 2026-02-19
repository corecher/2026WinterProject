using UnityEngine;


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
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<HeavyVehicleController>();
    }
    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
        
        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
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
        
        currentCooldown = skillData.cooldownTime;
    }
    // ==================== 포크레인 스킬 ====================
    void UseExcavatorSkill()
    {
        Debug.Log("포크레인 스킬 발동!");
        
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
            targetRb.linearVelocity = throwDirection.normalized * skillData.excavatorThrowForce;
            
            Debug.Log($"플레이어 던짐: {grabbedPlayer.name}");
            
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
        if (isGrabbing && grabbedPlayer != null)
        {
            grabbedPlayer.transform.position = transform.position + transform.forward * 2f + Vector3.up * 2f;
        }
    }
    public void ApplyStun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }
    System.Collections.IEnumerator StunCoroutine(float duration)
    {
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        Debug.Log($"{gameObject.name} 스턴 - {duration}초");
        yield return new WaitForSeconds(duration);
        
        yield return new WaitUntil(() => IsGrounded());
        
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
        if (hasShield && skillData.vehicleType == VehicleType.Bulldozer)
        {
            if (collision.gameObject.CompareTag("Player") || 
                collision.gameObject.GetComponent<HeavyVehicleController>() != null)
            {
                Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    Vector3 impactVelocity = collision.relativeVelocity;
                    
                    rb.linearVelocity = Vector3.zero;
                    
                    otherRb.linearVelocity += impactVelocity;
                    
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
        
        Collider[] hits = Physics.OverlapSphere(transform.position, skillData.dumpTruckDetectionRadius);
        
        System.Collections.Generic.List<GameObject> targets = new System.Collections.Generic.List<GameObject>();
        
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            if (hit.CompareTag("Player") || hit.GetComponent<HeavyVehicleController>() != null)
            {
                targets.Add(hit.gameObject);
            }
        }
        
        Debug.Log($"감지된 플레이어 수: {targets.Count}");

        Vector3 spawnPosition = transform.position 
            - transform.forward * 0.5f  
            + Vector3.up * 1f;   
        
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
    // 야메 GUI 표시
    void OnGUI()
    {
        if (skillData == null) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        
        float yOffset = 330; // HeavyVehicleController UI 아래쪽
        
        // 배경
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(5, yOffset, 400, 120), "");
        GUI.color = Color.white;
        
        // 스킬 정보
        GUI.Label(new Rect(10, yOffset + 5, 400, 30), $"스킬: {skillData.skillName}", style);
        
        // 쿨다운 표시
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