using UnityEngine;
using UnityEngine.SceneManagement;

public class scene_change : MonoBehaviour
{
    public void Onclick_Nextscene()
    {
        Scene currentscene = SceneManager.GetActiveScene();
        int Sceneindex = currentscene.buildIndex;
            SceneManager.LoadScene(Sceneindex + 1);
    }

    public void Onclick_Prevscene()
    {
        Scene currentscene = SceneManager.GetActiveScene();
        int Sceneindex = currentscene.buildIndex;
        SceneManager.LoadScene(Sceneindex - 1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
