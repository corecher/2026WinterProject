using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkGameTimer : NetworkBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 설정
    public static NetworkGameTimer Instance { get; private set; }

    [Header("UI 연결")]
    public TMP_Text Timet;

    [Header("설정")]
    public int starttime = 3;

    private NetworkVariable<int> netCurrentTime = new NetworkVariable<int>(
        -1, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    // 플레이어가 움직일 수 있는지 확인하는 프로퍼티
    // 값이 0이거나 -99(게임 중)일 때만 true를 반환합니다.
    public bool CanMove => netCurrentTime.Value == 0 || netCurrentTime.Value == -99;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (Timet != null) Timet.text = "다른 플레이어 대기 중...";
    }

    public override void OnNetworkSpawn()
    {
        netCurrentTime.OnValueChanged += OnTimeValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnAllPlayersLoaded;
        }
        
        UpdateUI(netCurrentTime.Value);
    }

    public override void OnNetworkDespawn()
    {
        netCurrentTime.OnValueChanged -= OnTimeValueChanged;
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnAllPlayersLoaded;
        }
    }

    private void OnAllPlayersLoaded(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (sceneName == SceneManager.GetActiveScene().name)
        {
            StartCoroutine(ServerCountdown());
        }
    }

    IEnumerator ServerCountdown()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        int current = starttime;
        while (current > 0)
        {
            netCurrentTime.Value = current; 
            yield return new WaitForSecondsRealtime(1f);
            current--;
        }

        // GO! (이제 CanMove가 true가 됩니다)
        netCurrentTime.Value = 0;
        
        yield return new WaitForSecondsRealtime(1.5f);

        netCurrentTime.Value = -99;
    }

    private void OnTimeValueChanged(int previousValue, int newValue)
    {
        UpdateUI(newValue);
    }

    private void UpdateUI(int timeValue)
    {
        if (Timet == null) return;

        if (timeValue > 0) Timet.text = timeValue.ToString();
        else if (timeValue == 0) Timet.text = "GO!";
        else if (timeValue == -99) Timet.text = "";
        else Timet.text = "Waiting...";
    }
}
