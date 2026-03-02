using UnityEngine;
using UnityEngine.SceneManagement;

public class scene_change : MonoBehaviour
{
    [SerializeField] private string sceneName;
    public void GotoScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
