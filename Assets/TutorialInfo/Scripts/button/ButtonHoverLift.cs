using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가

public class ReadyToggleNetwork : NetworkBehaviour
{
    [SerializeField] private Button readyButton;
    private TMP_Text readyText;
    private Image img;
    [SerializeField] private string nextScene;

    public NetworkVariable<bool> netIsReady = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            netIsReady.Value = false;
        }

        if (IsOwner)
        {
            // 씬 로드 이벤트 구독 (씬이 바뀔 때마다 호출됨)
            SceneManager.sceneLoaded += OnSceneLoaded;
            // 첫 스폰 시에도 버튼을 찾아야 하므로 수동 호출
            StartSearchUI();
        }
        
        netIsReady.OnValueChanged += OnReadyStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        netIsReady.OnValueChanged -= OnReadyStateChanged;
        if (IsOwner)
        {
            // 이벤트 구독 해제 (메모리 누수 방지)
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // 씬이 로드될 때마다 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsOwner)
        {
            StartSearchUI();
        }
    }

    private void StartSearchUI()
    {
        // 이전 씬의 버튼 참조가 남아있을 수 있으므로 초기화
        readyButton = null;
        StopAllCoroutines(); // 혹시 실행 중인 찾기 코루틴이 있다면 중지
        StartCoroutine(WaitForUIAndConnect());
    }

    IEnumerator WaitForUIAndConnect()
    {
        Debug.Log("🔍 ReadyButton을 찾는 중...");
        
        while (readyButton == null)
        {
            GameObject btnObj = GameObject.Find("ReadyButton");
            
            if (btnObj != null)
            {
                readyButton = btnObj.GetComponent<Button>();
                img = readyButton.GetComponent<Image>();
                readyText = readyButton.GetComponentInChildren<TMP_Text>();

                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(OnClickToggle);

                UpdateUI(netIsReady.Value);
                Debug.Log("✅ ReadyButton 연결 완료!");
            }
            else
            {
                // 너무 오래 못 찾을 경우를 대비해 일정 시간 후 종료하거나 
                // 특정 씬이 아닐 경우 중단하는 로직을 넣을 수도 있습니다.
                yield return new WaitForSeconds(0.5f); // 매 프레임보다 조금 여유 있게 찾기
            }
        }
    }

    public void OnClickToggle()
    {
        if (IsOwner)
        {
            ToggleReadyServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleReadyServerRpc()
    {
        netIsReady.Value = !netIsReady.Value;
        CheckAllPlayersReady();
    }

    private void CheckAllPlayersReady()
    {
        if (!IsServer) return;

        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        bool allReady = true;

        foreach (var client in connectedClients)
        {
            if (client.PlayerObject == null) continue;
            var script = client.PlayerObject.GetComponent<ReadyToggleNetwork>();
            
            if (script == null || !script.netIsReady.Value)
            {
                allReady = false;
                break;
            }
        }

        if (allReady && connectedClients.Count > 0)
        {
            // 씬 전환 전 모든 상태 리셋
            ResetAllPlayersReady();
            NetworkManager.Singleton.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    private void ResetAllPlayersReady()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var script = client.PlayerObject.GetComponent<ReadyToggleNetwork>();
                if (script != null) script.netIsReady.Value = false;
            }
        }
    }

    private void OnReadyStateChanged(bool previous, bool current)
    {
        if (IsOwner)
        {
            UpdateUI(current);
        }
    }

    void UpdateUI(bool isReady)
    {
        if (readyButton == null) return;
        
        if (!isReady)
        {
            readyText.text = "준비";
            img.color = Color.white;
        }
        else
        {
            readyText.text = "준비 완료";
            img.color = Color.green;
        }
    }
}

