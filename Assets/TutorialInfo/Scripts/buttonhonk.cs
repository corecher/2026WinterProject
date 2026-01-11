using UnityEngine;

public class buttonhonk : MonoBehaviour
{
    public TutorialCameraFocus camFocus;   
    public Transform tutorialTarget;       

    public void OnClick_StartTutorialView()
    {
        camFocus.StartFocus(tutorialTarget);
    }

    public void OnClick_EndTutorialView()
    {
        camFocus.StopFocus();
    }
}

