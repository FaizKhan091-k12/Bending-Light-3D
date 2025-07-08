using System.Collections;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

public class LaserMenuWalkThrough : MonoBehaviour
{
    [SerializeField] float speed,minAlpha,maxAlpha;
    [SerializeField] ProceduralImage walkThroughImage;

    void Start()
    {
        StartCoroutine(ImageAlphaController());
    }

    IEnumerator ImageAlphaController()
    {
        float t = 0f;

        while (true)
        {

            t += Time.deltaTime * speed;

            float sinTime = (Mathf.Sin(t) + 1) / 2f;
            walkThroughImage.color = new Color(0, 0, 0, Mathf.Lerp(minAlpha, maxAlpha, sinTime));

            yield return null;

        }
    }
}
