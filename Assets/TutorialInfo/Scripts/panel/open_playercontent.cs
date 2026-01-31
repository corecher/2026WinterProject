using UnityEngine;
using UnityEngine.UI;


public class open_playercontent : MonoBehaviour
{
    public GameObject player_content;
    public Image button_image;
    private bool Isopen = false;
    
    void Start()
    {
        Update_PlayerUI();
    }

    public void OnclickPlayerButton()
    {
        Isopen = !Isopen;
        Update_PlayerUI();
    }

    void Update_PlayerUI()
    {
        if (Isopen)
        {
            player_content.SetActive(true);
            Debug.Log(Isopen);
            button_image.rectTransform.rotation = Quaternion.Euler(0f, 0f, 180f);     
        }
        else
        {
            player_content.SetActive(false);
            Debug.Log(Isopen);
            button_image.rectTransform.rotation = Quaternion.identity;
        }
    }
    
}
