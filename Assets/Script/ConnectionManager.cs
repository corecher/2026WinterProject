using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : NetworkBehaviour
{
    public void StartHostAndLoadLobby()
    {
        // 1. 호스트로 시작
        if (NetworkManager.Singleton.StartHost())
        {
            // 2. 호스트가 성공적으로 시작되면 대기실 씬으로 이동
            // NetworkSceneManager를 사용해야 모든 클라이언트가 동기화됩니다.
            NetworkManager.Singleton.SceneManager.LoadScene("test", LoadSceneMode.Single);
        }
    }

    public void StartClient()
    {
        // 클라이언트는 단순히 시작만 하면 됩니다.
        // 서버가 이미 LobbyScene에 있다면 접속 즉시 해당 씬으로 자동 동기화됩니다.
        NetworkManager.Singleton.StartClient();
    }
}
