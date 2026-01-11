using UnityEngine;

public class tuto : MonoBehaviour
{
    public GameObject Image; 

    public void ShowTutorial()
    {
        Image.SetActive(true);
    }

    public void HideTutorial()
    {
        Image.SetActive(false);
    }
}
