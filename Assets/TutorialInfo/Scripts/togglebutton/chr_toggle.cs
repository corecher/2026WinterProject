using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class chr_toggle : MonoBehaviour
{
    public TMP_Text chrselect;
    public Button chrselectbutton;
    public Bblock_manager subject;

    private Image img;

    void Awake()
    {
        img = chrselectbutton.GetComponent<Image>();
        
    }

    void OnEnable()
    {
        subject.OnreadyChanged += HandleReadyStateChange; 
    }

    void OnDisable()
    {
        subject.OnreadyChanged -= HandleReadyStateChange;
    }

    void HandleReadyStateChange(Ready readystate)
    {
        switch (readystate)
        {
            case Ready.none:
                chrselect.text = "준비";
                
                break;

            case Ready.ready:
                chrselect.text = "확실합니까?";
                
                break;

            case Ready.confirmed:
                chrselect.text = "확정";
                
                img.color = Hex("#8BFFB2");
                break;
                
        }
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
