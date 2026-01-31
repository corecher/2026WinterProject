using UnityEngine;
using TMPro;
public class toggle_on_0ff : MonoBehaviour
{
    public TMP_Text stateText;

    private bool isOn = true;  

    void Start()
    {
        UpdateUI();
    }

    public void OnClickToggle()
    {
        isOn = !isOn;   
        UpdateUI();
    }

    void UpdateUI()
    {
        if (isOn)
        {
            stateText.text = "дт";
        }
        else
        {
            stateText.text = "╡Ш";
        }
    }
}
