using UnityEngine;
using System;

public class Subject_nextbutton : MonoBehaviour
{
    public event Action<ChrState> OnStateChanged;
    private ChrState state = ChrState.excavator;

    void Start()
    {
        OnStateChanged?.Invoke(state);    
    }

    public void NextOnclickChangeChr()
    {
        switch (state)
        {
            case ChrState.excavator:
                state = ChrState.bulldozer;
                break;

            case ChrState.bulldozer:
                state = ChrState.dtruck;
                break;

            case ChrState.dtruck:
                state = ChrState.excavator;
                break;
        }
        OnStateChanged?.Invoke(state);

    }

    public void PreviousOnclickChangeChr()
    {
        switch (state)
        {
            case ChrState.excavator:
                state = ChrState.dtruck;
                break;

            case ChrState.bulldozer:
                state = ChrState.excavator;
                break;

            case ChrState.dtruck:
                state = ChrState.bulldozer;
                break;
        }
        OnStateChanged?.Invoke(state);
        
    }
}
