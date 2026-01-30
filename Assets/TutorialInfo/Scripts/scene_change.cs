using UnityEngine;
using UnityEngine.SceneManagement;

public class scene_change : MonoBehaviour
{
    public void Onclick_scene()
    {
        Scene currentscene = SceneManager.GetActiveScene();
        int Sceneindex = currentscene.buildIndex;

        if(Sceneindex == 0)
        {
            SceneManager.LoadScene(Sceneindex + 1);
        }
        else
        {
            SceneManager.LoadScene(Sceneindex - 1);
        }
    }
}
