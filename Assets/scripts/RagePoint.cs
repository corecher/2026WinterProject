using UnityEngine;

// ==================== 분노 포인트 아이템 ====================
public class RagePoint : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // 회전
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // 상하 움직임
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 플레이어가 먹었을 때
        RageGaugeManager rageManager = other.GetComponent<RageGaugeManager>();
        if (rageManager != null)
        {
            rageManager.AddRage(5f); // 5% 증가
            Destroy(gameObject);
            Debug.Log($"{other.name}이(가) 분노 포인트 획득!");
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}

// ==================== 분노 게이지 매니저 ====================
public class RageGaugeManager : MonoBehaviour
{
    [Header("분노 게이지 설정")]
    [SerializeField] private float maxRageGauge = 100f;
    [SerializeField] private float ragePerPoint = 5f;
    
    [Header("각성 상태 설정")]
    [SerializeField] private float awakeningDuration = 20f;
    [SerializeField] private float awakeningSpeedMultiplier = 1.5f;
    
    // 내부 변수
    private float currentRageGauge = 0f;
    private bool isAwakened = false;
    private float awakeningTimer = 0f;
    
    // 컴포넌트 참조
    private HeavyVehicleController controller;
    private VehicleSkillManager skillManager;
    
    void Start()
    {
        controller = GetComponent<HeavyVehicleController>();
        skillManager = GetComponent<VehicleSkillManager>();
    }
    
    void Update()
    {
        // 각성 상태 타이머
        if (isAwakened)
        {
            awakeningTimer -= Time.deltaTime;
            
            if (awakeningTimer <= 0)
            {
                EndAwakening();
            }
        }
        
        // 각성 상태가 아닐 때 게이지가 가득 차면 각성 발동
        if (!isAwakened && currentRageGauge >= maxRageGauge)
        {
            StartAwakening();
        }
    }
    
    // 분노 포인트 획득
    public void AddRage(float amount)
    {
        if (isAwakened) return; // 각성 중에는 획득 불가
        
        currentRageGauge += amount;
        currentRageGauge = Mathf.Min(currentRageGauge, maxRageGauge);
        
        Debug.Log($"분노 게이지: {currentRageGauge}/{maxRageGauge}");
    }
    
    // 각성 시작
    void StartAwakening()
    {
        isAwakened = true;
        awakeningTimer = awakeningDuration;
        currentRageGauge = maxRageGauge;
        
        Debug.Log("🔥 각성 상태 돌입! 🔥");
        
        // TODO: 각성 이펙트, 사운드 등
    }
    
    // 각성 종료
    void EndAwakening()
    {
        isAwakened = false;
        currentRageGauge = 0f;
        
        Debug.Log("각성 상태 종료");
        
        // TODO: 각성 종료 이펙트
    }
    
    // 각성 상태 확인
    public bool IsAwakened()
    {
        return isAwakened;
    }
    
    // 각성 상태에서 속도 배율 가져오기
    public float GetSpeedMultiplier()
    {
        return isAwakened ? awakeningSpeedMultiplier : 1f;
    }
    
    // 각성 상태에서 부스트 게이지 고정 여부
    public bool ShouldLockBoostGauge()
    {
        return isAwakened;
    }
    
    // 분노 게이지 퍼센트
    public float GetRagePercent()
    {
        return currentRageGauge / maxRageGauge;
    }
    
    // GUI 표시
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        
        float yOffset = 460; // 스킬 UI 아래
        
        // 배경
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(5, yOffset, 550, 100), "");
        GUI.color = Color.white;
        
        if (isAwakened)
        {
            // 각성 상태 표시
            style.normal.textColor = Color.red;
            style.fontSize = 24;
            GUI.Label(new Rect(10, yOffset + 5, 550, 35), $"🔥🔥 각성 상태! 남은 시간: {awakeningTimer:F1}초 🔥🔥", style);
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, yOffset + 40, 550, 30), "부스트 무제한 | 속도 1.5배 | 스킬 사용 가능!", style);
        }
        else
        {
            // 분노 게이지 표시
            GUI.Label(new Rect(10, yOffset + 5, 550, 30), $"분노 게이지: {currentRageGauge:F0}%", style);
            
            // 게이지 바 배경
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            GUI.Box(new Rect(10, yOffset + 35, 530, 25), "");
            
            // 게이지 바 (빨간색)
            float gaugePercent = currentRageGauge / maxRageGauge;
            GUI.color = new Color(1f, 0f, 0f, 0.8f);
            GUI.Box(new Rect(10, yOffset + 35, 530 * gaugePercent, 25), "");
            GUI.color = Color.white;
            
            style.fontSize = 16;
            style.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            GUI.Label(new Rect(10, yOffset + 65, 550, 25), "분노 포인트를 먹어 게이지를 채우세요!", style);
        }
    }
}