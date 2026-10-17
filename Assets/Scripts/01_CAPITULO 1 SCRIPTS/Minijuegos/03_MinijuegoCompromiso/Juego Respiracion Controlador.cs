using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class RespiracionManager : MonoBehaviour
{
    public MovimientoRespiracion personaje;
    public TMP_Text texto;

    [Header("Ventana de progreso")]
    [Tooltip("Muestra el número de ciclo actual: 1, 2, 3...")]
    public TMP_Text textoCiclo;
    [Tooltip("Muestra el conteo de segundos de la fase actual: 'Inhalar 1 2 3 4'")]
    public TMP_Text textoContador;

    [Header("Barra de progreso")]
    [Tooltip("Slider (Min 0, Max 1) que se llena durante cada fase")]
    public Slider barraProgreso;
    [Tooltip("Qué tanto 'respira' la barra durante SOSTIENE (0 = nada, 0.05 = muy sutil)")]
    public float pulsoAmplitud = 0.04f;
    [Tooltip("Qué tan rápido pulsa durante SOSTIENE (más bajo = más lento)")]
    public float pulsoVelocidad = 0.4f;

    [Header("Fondo")]
    [Tooltip("Arrastra aquí el objeto que tiene el script FondoScroll")]
    public FondoScroll fondo;

    [Header("Ciclo")]
    public int respiraciones = 3;

    [Header("Duraciones (segundos)")]
    public float tiempoInhala = 4f;
    public float tiempoSostiene = 4f;
    public float tiempoExhala = 4f;

    [SerializeField, HideInInspector]
    private bool ritmo444Actualizado;

    [Header("Paneles")]
    public CanvasGroup panelInstrucciones;
    public CanvasGroup panelVictoria;

    [Header("Fade")]
    public CanvasGroup fadeEntrada;
    public CanvasGroup fadeSalida;

    [Header("Configuración")]
    public float duracionFade = 0.8f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoInhalar;
    public AudioClip sonidoExhalar;
    public AudioClip sonidoVictoria;

    bool presionando = false;
    private enum FaseVisual { Ninguna, Inhala, Sostiene, Exhala }
    private FaseVisual faseVisual;
    private float progresoVisual;
    private float escalaVerde;
    private RectTransform botonIndicador;
    private RectTransform checkRespiracion;
    private Vector3 escalaBoton;
    private Texture2D texturaCheck;
    private Sprite spriteCheck;
    private bool juegoIniciado;


    void Awake()
    {
        ActualizarRitmo444();
    }

    void OnValidate()
    {
        ActualizarRitmo444();
    }

    void ActualizarRitmo444()
    {
        if (ritmo444Actualizado) return;

        // Migrar una sola vez los valores 4-7-8 ya serializados en la escena.
        // Cambiar solo los valores iniciales del script no los reemplazaria.
        respiraciones = 3;
        tiempoInhala = 4f;
        tiempoSostiene = 4f;
        tiempoExhala = 4f;
        ritmo444Actualizado = true;
    }

    void Start()
    {
        CrearIndicadorRespiracion();
        StartCoroutine(FadeEntrada());
    }

    void Update()
    {
        LeerInput();
        ActualizarIndicadorRespiracion();
    }

    /// <summary>
    /// Combina mouse y touch con OR (no se pisan entre sí).
    /// Funciona igual en PC (click) y en celular (touch).
    /// </summary>
    void LeerInput()
    {
        bool presionado = false;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            presionado = true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            presionado = true;

        presionando = presionado;
    }

    private void CrearIndicadorRespiracion()
    {
        // Reutiliza el fondo negro real que contiene el texto de la instruccion.
        if (texto == null || texto.transform.parent == null) return;
        botonIndicador = texto.transform.parent as RectTransform;
        if (botonIndicador == null || botonIndicador.GetComponent<UnityEngine.UI.Image>() == null) return;
        escalaBoton = botonIndicador.localScale;
        TextAsset png = Resources.Load<TextAsset>("Capitulo1/CheckRespuesta");
        if (png == null) return;
        texturaCheck = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(texturaCheck, png.bytes, true)) return;
        texturaCheck.wrapMode = TextureWrapMode.Clamp;
        texturaCheck.filterMode = FilterMode.Bilinear;
        spriteCheck = Sprite.Create(texturaCheck, new Rect(0, 0, texturaCheck.width, texturaCheck.height),
            new Vector2(0.5f, 0.5f), 100f);
        GameObject visual = new GameObject("CheckRespiracionAutomatico", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        checkRespiracion = (RectTransform)visual.transform;
        checkRespiracion.SetParent(botonIndicador, false);
        checkRespiracion.anchorMin = Vector2.zero;
        checkRespiracion.anchorMax = Vector2.one;
        checkRespiracion.offsetMin = Vector2.zero;
        checkRespiracion.offsetMax = Vector2.zero;
        checkRespiracion.SetAsFirstSibling();
        var grafico = visual.GetComponent<UnityEngine.UI.Image>();
        grafico.sprite = spriteCheck;
        grafico.raycastTarget = false;
        checkRespiracion.localScale = Vector3.zero;
    }

    private void ActualizarIndicadorRespiracion()
    {
        if (checkRespiracion == null) return;
        float objetivo = 0f;
        if (faseVisual == FaseVisual.Inhala)
        {
            objetivo = presionando ? progresoVisual : 0f;
            texto.text = presionando ? "INHALA · MANTÉN PRESIONADO" : "PRESIONA PARA INHALAR";
        }
        else if (faseVisual == FaseVisual.Sostiene)
        {
            objetivo = presionando ? 1f : 0f;
            texto.text = presionando ? "SOSTÉN · SIGUE PRESIONANDO" : "VUELVE A PRESIONAR";
        }
        else if (faseVisual == FaseVisual.Exhala)
        {
            objetivo = 1f - progresoVisual;
            texto.text = presionando ? "SUELTA PARA EXHALAR" : "EXHALA · SIN PRESIONAR";
        }
        escalaVerde = Mathf.MoveTowards(escalaVerde, objetivo, Time.deltaTime);
        checkRespiracion.localScale = Vector3.one * escalaVerde;
        // El negro invita a pulsar cuando falta mantener el clic o el toque.
        bool invitar = (faseVisual == FaseVisual.Inhala || faseVisual == FaseVisual.Sostiene) && !presionando;
        float pulso = invitar ? 1f + 0.025f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 1.4f) : 1f;
        botonIndicador.localScale = escalaBoton * pulso;
    }

    private void OnDisable()
    {
        faseVisual = FaseVisual.Ninguna;
        escalaVerde = 0f;
        if (checkRespiracion != null) checkRespiracion.localScale = Vector3.zero;
        if (botonIndicador != null) botonIndicador.localScale = escalaBoton;
    }

    private void OnDestroy()
    {
        if (checkRespiracion != null) Destroy(checkRespiracion.gameObject);
        if (spriteCheck != null) Destroy(spriteCheck);
        if (texturaCheck != null) Destroy(texturaCheck);
    }

    IEnumerator Secuencia()
    {
        texto.text = "";

        yield return personaje.EntrarEscena();

        // El fondo empieza a moverse apenas el personaje ya está en escena
        // y se queda moviéndose hasta que termine todo el minijuego.
        if (fondo != null)
            fondo.IniciarScroll();

        int hechas = 0;

        while (hechas < respiraciones)
        {
            if (textoCiclo != null) textoCiclo.text = (hechas + 1) + "/" + respiraciones;

            // -------------------------
            // INHALA
            // -------------------------
            faseVisual = FaseVisual.Inhala;
            progresoVisual = 0f;
            escalaVerde = 0f;
            texto.text = "PRESIONA PARA INHALAR";
            if (barraProgreso != null) barraProgreso.value = 0f;

            if (audioSource != null && sonidoInhalar != null)
                audioSource.PlayOneShot(sonidoInhalar);

            yield return personaje.Subir(tiempoInhala, () => presionando, p =>
            {
                if (barraProgreso != null) barraProgreso.value = p;
                progresoVisual = p;
                ActualizarContador("INHALAR", p, tiempoInhala);
            });

            personaje.EmpezarIdle();

            // -------------------------
            // SOSTIENE
            // -------------------------
            yield return Sostener();

            // -------------------------
            // EXHALA
            // -------------------------
            personaje.DetenerIdle();
            faseVisual = FaseVisual.Exhala;
            progresoVisual = 0f;
            texto.text = "SUELTA PARA EXHALAR";

            while (presionando)
                yield return null;

            if (audioSource != null && sonidoExhalar != null)
                audioSource.PlayOneShot(sonidoExhalar);

            yield return personaje.Bajar(tiempoExhala, p =>
            {
                // p va de 0 a 1 mientras baja; la barra debe ir de llena (1) a vacía (0)
                if (barraProgreso != null) barraProgreso.value = 1f - p;
                progresoVisual = p;
                ActualizarContador("EXHALAR", p, tiempoExhala);
            }, () => !presionando);

            hechas++;
        }

        texto.text = "";

        if (fondo != null)
            fondo.DetenerScroll();

        faseVisual = FaseVisual.Ninguna;
        MostrarVictoria();
    }

    IEnumerator Sostener()
    {
        faseVisual = FaseVisual.Sostiene;
        texto.text = "SOSTENER PRESIONADO";

        float t = 0f;

        while (t < tiempoSostiene)
        {
            if (!presionando)
            {
                texto.text = "MANTÉN PRESIONADO";

                while (!presionando)
                    yield return null;

                texto.text = "SOSTENER PRESIONADO";
            }

            t += Time.deltaTime;

            if (barraProgreso != null)
            {
                // Se queda llena y "respira" muy lento y poco (oscila entre ~0.96 y 1.0)
                float pulso = 1f - pulsoAmplitud * (0.5f * (1f + Mathf.Sin(Time.time * pulsoVelocidad * Mathf.PI * 2f)));
                barraProgreso.value = pulso;
            }

            ActualizarContador("MANTENER", t / tiempoSostiene, tiempoSostiene);
            yield return null;
        }
    }

    /// <summary>
    /// Muestra "INHALAR 1", luego "INHALAR 2", etc., cambiando cada segundo
    /// de acuerdo al progreso de la fase actual.
    /// </summary>
    void ActualizarContador(string etiqueta, float progreso01, float duracionFase)
    {
        if (textoContador == null) return;

        int totalSegundos = Mathf.Max(1, Mathf.RoundToInt(duracionFase));
        int segundoActual = Mathf.Clamp(Mathf.FloorToInt(progreso01 * totalSegundos) + 1, 1, totalSegundos);

        textoContador.text = segundoActual + "\n" + etiqueta;
    }

    // PASO 3: Fade de entrada
    IEnumerator FadeEntrada()
    {
        fadeEntrada.gameObject.SetActive(true);

        fadeEntrada.alpha = 1;
        fadeEntrada.blocksRaycasts = true;

        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;

            fadeEntrada.alpha = Mathf.Lerp(1, 0, t / duracionFade);

            yield return null;
        }

        fadeEntrada.alpha = 0;
        fadeEntrada.blocksRaycasts = false;
        fadeEntrada.gameObject.SetActive(false);

        panelInstrucciones.gameObject.SetActive(true);

        panelInstrucciones.alpha = 1;
        panelInstrucciones.interactable = true;
        panelInstrucciones.blocksRaycasts = true;
    }

    // PASO 4: Botón "Comenzar"
    public void ComenzarJuego()
    {
        if (juegoIniciado) return;
        juegoIniciado = true;
        StartCoroutine(CerrarPanel());
    }

    IEnumerator CerrarPanel()
    {
        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;

            panelInstrucciones.alpha = Mathf.Lerp(1, 0, t / duracionFade);

            yield return null;
        }

        panelInstrucciones.alpha = 0;
        panelInstrucciones.blocksRaycasts = false;
        panelInstrucciones.interactable = false;
        panelInstrucciones.gameObject.SetActive(false);

        StartCoroutine(Secuencia());
    }

    // PASO 6: Mostrar panel WIN
    public void MostrarVictoria()
    {
        StartCoroutine(AparecerVictoria());
    }

    IEnumerator AparecerVictoria()
    {
        panelVictoria.gameObject.SetActive(true);

        panelVictoria.alpha = 0;

        if (audioSource != null && sonidoVictoria != null)
            audioSource.PlayOneShot(sonidoVictoria);

        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;

            panelVictoria.alpha = Mathf.Lerp(0, 1, t / duracionFade);

            yield return null;
        }

        panelVictoria.alpha = 1;
        panelVictoria.blocksRaycasts = true;
        panelVictoria.interactable = true;
    }

    // PASO 7: Botón "Desbloquear puerta"
    public void DesbloquearPuerta()
    {
        StartCoroutine(FadeSalida());
    }

    IEnumerator FadeSalida()
    {
        fadeSalida.gameObject.SetActive(true);

        fadeSalida.alpha = 0;
        fadeSalida.blocksRaycasts = true;

        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;

            fadeSalida.alpha = Mathf.Lerp(0, 1, t / duracionFade);

            yield return null;
        }

        PlayerPrefs.SetInt("EstadoCompromiso", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("02_Capitulo_1");
    }
}