using UnityEngine;
using UnityEngine.UI; // Image 컴ポ넌트 사용을 위해 필수
using TMPro;
using Unity.Netcode;

public class PlayerBoostUI : MonoBehaviour
{
    [Header("UI 연결")]
    // 💡 Slider 대신 Image를 가져옵니다.
    [SerializeField] private Image boostFillImage;

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
        // 💡 Image 컴포넌트가 연결되어 있는지 확인
        if (localPlayer == null || boostFillImage == null) return;

        // 3. fillAmount 계산 (0.0 ~ 1.0)
        // PlayerMove의 maxBoostGauge는 public이어야 합니다.
        // 안전을 위해 0으로 나누는 것을 방지합니다.
        float maxGauge = localPlayer.maxBoostGauge > 0 ? localPlayer.maxBoostGauge : 100f;
        float currentGauge = localPlayer.currentBoostGauge;

        // 💡 Mathf.Clamp01을 사용해 0~1 범위를 강제합니다.
        float fillValue = Mathf.Clamp01(currentGauge / maxGauge);
        
        // 💡 핵심: Image의 fillAmount에 할당
        boostFillImage.fillAmount = fillValue;
    }
}
