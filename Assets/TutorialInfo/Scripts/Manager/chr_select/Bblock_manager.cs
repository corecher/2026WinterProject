using System;
using UnityEngine;
using UnityEngine.UI;

public class Bblock_manager : MonoBehaviour
{
    public event Action<Ready> OnreadyChanged;
    private Ready readystate = Ready.none;

    public Button ReadyB;
    public Button NextB;
    public Button PrevB;

    
    void Start()
    {
        OnreadyChanged?.Invoke(readystate);
        
    }

    public void OnclickReady()
    {
        switch (readystate)
        {
            case Ready.none:
                readystate = Ready.ready;
                OnreadyChanged?.Invoke(readystate);
                
                break;

            case Ready.ready:
                readystate = Ready.confirmed;
                OnreadyChanged?.Invoke(readystate);
                ReadyB.interactable = false;
                NextB.interactable = false;
                PrevB.interactable = false;
                break;
        }   
    }
}
