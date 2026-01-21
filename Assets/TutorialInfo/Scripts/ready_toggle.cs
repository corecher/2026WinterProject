using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ready_toggle : MonoBehaviour
{
    
    public TMP_Text readyText;
    public Button readyButton;

    private Image img;
    private bool isReady = false; 

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
            readyText.text = "¡ÿ∫Ò";
            img.color = Hex("#FFFFFF");
            Debug.Log("µ ");
        }
        else
        {
            readyText.text = "¡ÿ∫Ò øœ∑·";
            img.color = Hex("#8BFFB2");
            Debug.Log("µ ");
        }
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
