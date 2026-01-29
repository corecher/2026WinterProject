using UnityEngine;
using UnityEngine.UI;

public class open_playercontent : MonoBehaviour
{
    public GameObject player_content;
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
        }
        else
        {
            player_content.SetActive(false);
        }
    }
    
}
