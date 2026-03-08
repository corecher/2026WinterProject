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
    
    [Header("사운드 설정")]
    [Tooltip("카운트다운(3,2,1)과 GO(0) 시 재생할 SoundManager의 SFX 인덱스")]
    [SerializeField] private int countdownSfxIndex = 1;

    [Header("시작 위치")]
    [SerializeField] private Vector3 startPosition = new Vector3(0, 1, 0);

    // 네트워크 변수: 서버가 값을 정하고 모든 클라이언트가 읽음
    private NetworkVariable<int> netCurrentTime = new NetworkVariable<int>(
        -1, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    // 플레이어가 움직일 수 있는지 확인하는 프로퍼티
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
        // 값이 변할 때마다 OnTimeValueChanged 함수를 실행하도록 이벤트 등록
        netCurrentTime.OnValueChanged += OnTimeValueChanged;

        if (IsServer)
        {
            // 모든 플레이어가 씬 로딩을 마쳤을 때 이벤트 등록
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
        // 현재 활성화된 씬에서 로드가 완료되었다면 카운트다운 시작
        if (sceneName == SceneManager.GetActiveScene().name)
        {
            StartCoroutine(ServerCountdown());
        }
    }

    IEnumerator ServerCountdown()
    {
        // 1. 모든 플레이어를 시작 위치로 이동
        TeleportAllPlayers();

        // 2. 동기화 대기 시간
        yield return new WaitForSecondsRealtime(1.0f);

        // 3. 카운트다운 진행 (starttime부터 1까지)
        int current = starttime;
        while (current > 0)
        {
            netCurrentTime.Value = current; 
            yield return new WaitForSecondsRealtime(1f);
            current--;
        }

        // 4. GO! (0)
        netCurrentTime.Value = 0;
        
        // 5. 잠시 후 UI 숨기기 (-99)
        yield return new WaitForSecondsRealtime(1.5f);
        netCurrentTime.Value = -99;
    }

    private void OnTimeValueChanged(int previousValue, int newValue)
    {
        // UI 텍스트 업데이트
        UpdateUI(newValue);

        // 💡 [사운드 재생] 3, 2, 1, 0일 때 모두 동일한 소리 재생
        if (SoundManager.Instance != null && newValue >= 0)
        {
            SoundManager.Instance.PlaySfxLocal(countdownSfxIndex);
        }
    }

    private void UpdateUI(int timeValue)
    {
        if (Timet == null) return;

        if (timeValue > 0) Timet.text = timeValue.ToString();
        else if (timeValue == 0) Timet.text = "GO!";
        else if (timeValue == -99) Timet.text = "";
        else Timet.text = "Waiting...";
    }

    private void TeleportAllPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                // 서버에서 위치 수정
                client.PlayerObject.transform.position = startPosition;

                // 클라이언트들에게 텔레포트 명령 (PlayerStats 스크립트 필요)
                if (client.PlayerObject.TryGetComponent<PlayerStats>(out var stats))
                {
                    stats.TeleportPlayerClientRpc(startPosition);
                }
            }
        }
    }
}