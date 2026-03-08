using UnityEngine;
using UnityEngine.UI;

public class round : MonoBehaviour
{
    public RawImage round1B;
    public RawImage round2B;
    public RawImage round3B;

    [SerializeField] public int currentround = 1;

    void Update()
    {
        if (currentround == 1)
        {
            round1B.color = Hex("#8BFFB2");
            round2B.color = Hex("#FFFFFF");
            round3B.color = Hex("#FFFFFF");
        }

        if (currentround == 2)
        {
            round1B.color = Hex("#8BFFB2");
            round2B.color = Hex("#8BFFB2");
            round3B.color = Hex("#FFFFFF");
        }

        if (currentround == 3)
        {
            round1B.color = Hex("#8BFFB2");
            round2B.color = Hex("#8BFFB2");
            round3B.color = Hex("#8BFFB2");
        }
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
