using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetInfo(ulong id, string name)
    {
        // 예: "Player 1 (Me)"
        string displayName = $"Player {id}";
        
        // (선택사항) 이름 뒤에 닉네임 붙이기
        if (!string.IsNullOrEmpty(name))
        {
            displayName += $" ({name})";
        }

        nameText.text = displayName;
    }
}
