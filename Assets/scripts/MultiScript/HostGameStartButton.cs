using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// NetworkBehaviour가 아니라 MonoBehaviour를 씁니다!
public class HostButtonVisibility : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene"; // 이동할 씬 이름
    private Button startButton;
    private bool isButtonActive = false;

    private void Awake()
    {
        startButton = GameObject.Find("HostStartButton").GetComponent<Button>();
        
        // 일단 끕니다.
        startButton.gameObject.SetActive(false);
        
        // 클릭 이벤트 연결
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void Update()
    {
        // 1. 네트워크 매니저가 없거나, 연결이 안 되어있으면 무조건 숨김
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            if (isButtonActive)
            {
                startButton.gameObject.SetActive(false);
                isButtonActive = false;
            }
            return;
        }

        // 2. "내가 방장(Host)인가?"를 실시간으로 체크
        bool isHost = NetworkManager.Singleton.IsHost;

        // 상태가 바뀔 때만 SetActive를 호출 (성능 최적화)
        if (isButtonActive != isHost)
        {
            isButtonActive = isHost;
            startButton.gameObject.SetActive(isHost);
            
            if (isHost) Debug.Log("방장 권한 확인됨: 시작 버튼 활성화");
        }
    }

    private void OnStartClicked()
    {
        // 혹시 모르니 누를 때 한번 더 체크
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("게임 시작! 씬 이동 중...");
            NetworkManager.Singleton.SceneManager.LoadScene(
                gameSceneName, 
                LoadSceneMode.Single
            );
        }
    }
}
