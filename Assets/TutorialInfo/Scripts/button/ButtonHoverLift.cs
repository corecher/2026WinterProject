using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections; // 코루틴 사용을 위해 필요

public class ReadyToggleNetwork : NetworkBehaviour
{
    private Button readyButton;
    private TMP_Text readyText;
    private Image img;

    public NetworkVariable<bool> netIsReady = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // 내 캐릭터(IsOwner)인 경우에만 버튼을 찾으러 갑니다.
        if (IsOwner)
        {
            StartCoroutine(WaitForUIAndConnect());
        }
        
        // 상태 변경 이벤트는 언제든 받을 수 있게 미리 연결
        netIsReady.OnValueChanged += OnReadyStateChanged;
    }

    // UI가 켜질 때까지 기다리는 코루틴 함수
    IEnumerator WaitForUIAndConnect()
    {
        // readyButton을 찾을 때까지 무한 반복
        while (readyButton == null)
        {
            // 1. 버튼 찾기 시도
            GameObject btnObj = GameObject.Find("ReadyButton");
            
            if (btnObj != null)
            {
                // 2. 찾았으면 컴포넌트 연결
                readyButton = btnObj.GetComponent<Button>();
                img = readyButton.GetComponent<Image>();
                readyText = readyButton.GetComponentInChildren<TMP_Text>();

                // 3. 버튼 기능 연결
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(OnClickToggle);

                Debug.Log("✅ 드디어 ReadyButton을 찾아서 연결했습니다!");

                // 4. 현재 상태에 맞춰 UI 즉시 갱신
                UpdateUI(netIsReady.Value);
            }
            else
            {
                // 못 찾았으면 다음 프레임까지 대기 (UI가 켜질 때까지 기다림)
                yield return null; 
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
        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        bool allReady = true;

        foreach (var client in connectedClients)
        {
            if (client.PlayerObject == null) continue;
            var script = client.PlayerObject.GetComponent<ReadyToggleNetwork>();
            
            // 아직 스크립트 로딩이 덜 된 플레이어가 있으면 준비 안 된 것으로 처리
            if (script == null || !script.netIsReady.Value)
            {
                allReady = false;
                break;
            }
        }

        if (allReady && connectedClients.Count > 0)
        {
            // 씬 이름 확인 필수!
            NetworkManager.Singleton.SceneManager.LoadScene("chr_select", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnReadyStateChanged(bool previous, bool current)
    {
        // 버튼이 연결되어 있을 때만 UI 업데이트
        if (IsOwner && readyButton != null)
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
            img.color = Color.green; // Hex 대신 간단히 Color 사용
        }
    }
}

