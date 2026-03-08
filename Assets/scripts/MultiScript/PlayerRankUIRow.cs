using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerRankUIRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText; // (선택) 숫자 점수 표시
    [SerializeField] private Image gaugeFillImage; // 채워지는 게이지 바

    public void Setup(string playerName, int score, float fillAmount)
    {
        if (nameText != null) nameText.text = playerName;
        if (scoreText != null) scoreText.text = $"{score} 점";
        
        // 게이지 바 채우기 (0.0 ~ 1.0)
        if (gaugeFillImage != null) gaugeFillImage.fillAmount = fillAmount;
    }
}
