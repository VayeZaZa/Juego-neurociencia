using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManagerEco : MonoBehaviour
{
    [Header("Zonas")]
    public RectTransform zonaLoEntiendo;
    public RectTransform zonaLoAplico;

    [Header("Slots Entiendo")]
    public RectTransform[] slotsEntiendo;

    [Header("Slots Aplico")]
    public RectTransform[] slotsAplico;

    [Header("Tarjetas en orden")]
    public CanvasGroup[] tarjetasEnOrden;

    [Header("Paneles")]
    public CanvasGroup panelInstrucciones;
    public CanvasGroup panelGanar;
    public CanvasGroup fadeEntrada;
    public CanvasGroup fadeSalida;

    [Header("Configuración")]
    public float duracionFade = 0.8f;
    public float duracionFadeGanar = 0.35f;

    [Header("Sonidos")]
    public AudioSource audioSourceEfectos;
    public AudioClip sonidoGanar;
    public AudioClip sonidoError;

    [Header("Flotación de Zonas")]
    public float flotacionAmplitud = 6f;    // píxeles que sube y baja
    public float flotacionVelocidad = 0.8f; // ciclos por segundo
    public float flotacionDesfase  = 0.5f; // desfase entre las dos zonas (0-1)

    private int slotEntiendo = 0;
    private int slotAplico   = 0;
    private int tarjetasCorrectas = 0;
    private int totalTarjetas = 6;
    private int tarjetaActual = 0;

    private Vector2 posOriginalEntiendo;
    private Vector2 posOriginalAplico;

    private Coroutine coroutineFlotacion;

    private UnityEngine.UI.Image destelloResultado;
    private Coroutine coroutineDestello;

    void Start()
    {
        // Mezclar referencias completas conserva el texto y la categoria de cada carta.
        tarjetasEnOrden = (CanvasGroup[])tarjetasEnOrden.Clone();
        for (int i = tarjetasEnOrden.Length - 1; i > 0; i--)
        {
            int elegido = Random.Range(0, i + 1);
            CanvasGroup temporal = tarjetasEnOrden[i];
            tarjetasEnOrden[i] = tarjetasEnOrden[elegido];
            tarjetasEnOrden[elegido] = temporal;
        }
        if (sonidoError == null)
            sonidoError = Resources.Load<AudioClip>("Capitulo1/ErrorTarjetas");
        if (sonidoError != null && sonidoError.loadState == AudioDataLoadState.Unloaded)
            sonidoError.LoadAudioData();
        CrearDestelloResultado();
        // Guardar posiciones originales de las zonas
        if (zonaLoEntiendo != null) posOriginalEntiendo = zonaLoEntiendo.anchoredPosition;
        if (zonaLoAplico   != null) posOriginalAplico   = zonaLoAplico.anchoredPosition;

        // Iniciar flotación
        coroutineFlotacion = StartCoroutine(FlotarZonas());

        // Ocultar todas las tarjetas menos la primera
        for (int i = 0; i < tarjetasEnOrden.Length; i++)
        {
            tarjetasEnOrden[i].alpha          = i == 0 ? 1f : 0f;
            tarjetasEnOrden[i].interactable   = i == 0;
            tarjetasEnOrden[i].blocksRaycasts = i == 0;
        }

        if (panelGanar != null)
        {
            panelGanar.alpha          = 0;
            panelGanar.interactable   = false;
            panelGanar.blocksRaycasts = false;
            panelGanar.gameObject.SetActive(false);
        }

        if (panelInstrucciones != null)
        {
            panelInstrucciones.alpha          = 1f;
            panelInstrucciones.interactable   = true;
            panelInstrucciones.blocksRaycasts = true;
        }

        if (fadeEntrada != null)
            StartCoroutine(FadeEntrada());
    }

    // Capa visual independiente: no necesita referencias nuevas en el Inspector.
    private void CrearDestelloResultado()
    {
        Canvas canvasUI = zonaLoEntiendo != null ? zonaLoEntiendo.GetComponentInParent<Canvas>(true) : null;
        if (canvasUI == null) return;
        GameObject capa = new GameObject("DestelloResultadoEco", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        capa.transform.SetParent(canvasUI.rootCanvas.transform, false);
        // Dibujar por encima de todo el Canvas, incluidas las cartas arrastradas.
        Canvas canvasDestello = capa.AddComponent<Canvas>();
        canvasDestello.overrideSorting = true;
        canvasDestello.sortingLayerID = canvasUI.rootCanvas.sortingLayerID;
        canvasDestello.sortingOrder = 32760;
        RectTransform rect = (RectTransform)capa.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        destelloResultado = capa.GetComponent<UnityEngine.UI.Image>();
        destelloResultado.raycastTarget = false;
        destelloResultado.color = Color.clear;
        capa.SetActive(false);
    }

    public void MostrarResultadoTarjeta(bool correcta)
    {
        if (destelloResultado == null) return;
        if (coroutineDestello != null) StopCoroutine(coroutineDestello);
        coroutineDestello = StartCoroutine(DestellarResultado(correcta));
    }

    private IEnumerator DestellarResultado(bool correcta)
    {
        destelloResultado.transform.SetAsLastSibling();
        destelloResultado.gameObject.SetActive(true);
        // Un solo sonido al iniciar el rojo; el verde y soltar fuera no lo reproducen.
        if (!correcta && audioSourceEfectos != null && sonidoError != null)
            audioSourceEfectos.PlayOneShot(sonidoError, 0.35f);
        Color color = correcta ? new Color(0.12f, 0.85f, 0.30f) : new Color(1f, 0.12f, 0.16f);
        const float entrada = 0.16f;
        const float pausa = 0.08f;
        const float duracion = 0.80f;
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            color.a = 0.30f * (tiempo < entrada
                ? Mathf.SmoothStep(0f, 1f, tiempo / entrada)
                : 1f - Mathf.SmoothStep(0f, 1f, (tiempo - entrada - pausa) / (duracion - entrada - pausa)));
            destelloResultado.color = color;
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        destelloResultado.color = Color.clear;
        destelloResultado.gameObject.SetActive(false);
        coroutineDestello = null;
    }

    private void OnDisable()
    {
        if (coroutineDestello != null) StopCoroutine(coroutineDestello);
        coroutineDestello = null;
        if (destelloResultado != null)
        {
            destelloResultado.color = Color.clear;
            destelloResultado.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (destelloResultado != null) Destroy(destelloResultado.gameObject);
    }

    //==================================================
    // FLOTACIÓN
    //==================================================

    IEnumerator FlotarZonas()
    {
        while (true)
        {
            float tiempo = Time.time * flotacionVelocidad * Mathf.PI * 2f;

            if (zonaLoEntiendo != null)
            {
                float offsetEntiendo = Mathf.Sin(tiempo) * flotacionAmplitud;
                zonaLoEntiendo.anchoredPosition = posOriginalEntiendo + new Vector2(0f, offsetEntiendo);
            }

            if (zonaLoAplico != null)
            {
                // El desfase hace que no suban y bajen al mismo tiempo, se ve más vivo
                float offsetAplico = Mathf.Sin(tiempo + flotacionDesfase * Mathf.PI * 2f) * flotacionAmplitud;
                zonaLoAplico.anchoredPosition = posOriginalAplico + new Vector2(0f, offsetAplico);
            }

            yield return null;
        }
    }

    //==================================================
    // SLOTS
    //==================================================

    public RectTransform ObtenerSlot(bool esAplico)
    {
        if (esAplico && slotAplico < slotsAplico.Length)
            return slotsAplico[slotAplico++];
        else if (!esAplico && slotEntiendo < slotsEntiendo.Length)
            return slotsEntiendo[slotEntiendo++];

        return null;
    }

    //==================================================
    // BOTÓN COMENZAR
    //==================================================

    public void ComenzarJuego()
    {
        StartCoroutine(CerrarPanelInstrucciones());
    }

    IEnumerator CerrarPanelInstrucciones()
    {
        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            panelInstrucciones.alpha = Mathf.Lerp(1, 0, t / duracionFade);
            yield return null;
        }

        panelInstrucciones.alpha          = 0;
        panelInstrucciones.interactable   = false;
        panelInstrucciones.blocksRaycasts = false;
        panelInstrucciones.gameObject.SetActive(false);
    }

    //==================================================
    // TARJETAS
    //==================================================

    public void TarjetaColocadaCorrectamente()
    {
        tarjetasCorrectas++;
        tarjetaActual++;

        if (tarjetaActual < tarjetasEnOrden.Length)
            StartCoroutine(MostrarSiguienteTarjeta(tarjetaActual));

        if (tarjetasCorrectas >= totalTarjetas)
            Invoke(nameof(MostrarVictoria), 1f);
    }

    IEnumerator MostrarSiguienteTarjeta(int indice)
    {
        yield return new WaitForSeconds(0.3f);

        CanvasGroup siguiente = tarjetasEnOrden[indice];
        float t = 0;
        float duracion = 0.4f;

        siguiente.interactable   = true;
        siguiente.blocksRaycasts = true;

        while (t < duracion)
        {
            t += Time.deltaTime;
            siguiente.alpha = Mathf.Lerp(0, 1, t / duracion);
            yield return null;
        }

        siguiente.alpha = 1f;
    }

    //==================================================
    // VICTORIA
    //==================================================

    void MostrarVictoria()
    {
        StartCoroutine(MostrarPanelVictoria());
    }

    IEnumerator MostrarPanelVictoria()
    {
        panelGanar.gameObject.SetActive(true);
        panelGanar.alpha = 0;

        // ============================================
        // Sonido de victoria (una sola vez)
        // ============================================
        if (audioSourceEfectos != null && sonidoGanar != null)
        {
            audioSourceEfectos.PlayOneShot(sonidoGanar);
        }

        float t = 0;

        while (t < duracionFadeGanar)
        {
            t += Time.deltaTime;
            panelGanar.alpha = Mathf.Lerp(0, 1, t / duracionFadeGanar);
            yield return null;
        }

        panelGanar.alpha          = 1;
        panelGanar.interactable   = true;
        panelGanar.blocksRaycasts = true;
    }

    //==================================================
    // BOTÓN DESBLOQUEAR
    //==================================================

    public void DesbloquearPuerta()
    {
        StartCoroutine(FadeSalir());
    }

    IEnumerator FadeSalir()
    {
        fadeSalida.gameObject.SetActive(true);
        fadeSalida.alpha          = 0;
        fadeSalida.blocksRaycasts = true;

        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            fadeSalida.alpha = Mathf.Lerp(0, 1, t / duracionFade);
            yield return null;
        }

        fadeSalida.alpha = 1;

        PlayerPrefs.SetInt("EstadoEcoCambio", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("02_Capitulo_1");
    }

    //==================================================
    // FADE ENTRADA
    //==================================================

    IEnumerator FadeEntrada()
    {
        fadeEntrada.gameObject.SetActive(true);
        fadeEntrada.alpha          = 1;
        fadeEntrada.blocksRaycasts = true;

        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            fadeEntrada.alpha = Mathf.Lerp(1, 0, t / duracionFade);
            yield return null;
        }

        fadeEntrada.alpha          = 0;
        fadeEntrada.blocksRaycasts = false;
        fadeEntrada.gameObject.SetActive(false);
    }
}