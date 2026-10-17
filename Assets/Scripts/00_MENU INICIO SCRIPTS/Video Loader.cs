using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VideoLoader : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImage;

    void Start()
    {
        rawImage.color = new Color(1, 1, 1, 0); // empieza invisible

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // velocidad del fade, súbelo para más rápido
            rawImage.color = new Color(1, 1, 1, t);
            yield return null;
        }

        rawImage.color = new Color(1, 1, 1, 1); // asegura que quede al 100%
    }
}