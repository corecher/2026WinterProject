using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class chr_toggle : MonoBehaviour
{
    public TMP_Text chrselect;
    public Button chrselectbutton;

    private Image img;
    private int decide = 0;

    void Awake()
    {
        img = chrselectbutton.GetComponent<Image>();
        chrselectbutton.onClick.AddListener(OnClickchrselect);
    }

    void Start()
    {
        UpdateUI();
    }

    public void OnClickchrselect()
    {
        if (decide >= 2) return;   
        decide++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (decide == 0)
        {
            chrselect.text = "선택";
            img.color = Hex("#FFFFFF");
            chrselectbutton.interactable = true;
        }
        else if (decide == 1)
        {
            chrselect.text = "확실합니까?";
        }
        else 
        {
            chrselect.text = "확정!";
            img.color = Hex("#8BFFB2");
            chrselectbutton.interactable = false; 
        }
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
