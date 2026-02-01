using UnityEngine;
using TMPro;

public class chrname : MonoBehaviour
{
    public Subject_nextbutton subject;
    public TMP_Text Name;   

    void OnEnable()
    {
        subject.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        subject.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(ChrState state)
    {
        switch (state)
        {
            case ChrState.excavator:
                Name.text = "포크레인";
                break;

            case ChrState.bulldozer:
                Name.text = "불도저";
                break;

            case ChrState.dtruck:
                Name.text = "덤프트럭";
                break;
        }
    }
}
