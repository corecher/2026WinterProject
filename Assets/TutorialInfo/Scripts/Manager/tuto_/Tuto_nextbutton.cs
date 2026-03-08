using UnityEngine;
using System;

public class Tuto_nextbutton : MonoBehaviour
{
    public event Action<Tuto> OnTutobuttonclick;
    private Tuto _currentTuto = Tuto.oper;

    void Start()
    {
        OnTutobuttonclick?.Invoke(_currentTuto);
    }

    public void NextTutoBclick()
    {
        switch (_currentTuto)
        {
            case Tuto.oper:
                _currentTuto = Tuto.booster;
                break;

            case Tuto.booster:
                _currentTuto = Tuto.shield;
                break;

            case Tuto.shield:
                _currentTuto = Tuto.gover;
                break;

            case Tuto.gover:
                _currentTuto = Tuto.monster_1;
                break;

            case Tuto.monster_1:
                _currentTuto = Tuto.monster_2;
                break;

            case Tuto.monster_2:
                _currentTuto = Tuto.gflow;
                break;

            case Tuto.gflow:
                _currentTuto = Tuto.gflow;
                break;
        }
        OnTutobuttonclick?.Invoke(_currentTuto);
    }

    public void PrevTutoBclick()
    {
        switch (_currentTuto)
        {

            case Tuto.oper:
                _currentTuto= Tuto.oper;
                break;

            case Tuto.booster:
                _currentTuto = Tuto.oper;
                break;

            case Tuto.shield:
                _currentTuto = Tuto.booster;
                break;

            case Tuto.gover:
                _currentTuto = Tuto.shield;
                break;

            case Tuto.monster_1:
                _currentTuto = Tuto.gover;
                break;

            case Tuto.monster_2:
                _currentTuto = Tuto.monster_1;
                break;

            case Tuto.gflow:
                _currentTuto = Tuto.monster_2;
                break;
        }
        OnTutobuttonclick?.Invoke(_currentTuto);
    }
}
