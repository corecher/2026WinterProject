using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    [Header("사운드 리소스")]
    public List<AudioClip> bgmClips;  // 배경음 리스트
    public List<AudioClip> sfxClips;  // 3D 효과음 리스트

    private AudioSource bgmSource;
    [Header("믹서 설정")]
    public AudioMixer mainMixer;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 1. AudioSource 생성
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0;

            // 🔥 [추가] 생성된 AudioSource를 Mixer의 BGM 그룹에 연결
            if (mainMixer != null)
            {
                // Mixer에서 "BGM"이라는 이름의 그룹을 찾아옵니다.
                // (주의: Audio Mixer 창의 Group 이름과 정확히 일치해야 함)
                AudioMixerGroup[] groups = mainMixer.FindMatchingGroups("BGM");
                if (groups.Length > 0)
                {
                    bgmSource.outputAudioMixerGroup = groups[0];
                }
                else
                {
                    Debug.LogWarning("BGM 그룹을 찾을 수 없습니다! Mixer의 그룹 이름을 확인하세요.");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 2. PlaySfxLocal 부분도 수정 (PlayClipAtPoint는 믹서 연결이 안 됨)
    public void PlaySfxLocal(int index)
    {
        if (index < 0 || index >= sfxClips.Count) return;

        // PlayClipAtPoint는 새로운 객체를 생성해버려서 Mixer 연결이 불가능합니다.
        // 대신 임시 AudioSource를 만들어서 SFX 그룹에 연결해 재생해야 합니다.
        GameObject sfxObj = new GameObject("TempSFX");
        AudioSource source = sfxObj.AddComponent<AudioSource>();
        
        // SFX 믹서 그룹 연결
        if (mainMixer != null)
        {
            AudioMixerGroup[] groups = mainMixer.FindMatchingGroups("SFX");
            if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
        }

        source.clip = sfxClips[index];
        source.Play();
        
        // 재생 종료 후 파괴
        Destroy(sfxObj, sfxClips[index].length);
    }

    // ================= [ BGM 관리 (2D) ] =================

    public void ChangeBgm(int index)
    {
        if (!IsServer) return;
        ChangeBgmClientRpc(index);
    }

    [ClientRpc]
    private void ChangeBgmClientRpc(int index)
    {
        if (index < 0 || index >= bgmClips.Count) return;

        bgmSource.clip = bgmClips[index];
        bgmSource.Play();
        Debug.Log($"BGM 변경됨: {bgmClips[index].name}");
    }

    public void StopBgm()
    {
        if (!IsServer) return;
        StopBgmClientRpc();
    }

    [ClientRpc]
    private void StopBgmClientRpc() => bgmSource.Stop();


    // ================= [ SFX 관리 (3D) ] =================

    public void PlaySfx(int index, Vector3 position)
    {
        if (!IsServer) return;
        PlaySfxClientRpc(index, position);
    }

    [ClientRpc]
    private void PlaySfxClientRpc(int index, Vector3 position)
    {
        if (index < 0 || index >= sfxClips.Count) return;

        // 3D 공간의 해당 위치에서 소리 재생
        AudioSource.PlayClipAtPoint(sfxClips[index], position, 1.0f);
    }
    public void ChangeBgmLocal(int index)
    {
        // 인덱스 범위 확인
        if (index < 0 || index >= bgmClips.Count) return;

        // 이미 같은 곡이 재생 중이면 중복 재생 방지
        if (bgmSource.clip == bgmClips[index] && bgmSource.isPlaying) return;

        bgmSource.clip = bgmClips[index];
        bgmSource.Play();
        Debug.Log($"[Local] BGM 재생 시작: {bgmClips[index].name}");
    }

    public void StopBgmLocal()
    {
        if (bgmSource != null) bgmSource.Stop();
    }
    

    // 슬라이더에서 호출할 함수 (0.0001 ~ 1 사이의 값을 받음)
    public void SetVolume(string parameterName, float sliderValue)
    {
        // 로그(Log) 스케일을 사용하여 자연스러운 음량 변화 구현
        // -80dB(무음) ~ 20dB(최대) 사이로 변환
        float volume = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
        mainMixer.SetFloat(parameterName, volume);
    }
}
