using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using Unity.Netcode;

public class PlayerBoostUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image boostFillImage;
    // 💡 분노 게이지 UI 연결을 위한 변수 추가
    [SerializeField] private Image rageFillImage; 

    private PlayerMove localPlayer;

    void Update()
    {
        // 1. 로컬 플레이어 참조가 아직 없다면 찾기
        if (localPlayer == null)
        {
            FindLocalPlayer();
            return; 
        }

        // 2. UI 업데이트
        UpdateUI();
    }

    private void FindLocalPlayer()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (playerObject != null)
            {
                localPlayer = playerObject.GetComponent<PlayerMove>();
            }
        }
    }

    private void UpdateUI()
    {
        if (localPlayer == null) return;

        // ==========================================
        // 1. 부스트 게이지 업데이트
        // ==========================================
        if (boostFillImage != null)
        {
            float maxBoost = localPlayer.maxBoostGauge > 0 ? localPlayer.maxBoostGauge : 100f;
            float currentBoost = localPlayer.currentBoostGauge;
            
            boostFillImage.fillAmount = Mathf.Clamp01(currentBoost / maxBoost);
        }

        // ==========================================
        // 2. 💡 분노 게이지 업데이트
        // ==========================================
        if (rageFillImage != null)
        {
            float maxRage = localPlayer.maxRageGauge > 0 ? localPlayer.maxRageGauge : 100f;
            float currentRage = localPlayer.currentRageGauge;
            
            rageFillImage.fillAmount = Mathf.Clamp01(currentRage / maxRage);
            if (localPlayer.isAwakened)
            {
                rageFillImage.color = Color.red; // 각성 시 붉은색
            }
            else
            {
                rageFillImage.color = Color.yellow; // 평상시 노란색
            }
        }
    }
}
