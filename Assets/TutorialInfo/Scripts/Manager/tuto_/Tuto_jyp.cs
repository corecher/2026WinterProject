using TMPro;
using UnityEngine;

public class Tuto_jyp : MonoBehaviour
{
    public Tuto_nextbutton tuto;
    public TMP_Text Tuto_jy;

    void OnEnable()
    {
        tuto.OnTutobuttonclick += HandleTuto;
    }

    void OnDisable()
    {
        tuto.OnTutobuttonclick -= HandleTuto;
    }


    void HandleTuto(Tuto tuto)
    {
        switch (tuto)
        {
            case Tuto.oper:
                Tuto_jy.text = "기본조작";
                break;

            case Tuto.booster:
                Tuto_jy.text = "부스터";
                break;

            case Tuto.shield:
                Tuto_jy.text = "쉴드";
                break;

            case Tuto.gover:
                Tuto_jy.text = "게임 오버";
                break;

            case Tuto.monster_1:
                Tuto_jy.text = "괴물 상태 #1";
                break;

            case Tuto.monster_2:
                Tuto_jy.text = "괴물 상태 #2";
                break;

            case Tuto.gflow:
                Tuto_jy.text = "승리";
                break;
        }
    }
}
