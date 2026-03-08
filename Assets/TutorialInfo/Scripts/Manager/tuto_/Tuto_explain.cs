using UnityEngine;
using TMPro;

public class Tuto_explain : MonoBehaviour
{
    public Tuto_nextbutton tuto;
    public TMP_Text explain_content;

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
                explain_content.text = "W, A, S, D를 활용하여 이동하고, Space로 점프하세요!";
                break;

            case Tuto.booster:
                explain_content.text = "부스트 게이지는 천천히 모입니다, 부스트 게이지를 소모하여 빠르게 이동하세요! 단, 장애물에 부딪히면 속도가 잠시 느려지고, 부스트 게이지가 해당 속력에 비례하여 최대 50% 깎입니다.";
                break;

            case Tuto.shield:
                explain_content.text = " 한 라운드당 3번의 쉴드를 사용할 수 있습니다. 장애물에 부딪히기 직전 쉴드를 사용하면 부스트 감소가 무시되며, 오히려 속력이 잠시 빨라지고 분노 게이지가 20%만큼 쌓입니다.";
                break;

            case Tuto.gover:
                explain_content.text = "너무 느리게 가면 분쇄기에게 따라잡혀 아웃되니 조심하세요!";
                break;

            case Tuto.monster_1:
                explain_content.text = "분노 포인트를 획득하거나, 쉴드를 알맞게 씀으로서, 분노 게이지를 모두 채웠다면 20초 동안 “괴물 상태”에 돌입하게 됩니다!";
                break;

            case Tuto.monster_2:
                explain_content.text = "괴물 상태에 돌입하면 부스트 게이지가 100%에서 줄어들지 않으며, 기본적인 속도가 아주 빨라집니다! 또한 상대를 방해할 수 있는 스킬을 LMB로 사용할 수 있게 됩니다!";
                break;

            case Tuto.gflow:
                explain_content.text = "한 명의 플레이어가 20점을 달성하여 최종 우승할 때까지 라운드가 반복되며, 도착 유무, 빠른 순위, 자신으로 인해 아웃된 플레이어 수에 따라 점수가 오릅니다." +
                    " (도착 : 1점, 빠른 순위 (1위부터) : 4, 2, 1, 0, 킬 수 : 2)";
                break;
        }
    }
}
