using UnityEngine;


public class RageGaugeManager : MonoBehaviour
{
    [Header("분노 게이지 설정")]
    [SerializeField] private float maxRageGauge = 100f;
    [SerializeField] private float ragePerPoint = 5f;
    
    [Header("각성 상태 설정")]
    [SerializeField] private float awakeningDuration = 20f;
    [SerializeField] private float awakeningSpeedMultiplier = 1.5f;
    
    private float currentRageGauge = 0f;
    private bool isAwakened = false;
    private float awakeningTimer = 0f;
    
    private HeavyVehicleController controller;
    private VehicleSkillManager skillManager;
    
    void Start()
    {
        controller = GetComponent<HeavyVehicleController>();
        skillManager = GetComponent<VehicleSkillManager>();
    }
    
    void Update()
    {
        if (isAwakened)
        {
            awakeningTimer -= Time.deltaTime;
            
            if (awakeningTimer <= 0)
            {
                EndAwakening();
            }
        }
        
        if (!isAwakened && currentRageGauge >= maxRageGauge)
        {
            StartAwakening();
        }
    }
    
    public void AddRage(float amount)
    {
        if (isAwakened) return; 
        
        currentRageGauge += amount;
        currentRageGauge = Mathf.Min(currentRageGauge, maxRageGauge);
        
        Debug.Log($"분노 게이지: {currentRageGauge}/{maxRageGauge}");
    }
    
    void StartAwakening()
    {
        isAwakened = true;
        awakeningTimer = awakeningDuration;
        currentRageGauge = maxRageGauge;
        
        Debug.Log("🔥 각성 상태 돌입! 🔥");
        
    }
    
    void EndAwakening()
    {
        isAwakened = false;
        currentRageGauge = 0f;
        
        Debug.Log("각성 상태 종료");
        
    }
    
    public bool IsAwakened()
    {
        return isAwakened;
    }
    
    public float GetSpeedMultiplier()
    {
        return isAwakened ? awakeningSpeedMultiplier : 1f;
    }
    
    public bool ShouldLockBoostGauge()
    {
        return isAwakened;
    }
    
    public float GetRagePercent()
    {
        return currentRageGauge / maxRageGauge;
    }
    
    public float GetAwakeningRemainingTime()
    {
        return isAwakened ? awakeningTimer : 0f;
    }
    
}
