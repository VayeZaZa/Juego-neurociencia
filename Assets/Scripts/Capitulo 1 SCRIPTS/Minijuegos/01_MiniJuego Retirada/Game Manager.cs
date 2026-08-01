using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup panelInstrucciones;
    public GameObject panelGanar;
    public CanvasGroup panelGanarGroup;
    public TextMeshProUGUI textoContador;
    public CanvasGroup panelFadeEntrada; // nuevo campo agregado
    public CanvasGroup panelFadeSalida; // NUEVO

    [Header("Objetos del juego")]
    public GameObject cerebro;
    public GameObject pensamientos;
    public GameObject corazones;
    public GameObject contador;

    [Header("Velocidad Fades")]
    public float velocidadFadeOut = 1.5f;
    public float velocidadFadeIn = 1.5f;

    [Header("Sonido de Victoria")]
    public AudioSource audioSourceEfectos;
    public AudioClip sonidoGanar;

    private int nubesExplotadas = 0;
    private int totalNubes = 6;

    void Start()
    {
        StartCoroutine(FadeEntrada());
        panelInstrucciones.alpha = 1f;
        panelInstrucciones.gameObject.SetActive(true);
        panelGanar.SetActive(false);
        contador.SetActive(true);

        cerebro.SetActive(true);
        pensamientos.SetActive(true);

        foreach (Transform hijo in corazones.transform)
            hijo.gameObject.SetActive(false);
    }

    public void ComenzarJuego()
    {
        StartCoroutine(TransicionComenzar());
    }

    IEnumerator TransicionComenzar()
    {
        // Fade out del panel instrucciones
        float tiempo = 0f;
        while (tiempo < velocidadFadeOut)
        {
            tiempo += Time.deltaTime;
            panelInstrucciones.alpha = Mathf.Lerp(1f, 0f, tiempo / velocidadFadeOut);
            yield return null;
        }
        panelInstrucciones.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        contador.SetActive(true);

        yield return StartCoroutine(FadeInSprites(velocidadFadeIn));
    }

    IEnumerator FadeInSprites(float duracion)
    {
        SpriteRenderer[] spritesCerebro = cerebro.GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer[] spritesPensamientos = pensamientos.GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spritesCerebro)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        foreach (var sr in spritesPensamientos)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / duracion);

            foreach (var sr in spritesCerebro)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            foreach (var sr in spritesPensamientos)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

            yield return null;
        }
    }

    public void NubeExplotada(GameObject corazonAsociado)
    {
        nubesExplotadas++;
        ActualizarContador();
        corazonAsociado.SetActive(true);

        if (nubesExplotadas >= totalNubes)
            Invoke(nameof(MostrarVictoria), 1.5f);
    }

    void ActualizarContador()
    {
        textoContador.text = nubesExplotadas + "/" + totalNubes;
    }

    void MostrarVictoria()
    {
        StartCoroutine(AnimacionVictoria());
    }

    IEnumerator AnimacionVictoria()
    {
        panelGanar.SetActive(true);

        // ============================================
        // Reproducir sonido de victoria (una sola vez)
        // ============================================
        if (audioSourceEfectos != null && sonidoGanar != null)
        {
            audioSourceEfectos.PlayOneShot(sonidoGanar);
        }

        CanvasGroup cgGanar = panelGanarGroup;
        cgGanar.alpha = 0f;

        float duracionFade = 0.6f;
        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            cgGanar.alpha = Mathf.Lerp(0f, 1f, tiempo / duracionFade);
            yield return null;
        }

        cgGanar.alpha = 1f;
    }

    // LatidoImagen method removed as per requirements

    IEnumerator FadeEntrada()
    {
        panelFadeEntrada.alpha = 1f;
        panelFadeEntrada.blocksRaycasts = true;

        float duracion = 1.5f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            panelFadeEntrada.alpha = Mathf.Lerp(1f, 0f, tiempo / duracion);
            yield return null;
        }

        panelFadeEntrada.alpha = 0f;
        panelFadeEntrada.blocksRaycasts = false;
        panelFadeEntrada.gameObject.SetActive(false);
    }

    public void Regresar()
    {
        StartCoroutine(FadeSalida());
    }

    IEnumerator FadeSalida()
{
    panelFadeSalida.gameObject.SetActive(true);
    panelFadeSalida.alpha = 0f;
    panelFadeSalida.blocksRaycasts = true;

    float duracion = 1.5f;
    float tiempo = 0f;

    while (tiempo < duracion)
    {
        tiempo += Time.deltaTime;
        panelFadeSalida.alpha = Mathf.Lerp(0f, 1f, tiempo / duracion);
        yield return null;
    }

    panelFadeSalida.alpha = 1f;

    // Guarda que el minijuego fue completado ← NUEVO
    PlayerPrefs.SetInt("EstadoRetirada", 1);
    PlayerPrefs.Save();

    UnityEngine.SceneManagement.SceneManager.LoadScene("02_Capitulo_1");
}
}