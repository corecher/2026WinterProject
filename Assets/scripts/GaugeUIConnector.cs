using UnityEngine;
using UnityEngine.UI;

// 부스트 게이지와 분노 게이지를 UI에 연결하는 스크립트
public class GaugeUIConnector : MonoBehaviour
{
    [Header("UI 참조 - Inspector에서 연결")]
    [SerializeField] private Image boostGaugeFillImage;    
    [SerializeField] private Image rageGaugeFillImage;     
    
    [SerializeField] private Text boostGaugeText;          
    [SerializeField] private Text rageGaugeText;           
    [SerializeField] private Text awakeningStatusText;     
    
    [Header("게이지 색상 설정")]
    [SerializeField] private bool useColorGradient = true;
    [SerializeField] private Color boostEmptyColor = new Color(0.3f, 0.3f, 0.3f);  
    [SerializeField] private Color boostFullColor = Color.cyan;                     
    [SerializeField] private Color rageEmptyColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color rageFullColor = Color.red;
    [SerializeField] private Color awakeningColor = Color.yellow;                  
    
    private HeavyVehicleController vehicleController;
    private RageGaugeManager rageManager;
    
    void Start()
    {
        vehicleController = GetComponent<HeavyVehicleController>();
        rageManager = GetComponent<RageGaugeManager>();
        
        if (boostGaugeFillImage == null)
            Debug.LogWarning("부스트 게이지 Image가 연결되지 않았습니다!");
        if (rageGaugeFillImage == null)
            Debug.LogWarning("분노 게이지 Image가 연결되지 않았습니다!");
    }
    
    void Update()
    {
        UpdateBoostGaugeUI();
        UpdateRageGaugeUI();
        UpdateAwakeningStatusUI();
    }
    
    void UpdateBoostGaugeUI()
    {
        if (vehicleController == null || boostGaugeFillImage == null) return;
        
        float boostPercent = vehicleController.GetBoostPercent();
        
        boostGaugeFillImage.fillAmount = boostPercent;
        
        if (useColorGradient)
        {
            bool isAwakened = rageManager != null && rageManager.IsAwakened();
            
            if (isAwakened)
            {
                boostGaugeFillImage.color = awakeningColor;
            }
            else
            {
                boostGaugeFillImage.color = Color.Lerp(boostEmptyColor, boostFullColor, boostPercent);
            }
        }
        
        if (boostGaugeText != null)
        {
            boostGaugeText.text = $"{(boostPercent * 100):F0}%";
        }
    }
    
    void UpdateRageGaugeUI()
    {
        if (rageManager == null || rageGaugeFillImage == null) return;
        
        float ragePercent = rageManager.GetRagePercent();
        
        rageGaugeFillImage.fillAmount = ragePercent;
        
        if (useColorGradient)
        {
            rageGaugeFillImage.color = Color.Lerp(rageEmptyColor, rageFullColor, ragePercent);
        }
        
        if (rageGaugeText != null)
        {
            rageGaugeText.text = $"{(ragePercent * 100):F0}%";
        }
    }
    
    void UpdateAwakeningStatusUI()
    {
        if (rageManager == null || awakeningStatusText == null) return;
        
        bool isAwakened = rageManager.IsAwakened();
        
        if (isAwakened)
        {
            float remainingTime = rageManager.GetAwakeningRemainingTime();
            awakeningStatusText.text = $"🔥 각성! {remainingTime:F1}초";
            awakeningStatusText.color = Color.red;
        }
        else
        {
            awakeningStatusText.text = "";
        }
    }
}

