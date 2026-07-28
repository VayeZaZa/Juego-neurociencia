using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecompensaCompromiso : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogoCompromiso dialogoCompromiso;

    [Header("Avatar")]
    public Animator avatarAnimator;

    [Header("Referencias - Brújula principal 3D")]
    public Transform brujula;
    public Transform puntoFinal;

    [Header("Movimiento espiral")]
    public float duracionSubida = 5.5f;
    public float radioEspiral = 1.0f;
    public float vueltas = 2.0f;

    [Header("Animación final (loop constante)")]
    public float velocidadRotacion = 35f;
    public float amplitudFlotacion = 0.06f;
    public float velocidadFlotacion = 1.8f;
    public float duracionEntradaFlotacion = 1.0f;

    [Header("Luz suave")]
    public Light luzBrujula;
    public float intensidadMaxima = 2f;
    public float duracionEncendido = 1.5f;

    [Header("Panel de Recompensa (UI)")]
    [Tooltip("El panel UI que contiene el texto de recompensa y el botón")]
    public GameObject panelRecompensa;
    [Tooltip("Segundos que espera en espiral/flotando antes de mostrar el panel")]
    public float esperaAntesDePanel = 3f;
    [Tooltip("Duración del fade in/out del panel")]
    public float duracionFadePanel = 0.8f;

    [Header("Botón y Animación de Pulso (UI)")]
    public RectTransform botonRecibirBrujula;
    [Tooltip("Amplitud suave (recomendado: 0.01 a 0.015)")]
    public float amplitudPulsoBoton = 0.015f;
    [Tooltip("Velocidad de respiración (recomendado: 1.2)")]
    public float velocidadPulsoBoton = 1.2f;
    [Tooltip("Velocidad de respuesta del Lerp")]
    public float suavizadoPulso = 8f;

    private bool flotando = false;
    private bool pulsandoBoton = false;
    private Vector3 posicionFinal;
    private Vector3 escalaInicialBoton;
    private float tiempoFlotacion = 0f;
    private CanvasGroup cgPanelRecompensa;

    void Start()
    {
        if (puntoFinal != null)
            posicionFinal = puntoFinal.position;

        if (brujula != null)
            brujula.gameObject.SetActive(false);

        if (luzBrujula != null)
            luzBrujula.intensity = 0f;

        if (botonRecibirBrujula != null)
            escalaInicialBoton = botonRecibirBrujula.localScale;

        // Preparamos el panel de recompensa (oculto e interactivo)
        if (panelRecompensa != null)
        {
            cgPanelRecompensa = panelRecompensa.GetComponent<CanvasGroup>();
            if (cgPanelRecompensa == null)
                cgPanelRecompensa = panelRecompensa.AddComponent<CanvasGroup>();

            cgPanelRecompensa.alpha = 0f;
            cgPanelRecompensa.interactable = false;
            cgPanelRecompensa.blocksRaycasts = false;
            panelRecompensa.SetActive(false);
        }
    }

    public void MostrarRecompensa()
    {
        if (avatarAnimator != null)
            avatarAnimator.CrossFade("Clapping", 0.08f);

        if (brujula == null || puntoFinal == null) return;

        brujula.gameObject.SetActive(true);
        StartCoroutine(SubirEnEspiral());

        if (luzBrujula != null)
            StartCoroutine(EncenderLuz());
    }

    IEnumerator EncenderLuz()
    {
        float t = 0f;
        while (t < duracionEncendido)
        {
            t += Time.deltaTime;
            luzBrujula.intensity = Mathf.Lerp(0f, intensidadMaxima, t / duracionEncendido);
            yield return null;
        }
        luzBrujula.intensity = intensidadMaxima;
    }

    IEnumerator SubirEnEspiral()
    {
        Vector3 inicio = brujula.position;
        float tiempo = 0f;

        while (tiempo < duracionSubida)
        {
            tiempo += Time.deltaTime;

            float tNormal = Mathf.Clamp01(tiempo / duracionSubida);
            float tSuave = Mathf.SmoothStep(0f, 1f, tNormal);

            Vector3 centro = Vector3.Lerp(inicio, posicionFinal, tSuave);
            float angulo = tNormal * vueltas * Mathf.PI * 2f;
            float radioActual = radioEspiral * Mathf.Sin((1f - tNormal) * Mathf.PI * 0.5f);

            Vector3 offset = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)) * radioActual;
            brujula.position = centro + offset;

            yield return null;
        }

        brujula.position = posicionFinal;

        tiempoFlotacion = 0f;
        flotando = true;

        // Sigue aplaudiendo un poquito más
        yield return new WaitForSeconds(2.5f);

        if (avatarAnimator != null)
        {
            avatarAnimator.speed = 1.5f; // vuelve a velocidad normal por si la cambiaste
            avatarAnimator.CrossFade("Idle", 1f);
        }

        yield return new WaitForSeconds(esperaAntesDePanel);

        StartCoroutine(MostrarPanelRecompensa());
    }

    IEnumerator MostrarPanelRecompensa()
    {
        if (panelRecompensa == null) yield break;

        panelRecompensa.SetActive(true);

        cgPanelRecompensa.alpha = 0f;
        cgPanelRecompensa.interactable = false;
        cgPanelRecompensa.blocksRaycasts = false;

        float t = 0f;
        while (t < duracionFadePanel)
        {
            t += Time.deltaTime;
            cgPanelRecompensa.alpha = Mathf.Clamp01(t / duracionFadePanel);
            yield return null;
        }

        cgPanelRecompensa.alpha = 1f;
        cgPanelRecompensa.interactable = true;
        cgPanelRecompensa.blocksRaycasts = true;

        // Inicia el pulso cuando el panel es interactivo
        pulsandoBoton = true;
    }

    // Función pública vinculada en el OnClick() del Button
    public void RecibirRecompensa()
    {
        if (panelRecompensa != null)
            StartCoroutine(FadeOutYCerrarPanel());
    }

    IEnumerator FadeOutYCerrarPanel()
    {
        pulsandoBoton = false;

        // Reseteamos el botón a su escala original
        if (botonRecibirBrujula != null)
            botonRecibirBrujula.localScale = escalaInicialBoton;

        cgPanelRecompensa.interactable = false;
        cgPanelRecompensa.blocksRaycasts = false;

        float t = 0f;

        while (t < duracionFadePanel)
        {
            t += Time.deltaTime;

            cgPanelRecompensa.alpha = Mathf.Lerp(1f, 0f, t / duracionFadePanel);

            yield return null;
        }

        panelRecompensa.SetActive(false);

        if (dialogoCompromiso != null)
        {
            dialogoCompromiso.DialogoDespedidaFinal();
        }
    }

    public void ApagarLuz()
    {
        if (luzBrujula != null)
            StartCoroutine(FadeOutLuz());
    }

    IEnumerator FadeOutLuz()
    {
        float inicioIntensidad = luzBrujula.intensity;
        float t = 0f;
        while (t < duracionEncendido)
        {
            t += Time.deltaTime;
            luzBrujula.intensity = Mathf.Lerp(inicioIntensidad, 0f, t / duracionEncendido);
            yield return null;
        }
        luzBrujula.intensity = 0f;
    }

    void Update()
    {
        // 1. Brújula 3D rotando y flotando
        if (flotando && brujula != null)
        {
            tiempoFlotacion += Time.deltaTime;

            brujula.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime, Space.World);

            float factorEntrada = (duracionEntradaFlotacion > 0f)
                ? Mathf.Clamp01(tiempoFlotacion / duracionEntradaFlotacion)
                : 1f;

            float amplitudActual = amplitudFlotacion * factorEntrada;

            Vector3 pos = posicionFinal;
            pos.y += Mathf.Sin(tiempoFlotacion * velocidadFlotacion) * amplitudActual;

            brujula.position = pos;
        }

        // 2. Pulso ultra-fluido tipo "respiración" con Lerp
        if (pulsandoBoton && botonRecibirBrujula != null)
        {
            float objetivo = 1f + Mathf.Sin(Time.time * velocidadPulsoBoton) * amplitudPulsoBoton;
            Vector3 escalaObjetivo = escalaInicialBoton * objetivo;

            botonRecibirBrujula.localScale = Vector3.Lerp(
                botonRecibirBrujula.localScale,
                escalaObjetivo,
                Time.deltaTime * suavizadoPulso
            );
        }
    }
}
