using UnityEngine;
using Unity.Netcode;

public class SceneAudioInitializer : MonoBehaviour
{
    [SerializeField] private int bgmIndex = 0; // 이 씬에서 재생할 BGM 번호
    [SerializeField] private bool stopBgmOnStart = false; // BGM을 끄고 싶을 때 체크

    void Start()
    {
        // SoundManager가 있는지 확인 (싱글톤)
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager를 찾을 수 없습니다!");
            return;
        }

        // 서버 접속 여부와 상관없이 내 컴퓨터에서 즉시 재생
        if (stopBgmOnStart)
        {
            SoundManager.Instance.StopBgmLocal();
        }
        else
        {
            SoundManager.Instance.ChangeBgmLocal(bgmIndex);
        }
    }
}
