using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        // 슬라이더 범위는 0.0001 ~ 1로 설정하는 것이 좋습니다.
        masterSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetVolume("MasterVol", val));
        bgmSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetVolume("BGMVol", val));
        sfxSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetVolume("SFXVol", val));
        
        // 초기 값 설정 (예: 0.75f)
        masterSlider.value = 0.75f;
        bgmSlider.value = 0.75f;
        sfxSlider.value = 0.75f;
    }
}
