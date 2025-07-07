using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextAlphaController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI laserTapText;
      [SerializeField] float minAlpha;
    [SerializeField] float maxAlpha;
    [SerializeField] float alphasmoothTime;


    void OnEnable()
    {
        StartCoroutine(TextColorPingPop());
    }
    IEnumerator TextColorPingPop()
    {
        float t = 0f;
        bool goingUp = true;

        while (true)
        {
            t = 0f;

            float from = goingUp ? minAlpha : maxAlpha;
            float to = goingUp ? maxAlpha : minAlpha;

            while (t < 1f)
            {
                t += Time.deltaTime * alphasmoothTime;

                laserTapText.color = new Color(1, 1, 1, Mathf.Lerp(from, to, t));
                yield return null;
            }

            goingUp = !goingUp;
        }

    }
}
