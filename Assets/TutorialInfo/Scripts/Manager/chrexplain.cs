using UnityEngine;
using TMPro;

public class chrexplain : MonoBehaviour
{
    public Subject_nextbutton subject;
    public TMP_Text explain;
    void OnEnable()
    {
        subject.OnStateChanged += HandleState ;
    }

    void OnDisable()
    {
        subject.OnStateChanged -= HandleState;
    }

    void HandleState(ChrState state)
    {
        switch (state)
        {
            case ChrState.excavator:
                explain.text = "좌클릭으로 바로 앞에 있는 플레이어를 잡을 수 있으며, 3초 안에 좌클릭을 다시 누르면, 바라보고 있는 방향으로 던진다. 던지는 데 실패했을 경우 괴물 상태가 즉시 해제된다.";
                break;

            case ChrState.bulldozer:
                explain.text = "5초간 상시 쉴드 상태가 되며 플레이어와 부딪힐 시 자신이 밀려날 힘까지 상대에게 적용된다.";
                break;

            case ChrState.dtruck:
                explain.text = "자신에게 실려있던 흙을 근처 상대에게 던진다. 피격 시 속력이 50% 감소된다.";
                break;
        }
    }
}
