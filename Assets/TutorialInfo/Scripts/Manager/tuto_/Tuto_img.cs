using UnityEngine;
using UnityEngine.UI;

public class Tuto_img : MonoBehaviour
{
    public Tuto_nextbutton tuto;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite operimg;
    [SerializeField] private Sprite boosterimg;
    [SerializeField] private Sprite shieldimg;
    [SerializeField] private Sprite goverimg;
    [SerializeField] private Sprite mons1img;
    [SerializeField] private Sprite mons2img;
    [SerializeField] private Sprite gflowimg;
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
                targetImage.sprite = operimg;
                break;

            case Tuto.booster:
                targetImage.sprite = boosterimg;
                break;

            case Tuto.shield:
                targetImage.sprite = shieldimg;
                break;

            case Tuto.gover:
                targetImage.sprite = goverimg;
                break;

            case Tuto.monster_1:
                targetImage.sprite = mons1img;
                break;

            case Tuto.monster_2:
                targetImage.sprite = mons2img;
                break;

            case Tuto.gflow:
                targetImage.sprite = gflowimg;
                break;
        }
    }
}
