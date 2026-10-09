using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CorazonController : MonoBehaviour
{
    [Header("Configuracion Fade")]
    public float duracionFadeIn = 0.8f;

    [Header("Configuracion Latido")]
    public float velocidadLatido = 1.0f;
    public float intensidadLatido = 0.1f;

    [Header("Sonido")]
    public AudioSource audioSourceEfectos;
    public AudioClip sonidoPop;

    private Image img;
    private Vector3 escalaOriginal;

    void Awake()
    {
        img = GetComponent<Image>();
        escalaOriginal = transform.localScale;
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
    }

    void OnEnable()
    {
        // ============================================
        // Sonido de pop al aparecer el corazón (click en la nube)
        // ============================================
        if (audioSourceEfectos != null && sonidoPop != null)
        {
            audioSourceEfectos.PlayOneShot(sonidoPop);
        }

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        transform.localScale = Vector3.zero;

        float tiempo = 0f;
        while (tiempo < duracionFadeIn)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionFadeIn;

            img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(0f, 1f, t));
            transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginal, t);

            yield return null;
        }

        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        transform.localScale = escalaOriginal;

        StartCoroutine(Latido());
    }

    IEnumerator Latido()
    {
        while (true)
        {
            float pulso = 1f + Mathf.Sin(Time.time * velocidadLatido) * intensidadLatido;
            transform.localScale = escalaOriginal * pulso;
            yield return null;
        }
    }
}