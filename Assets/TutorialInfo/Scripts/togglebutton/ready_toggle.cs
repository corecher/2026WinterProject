using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ready_toggle : MonoBehaviour
{
    
    public TMP_Text readyText;
    public Button readyButton;

    private Image img;
    public bool isReady = false;

    void Awake()
    {
        
        img = readyButton.GetComponent<Image>();

        
        readyButton.onClick.AddListener(OnClickToggle);
    }

    void Start()
    {
        UpdateUI();
    }
    public void OnClickToggle()
    {
        isReady = !isReady;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (!isReady)
        {
            readyText.text = "시작?";
            img.color = Hex("#FFFFFF");
            
        }
        else
        {
            readyText.text = "시작!";
            img.color = Hex("#8BFFB2");
        }
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
