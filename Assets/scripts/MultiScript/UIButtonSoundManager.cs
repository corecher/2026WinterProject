using UnityEngine;
using UnityEngine.UI;

public class UIButtonSoundManager : MonoBehaviour
{
    [SerializeField] private int clickSoundIndex = 0; // sfxClips의 인덱스 번호

    void Start()
    {
        // 1. 현재 씬에 있는 모든 Button 컴포넌트를 찾습니다.
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button btn in allButtons)
        {
            // 2. 버튼이 눌릴 때 SoundManager의 로컬 재생 기능을 호출하도록 이벤트 추가
            btn.onClick.AddListener(() => {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySfxLocal(clickSoundIndex);
                }
            });
        }
    }
}
