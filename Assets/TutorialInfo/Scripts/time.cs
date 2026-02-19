using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode; // 필수 네임스페이스
using UnityEngine.SceneManagement;

public class NetworkGameTimer : NetworkBehaviour
{
    [Header("UI 연결")]
    public TMP_Text Timet;

    [Header("설정")]
    public int starttime = 3;

    // 현재 시간을 동기화하는 변수입니다. (초기값 -1: 대기 상태)
    // 서버만 쓸 수 있고(Write), 모든 사람이 읽을 수 있습니다(Read).
    private NetworkVariable<int> netCurrentTime = new NetworkVariable<int>(
        -1, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (Timet != null) Timet.text = "다른 플레이어 대기 중...";
    }

    public override void OnNetworkSpawn()
    {
        // 1. 값 변경 시 UI 업데이트 함수가 실행되도록 연결
        netCurrentTime.OnValueChanged += OnTimeValueChanged;

        // 2. [서버 전용] 씬 로드 이벤트 감지 시작
        if (IsServer)
        {
            // 모든 클라이언트가 씬 로드를 마쳤을 때 발생하는 이벤트
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnAllPlayersLoaded;
        }
        
        // 초기 상태 UI 갱신
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

    // --- [Server Side] 로직 ---

    // 모든 플레이어가 씬에 들어왔을 때 서버에서 자동 실행됨
    private void OnAllPlayersLoaded(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        // 현재 게임 씬인지 확인 (안전장치)
        if (sceneName == SceneManager.GetActiveScene().name)
        {
            Debug.Log("모든 플레이어 로드 완료! 카운트다운 시작.");
            StartCoroutine(ServerCountdown());
        }
    }

    IEnumerator ServerCountdown()
    {
        // 잠시 대기 (로딩 직후 바로 시작하면 정신없으므로)
        yield return new WaitForSecondsRealtime(1.5f);

        // 카운트다운 시작 (3, 2, 1)
        int current = starttime;
        while (current > 0)
        {
            netCurrentTime.Value = current; // 값을 바꾸면 클라이언트들에게 자동 전송됨
            yield return new WaitForSecondsRealtime(1f);
            current--;
        }

        // GO! 상태 (0으로 약속)
        netCurrentTime.Value = 0;
        
        // GO 메시지 보여주는 시간 대기
        yield return new WaitForSecondsRealtime(1.5f);

        // 텍스트 끄기 상태 (-99로 약속)
        netCurrentTime.Value = -99;
    }

    // --- [Client Side] UI 업데이트 ---

    // NetworkVariable 값이 바뀔 때마다 실행됨
    private void OnTimeValueChanged(int previousValue, int newValue)
    {
        UpdateUI(newValue);
    }

    private void UpdateUI(int timeValue)
    {
        if (Timet == null) return;

        if (timeValue > 0)
        {
            // 3, 2, 1 카운트
            Timet.text = timeValue.ToString();
        }
        else if (timeValue == 0)
        {
            // 0일 때는 GO!
            Timet.text = "GO!";
        }
        else if (timeValue == -99)
        {
            // 끝나면 텍스트 지우기
            Timet.text = "";
        }
        else
        {
            // -1 등 초기 상태
            Timet.text = "Waiting...";
        }
    }
}
