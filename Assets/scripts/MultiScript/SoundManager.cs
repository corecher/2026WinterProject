using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    [Header("사운드 리소스")]
    public List<AudioClip> bgmClips;  // 배경음 리스트
    public List<AudioClip> sfxClips;  // 3D 효과음 리스트

    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0; // BGM은 2D(전체 화면)로 설정
        }
        else
        {
            Destroy(gameObject);
        }
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
}
