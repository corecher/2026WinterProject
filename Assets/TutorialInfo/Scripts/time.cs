using UnityEngine;
using TMPro;
using System.Collections;
public class time : MonoBehaviour
{
    public TMP_Text Timet;
    public int starttime = 3;

    void Start()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        int current = starttime;
        while (current > 0)
        {
            Timet.text = current.ToString();
            yield return new WaitForSecondsRealtime(1f);
            current--;
            if(current == 0)
            {
                Timet.text = "GO!";
                yield return new WaitForSecondsRealtime(1.5f);
                Timet.text = "";
                yield break;
            }
        }
        

    }
}
