using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Capitulo2
{
    /// <summary>
    /// Gestiona la Cinemática Final del Capítulo 2 (Parte 2 - Broche de oro):
    /// 1. Transición de cámara con fade hacia la cámara final enfocada en el jugador.
    /// 2. El avatar del jugador (Hombre o Mujer) celebra con una animación de baile (ej. "Excited").
    /// 3. Fade in del panel de victoria/resumen final.
    /// 4. POP secuencial de las 3 estrellas de maestría con sonido y efecto elástico.
    /// 5. POP/Fade in secuencial de los 4 logros del Capítulo 2 (uno por cada estación).
    /// 6. Aparición suave del botón para reiniciar progreso y volver al menú principal.
    /// </summary>
    public class CinematicaFinalCapitulo2 : MonoBehaviour
    {
        public static CinematicaFinalCapitulo2 Instance { get; private set; }

        [Header("--- Personajes del Jugador ---")]
        [Tooltip("Modelo o prefab del avatar masculino en la escena")]
        [SerializeField] private GameObject hombre;
        [Tooltip("Modelo o prefab del avatar femenino en la escena")]
        [SerializeField] private GameObject mujer;
        private Animator animatorJugador;

        [Header("--- Cámaras ---")]
        [Tooltip("Cámara activa durante el juego o diálogo de bienvenida")]
        [SerializeField] private GameObject camaraJuego;
        [Tooltip("Cámara cinemática final que enfoca al jugador bailando/celebrando")]
        [SerializeField] private GameObject camaraFinal;

        [Header("--- Fade de Cámara ---")]
        [Tooltip("Imagen o panel negro para la transición entre cámaras")]
        [SerializeField] private Image panelFadeCamara;
        [SerializeField] private CanvasGroup canvasGroupFadeCamara;
        [SerializeField] private float duracionFadeCamara = 1.0f;

        [Header("--- Panel Final UI ---")]
        [Tooltip("Panel principal que contiene las estrellas, logros y botón de reinicio")]
        [SerializeField] private GameObject panelFinal;
        [SerializeField] private float duracionFadePanelFinal = 1.5f;

        [Header("--- Estrellas (3 en orden) ---")]
        [Tooltip("Arrastrar las 3 estrellas en orden de izquierda a derecha")]
        [SerializeField] private RectTransform[] estrellas;
        [SerializeField] private float duracionPopEstrella = 0.5f;
        [SerializeField] private float pausaEntreEstrellas = 0.35f;
        [SerializeField] private float pausaAntesDeEstrellas = 0.4f;

        [Header("--- Logros del Capítulo 2 (4 en orden) ---")]
        [Tooltip("Arrastrar los 4 logros (uno por cada estación del Cap 2)")]
        [SerializeField] private RectTransform[] logros;
        [SerializeField] private float duracionPopLogro = 0.5f;
        [SerializeField] private float pausaEntreLogros = 0.4f;
        [SerializeField] private float pausaAntesDeLogros = 0.65f;

        [Header("--- Botón Reiniciar / Volver al Menú ---")]
        [Tooltip("Botón para borrar el progreso y volver al menú de inicio")]
        [SerializeField] private Button botonReiniciar;
        [SerializeField] private float duracionFadeBoton = 0.6f;
        [SerializeField] private float pausaAntesDelBoton = 0.4f;
        [SerializeField] private string nombreEscenaMenu = "00_MenuJuego";

        [Header("--- Animación de Celebración ---")]
        [Tooltip("Nombre del estado de animación de baile o emoción en el Animator")]
        [SerializeField] private string nombreAnimacion = "Excited";
        [Tooltip("Tiempo en segundos que el jugador baila antes de mostrar el panel")]
        [SerializeField] private float duracionBaile = 3f;
        [SerializeField] private float esperaAntesDelCambioCamara = 0.6f;

        [Header("--- Audio y Efectos Sonoros ---")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Fanfarria o música que suena al iniciar la cinemática de victoria")]
        [SerializeField] private AudioClip sonidoCinematica;
        [Tooltip("Efecto de sonido al aparecer cada estrella")]
        [SerializeField] private AudioClip sonidoPopEstrella;
        [Tooltip("Efecto de sonido al aparecer cada logro")]
        [SerializeField] private AudioClip sonidoPopLogro;
        [Tooltip("Música de fondo del menú principal")]
        [SerializeField] private AudioClip musicaMenu;

        [Header("--- Debug / Pruebas ---")]
        [Tooltip("Si se activa, inicia la cinemática inmediatamente al dar Play para probarla")]
        [SerializeField] private bool probarCinematicaAlIniciar = false;

        private CanvasGroup canvasPanelFinal;
        private bool cinematicaIniciada = false;

        // Diccionarios para preservar fielmente el diseño exacto del Canvas en el Editor
        private System.Collections.Generic.Dictionary<RectTransform, Vector3> escalasOriginales = new System.Collections.Generic.Dictionary<RectTransform, Vector3>();
        private System.Collections.Generic.Dictionary<RectTransform, Vector2> posicionesOriginales = new System.Collections.Generic.Dictionary<RectTransform, Vector2>();
        private System.Collections.Generic.Dictionary<RectTransform, Vector2> tamanosOriginales = new System.Collections.Generic.Dictionary<RectTransform, Vector2>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            AutoDetectarReferenciasSiFaltan();
            GuardarTransformacionesOriginales();
            PrepararEstadoInicial();
        }

        private void Start()
        {
            AutoDetectarReferenciasSiFaltan();
            GuardarTransformacionesOriginales();

            if (botonReiniciar != null)
            {
                botonReiniciar.onClick.RemoveListener(VolverAlInicio);
                botonReiniciar.onClick.AddListener(VolverAlInicio);
            }

            if (probarCinematicaAlIniciar)
            {
                Iniciar();
            }
        }

        private void GuardarTransformacionesOriginales()
        {
            if (estrellas != null)
            {
                foreach (RectTransform e in estrellas)
                {
                    if (e != null && !escalasOriginales.ContainsKey(e))
                    {
                        Vector3 s = e.localScale;
                        if (s == Vector3.zero || s.x <= 0.01f) s = Vector3.one;
                        escalasOriginales[e] = s;
                        posicionesOriginales[e] = e.anchoredPosition;
                        tamanosOriginales[e] = e.sizeDelta;
                    }
                }
            }

            if (logros != null)
            {
                foreach (RectTransform l in logros)
                {
                    if (l != null && !escalasOriginales.ContainsKey(l))
                    {
                        Vector3 s = l.localScale;
                        if (s == Vector3.zero || s.x <= 0.01f) s = Vector3.one;
                        escalasOriginales[l] = s;
                        posicionesOriginales[l] = l.anchoredPosition;
                        tamanosOriginales[l] = l.sizeDelta;
                    }
                }
            }
        }

        /// <summary>
        /// Oculta y resetea todos los elementos para que no bloqueen raycasts ni se vean antes de tiempo.
        /// </summary>
        private void PrepararEstadoInicial()
        {
            if (camaraFinal != null)
            {
                camaraFinal.SetActive(false);
            }

            // Fade de cámara apagado
            if (panelFadeCamara != null)
            {
                panelFadeCamara.gameObject.SetActive(false);
                Color c = panelFadeCamara.color;
                c.a = 0f;
                panelFadeCamara.color = c;
            }
            if (canvasGroupFadeCamara != null)
            {
                canvasGroupFadeCamara.alpha = 0f;
                canvasGroupFadeCamara.blocksRaycasts = false;
                canvasGroupFadeCamara.gameObject.SetActive(false);
            }

            // Panel final oculto
            if (panelFinal != null)
            {
                canvasPanelFinal = ObtenerOAgregarCanvasGroup(panelFinal);
                canvasPanelFinal.alpha = 0f;
                canvasPanelFinal.interactable = false;
                canvasPanelFinal.blocksRaycasts = false;
                panelFinal.SetActive(false);
            }

            // Estrellas ocultas con escala 0
            if (estrellas != null)
            {
                foreach (RectTransform estrella in estrellas)
                {
                    if (estrella != null)
                    {
                        estrella.localScale = Vector3.zero;
                        estrella.gameObject.SetActive(false);
                    }
                }
            }

            // Logros ocultos con alpha 0 pero respetando su escala, tamaño y posición originales de diseño
            if (logros != null)
            {
                foreach (RectTransform logro in logros)
                {
                    if (logro != null)
                    {
                        CanvasGroup cg = ObtenerOAgregarCanvasGroup(logro.gameObject);
                        if (cg != null)
                        {
                            cg.alpha = 0f;
                            cg.interactable = false;
                            cg.blocksRaycasts = false;
                        }

                        // Restaurar escala y posición original exactas para que no se deformen en el layout
                        if (escalasOriginales.TryGetValue(logro, out Vector3 escOrig))
                        {
                            logro.localScale = escOrig;
                        }
                        if (posicionesOriginales.TryGetValue(logro, out Vector2 posOrig))
                        {
                            logro.anchoredPosition = posOrig;
                        }
                        if (tamanosOriginales.TryGetValue(logro, out Vector2 tamOrig) && tamOrig != Vector2.zero)
                        {
                            logro.sizeDelta = tamOrig;
                        }

                        logro.gameObject.SetActive(true);
                    }
                }
            }

            // Botón reiniciar oculto
            if (botonReiniciar != null)
            {
                CanvasGroup cgBoton = ObtenerOAgregarCanvasGroup(botonReiniciar.gameObject);
                if (cgBoton != null)
                {
                    cgBoton.alpha = 0f;
                    cgBoton.interactable = false;
                    cgBoton.blocksRaycasts = false;
                }
                botonReiniciar.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Busca automáticamente todas las referencias que falten en el Inspector.
        /// </summary>
        private void AutoDetectarReferenciasSiFaltan()
        {
            int avatar = PlayerPrefs.GetInt("Avatar", PlayerPrefs.GetInt("AvatarElegido", 1));

            // 1. Detectar avatares
            if (hombre == null)
            {
                GameObject h = GameObject.Find("Avatar_Hombre") ?? GameObject.Find("Hombre") ?? GameObject.Find("JugadorHombre");
                if (h != null) hombre = h;
            }
            if (mujer == null)
            {
                GameObject m = GameObject.Find("Avatar_Mujer") ?? GameObject.Find("Mujer") ?? GameObject.Find("JugadorMujer");
                if (m != null) mujer = m;
            }

            GameObject jugadorActivo = (avatar == 1) ? hombre : mujer;
            if (jugadorActivo != null)
            {
                animatorJugador = jugadorActivo.GetComponent<Animator>();
            }

            if (animatorJugador == null)
            {
                Animator[] anims = Resources.FindObjectsOfTypeAll<Animator>();
                foreach (Animator a in anims)
                {
                    if (a.hideFlags != HideFlags.None || !a.gameObject.scene.isLoaded) continue;
                    string n = a.gameObject.name.ToLower();
                    if (!n.Contains("guia") && !n.Contains("clinico") && (n.Contains("avatar") || n.Contains("hombre") || n.Contains("mujer") || n.Contains("player") || n.Contains("jugador")))
                    {
                        animatorJugador = a;
                        break;
                    }
                }
            }

            // 2. Detectar cámaras
            if (camaraJuego == null && Camera.main != null)
            {
                camaraJuego = Camera.main.gameObject;
            }

            if (camaraFinal == null)
            {
                camaraFinal = GameObject.Find("CamaraFinal") ?? GameObject.Find("CamaraCinematica") ?? GameObject.Find("CamaraVictoria");
                if (camaraFinal == null)
                {
                    Camera[] cams = Resources.FindObjectsOfTypeAll<Camera>();
                    foreach (Camera c in cams)
                    {
                        if (c.hideFlags != HideFlags.None || !c.gameObject.scene.isLoaded) continue;
                        string n = c.name.ToLower();
                        if (n.Contains("final") || n.Contains("cinematica") || n.Contains("victoria"))
                        {
                            camaraFinal = c.gameObject;
                            break;
                        }
                    }
                }
            }

            // 3. AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
            }

            // 4. Panel Final y componentes internos
            if (panelFinal == null)
            {
                Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (Canvas cv in canvases)
                {
                    if (cv.hideFlags != HideFlags.None || !cv.gameObject.scene.isLoaded) continue;
                    Transform tf = cv.transform.Find("PanelFinal") ?? cv.transform.Find("Panel Victoria") ?? cv.transform.Find("PanelFin");
                    if (tf != null)
                    {
                        panelFinal = tf.gameObject;
                        break;
                    }
                }
            }

            if (panelFinal != null)
            {
                // Detectar estrellas si el arreglo está vacío
                if (estrellas == null || estrellas.Length == 0)
                {
                    RectTransform[] rects = panelFinal.GetComponentsInChildren<RectTransform>(true);
                    System.Collections.Generic.List<RectTransform> listaEstrellas = new System.Collections.Generic.List<RectTransform>();
                    foreach (RectTransform r in rects)
                    {
                        if (r.gameObject == panelFinal) continue;
                        if (r.GetComponent<LayoutGroup>() != null) continue;
                        string n = r.name.ToLower();
                        if (n.Contains("contenedor") || n.Contains("grid") || n.Contains("holder") || n.Contains("group") || n.Contains("panel")) continue;
                        if (n.Contains("estrella") || n.Contains("star"))
                        {
                            listaEstrellas.Add(r);
                        }
                    }
                    listaEstrellas.RemoveAll(padre => listaEstrellas.Exists(hijo => hijo != padre && hijo.IsChildOf(padre)));

                    if (listaEstrellas.Count >= 3)
                    {
                        estrellas = listaEstrellas.ToArray();
                    }
                }

                // Detectar logros si el arreglo está vacío
                if (logros == null || logros.Length == 0)
                {
                    RectTransform[] rects = panelFinal.GetComponentsInChildren<RectTransform>(true);
                    System.Collections.Generic.List<RectTransform> listaLogros = new System.Collections.Generic.List<RectTransform>();
                    foreach (RectTransform r in rects)
                    {
                        if (r.gameObject == panelFinal) continue;
                        if (r.GetComponent<LayoutGroup>() != null) continue;
                        string n = r.name.ToLower();
                        if (n.Contains("contenedor") || n.Contains("grid") || n.Contains("holder") || n.Contains("group") || n.Contains("panel")) continue;
                        if (n.Contains("logro") || n.Contains("medalla") || n.Contains("estacion") || n.Contains("insignia"))
                        {
                            listaLogros.Add(r);
                        }
                    }
                    listaLogros.RemoveAll(padre => listaLogros.Exists(hijo => hijo != padre && hijo.IsChildOf(padre)));

                    if (listaLogros.Count >= 4)
                    {
                        logros = listaLogros.ToArray();
                    }
                }

                // Detectar botón reiniciar si falta
                if (botonReiniciar == null)
                {
                    Button[] btns = panelFinal.GetComponentsInChildren<Button>(true);
                    foreach (Button b in btns)
                    {
                        string n = b.name.ToLower();
                        if (n.Contains("reiniciar") || n.Contains("menu") || n.Contains("inicio") || n.Contains("comenzar"))
                        {
                            botonReiniciar = b;
                            break;
                        }
                    }
                    if (botonReiniciar == null && btns.Length > 0)
                    {
                        botonReiniciar = btns[0];
                    }
                }
            }
        }

        /// <summary>
        /// Inicia la secuencia completa de la cinemática final.
        /// </summary>
        public void Iniciar()
        {
            if (cinematicaIniciada) return;
            cinematicaIniciada = true;

            gameObject.SetActive(true);
            ActivarPadresHastaCanvas(gameObject);
            AutoDetectarReferenciasSiFaltan();

            if (audioSource != null && sonidoCinematica != null)
            {
                audioSource.PlayOneShot(sonidoCinematica);
            }

            StopAllCoroutines();
            StartCoroutine(SecuenciaFinal());
        }

        private IEnumerator SecuenciaFinal()
        {
            // Acorta un poco la espera tras el último diálogo, conservando una pausa de salida.
            yield return new WaitForSeconds(Mathf.Max(0f, esperaAntesDelCambioCamara - 0.2f));

            // 1. Cambio de cámara suave con Fade a Negro
            yield return StartCoroutine(CambiarCamaraConFade());

            yield return new WaitForSeconds(duracionBaile);

            // 3. Secuencia de UI: Panel -> Estrellas -> Logros -> Botón
            yield return StartCoroutine(SecuenciaPanelFinal());
        }

        private IEnumerator CambiarCamaraConFade()
        {
            // Hace el fundido ligeramente más ágil sin perder suavidad.
            float duracionTransicion = Mathf.Max(0.1f, duracionFadeCamara * 0.9f);

            // Activar y hacer fade in a negro
            yield return StartCoroutine(EjecutarFadePantalla(0f, 1f, duracionTransicion));

            // Activar y validar primero la cámara final; conserva la de juego si la referencia falla.
            Camera camaraFinalActiva = camaraFinal != null ? CamarasCapitulo2.Activar(camaraFinal) : null;
            if (camaraFinalActiva != null)
            {
                camaraFinalActiva.enabled = true;
                if (camaraJuego != null)
                    camaraJuego.SetActive(false);
            }
            else
            {
                Debug.LogError("CinematicaFinalCapitulo2: No se pudo activar la cámara final. Se conserva la cámara de juego.", this);
                if (camaraJuego != null)
                    CamarasCapitulo2.Activar(camaraJuego);
            }
            // El baile empieza con la pantalla aún negra para que aparezca ya en movimiento.
            if (animatorJugador != null)
            {
                animatorJugador.applyRootMotion = false;
                animatorJugador.CrossFade(nombreAnimacion, 0.15f);
            }

            yield return new WaitForSeconds(0.2f);

            // Fade out de negro y revelado del jugador bailando.
            yield return StartCoroutine(EjecutarFadePantalla(1f, 0f, duracionTransicion));
        }

        private IEnumerator EjecutarFadePantalla(float alphaInicio, float alphaFin, float duracion)
        {
            if (panelFadeCamara != null)
            {
                panelFadeCamara.transform.SetAsLastSibling();
                panelFadeCamara.gameObject.SetActive(true);
                if (canvasGroupFadeCamara != null && canvasGroupFadeCamara.gameObject == panelFadeCamara.gameObject)
                {
                    canvasGroupFadeCamara.alpha = 1f;
                    canvasGroupFadeCamara.blocksRaycasts = (alphaFin > 0.5f);
                }
                float t = 0f;
                Color c = panelFadeCamara.color;

                while (t < duracion)
                {
                    t += Time.deltaTime;
                    c.a = Mathf.Lerp(alphaInicio, alphaFin, t / duracion);
                    panelFadeCamara.color = c;
                    yield return null;
                }

                c.a = alphaFin;
                panelFadeCamara.color = c;

                if (alphaFin <= 0.01f)
                {
                    if (canvasGroupFadeCamara != null && canvasGroupFadeCamara.gameObject == panelFadeCamara.gameObject)
                    {
                        canvasGroupFadeCamara.alpha = 0f;
                        canvasGroupFadeCamara.blocksRaycasts = false;
                    }
                    panelFadeCamara.gameObject.SetActive(false);
                }
            }
            else if (canvasGroupFadeCamara != null)
            {
                canvasGroupFadeCamara.transform.SetAsLastSibling();
                canvasGroupFadeCamara.gameObject.SetActive(true);
                canvasGroupFadeCamara.blocksRaycasts = (alphaFin > 0.5f);
                float t = 0f;

                while (t < duracion)
                {
                    t += Time.deltaTime;
                    canvasGroupFadeCamara.alpha = Mathf.Lerp(alphaInicio, alphaFin, t / duracion);
                    yield return null;
                }

                canvasGroupFadeCamara.alpha = alphaFin;
                if (alphaFin <= 0.01f)
                {
                    canvasGroupFadeCamara.blocksRaycasts = false;
                    canvasGroupFadeCamara.gameObject.SetActive(false);
                }
            }
            else
            {
                yield return new WaitForSeconds(duracion);
            }
        }

        private IEnumerator SecuenciaPanelFinal()
        {
            // 1. Fade del panel base (UI general)
            yield return StartCoroutine(MostrarPanelBase());

            // 2. Pop de las 3 estrellas, una por una con rebote elástico
            yield return new WaitForSeconds(pausaAntesDeEstrellas);

            if (estrellas != null)
            {
                foreach (RectTransform estrella in estrellas)
                {
                    if (estrella != null)
                    {
                        yield return StartCoroutine(PopIn(estrella, duracionPopEstrella, sonidoPopEstrella != null ? sonidoPopEstrella : sonidoPopLogro));
                    }
                    yield return new WaitForSeconds(pausaEntreEstrellas);
                }
            }

            // 3. Fade-in suave de los 4 logros del Capítulo 2
            yield return new WaitForSeconds(pausaAntesDeLogros);

            if (logros != null)
            {
                foreach (RectTransform logro in logros)
                {
                    if (logro != null)
                    {
                        yield return StartCoroutine(FadeInSuaveLogro(logro, duracionPopLogro));
                    }
                    yield return new WaitForSeconds(pausaEntreLogros);
                }
            }

            // 4. Fade del botón "Comenzar de nuevo" / "Reiniciar Progreso"
            yield return new WaitForSeconds(pausaAntesDelBoton);
            yield return StartCoroutine(MostrarBotonReiniciar());
        }

        private IEnumerator MostrarPanelBase()
        {
            if (panelFinal == null) yield break;

            ActivarPadresHastaCanvas(panelFinal);
            panelFinal.SetActive(true);
            panelFinal.transform.SetAsLastSibling();

            if (canvasPanelFinal == null)
            {
                canvasPanelFinal = ObtenerOAgregarCanvasGroup(panelFinal);
            }

            canvasPanelFinal.alpha = 0f;
            canvasPanelFinal.interactable = false;
            canvasPanelFinal.blocksRaycasts = false;

            float t = 0f;
            while (t < duracionFadePanelFinal)
            {
                t += Time.deltaTime;
                canvasPanelFinal.alpha = Mathf.Lerp(0f, 1f, t / duracionFadePanelFinal);
                yield return null;
            }

            canvasPanelFinal.alpha = 1f;
            canvasPanelFinal.interactable = true;
            canvasPanelFinal.blocksRaycasts = true;
        }

        /// <summary>
        /// Animación tipo "pop" con rebote elástico suave respetando la escala original de diseño
        /// </summary>
        private IEnumerator PopIn(RectTransform objeto, float duracion, AudioClip clipAudio = null)
        {
            if (objeto == null) yield break;

            if (audioSource != null && clipAudio != null)
            {
                audioSource.PlayOneShot(clipAudio);
            }

            ActivarPadresHastaCanvas(objeto.gameObject);
            objeto.gameObject.SetActive(true);

            Vector3 escalaBase = Vector3.one;
            if (escalasOriginales.TryGetValue(objeto, out Vector3 esc))
            {
                escalaBase = esc;
            }
            else if (objeto.localScale != Vector3.zero && objeto.localScale.x > 0.05f)
            {
                escalaBase = objeto.localScale;
            }

            if (posicionesOriginales.TryGetValue(objeto, out Vector2 pos))
            {
                objeto.anchoredPosition = pos;
            }

            objeto.localScale = Vector3.zero;

            float sobrepasoEscala = 1.15f;
            float mitad = duracion * 0.6f;
            float resto = duracion - mitad;

            float t = 0f;
            while (t < mitad)
            {
                t += Time.deltaTime;
                float progreso = Mathf.Clamp01(t / mitad);
                objeto.localScale = escalaBase * Mathf.Lerp(0f, sobrepasoEscala, progreso);
                yield return null;
            }

            t = 0f;
            while (t < resto)
            {
                t += Time.deltaTime;
                float progreso = Mathf.Clamp01(t / resto);
                objeto.localScale = escalaBase * Mathf.Lerp(sobrepasoEscala, 1f, progreso);
                yield return null;
            }

            objeto.localScale = escalaBase;
        }

        /// <summary>Fade-in suave para los logros, conservando su escala y posición de diseño.</summary>
        private IEnumerator FadeInSuaveLogro(RectTransform objeto, float duracion)
        {
            if (objeto == null) yield break;

            ActivarPadresHastaCanvas(objeto.gameObject);
            objeto.gameObject.SetActive(true);

            CanvasGroup cg = ObtenerOAgregarCanvasGroup(objeto.gameObject);
            cg.alpha = 0f;
            cg.blocksRaycasts = true;

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                cg.alpha = p;
                yield return null;
            }

            cg.alpha = 1f;

            if (objeto.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(objeto.parent as RectTransform);
            }
        }

        private IEnumerator MostrarBotonReiniciar()
        {
            if (botonReiniciar == null) yield break;

            ActivarPadresHastaCanvas(botonReiniciar.gameObject);
            botonReiniciar.gameObject.SetActive(true);

            CanvasGroup cgBoton = ObtenerOAgregarCanvasGroup(botonReiniciar.gameObject);
            cgBoton.alpha = 0f;
            cgBoton.interactable = false;
            cgBoton.blocksRaycasts = false;

            float t = 0f;
            while (t < duracionFadeBoton)
            {
                t += Time.deltaTime;
                cgBoton.alpha = Mathf.Lerp(0f, 1f, t / duracionFadeBoton);
                yield return null;
            }

            cgBoton.alpha = 1f;
            cgBoton.interactable = true;
            cgBoton.blocksRaycasts = true;

            // Iniciar suave latido en el botón para invitar al clic
            StartCoroutine(AnimarLatidoBotonReiniciar(botonReiniciar.gameObject));
        }

        private IEnumerator AnimarLatidoBotonReiniciar(GameObject btnObj)
        {
            if (btnObj == null) yield break;
            Vector3 original = btnObj.transform.localScale;
            if (original == Vector3.zero || original.x < 0.1f) original = Vector3.one;

            float tiempo = 0f;
            while (btnObj != null && btnObj.activeSelf)
            {
                tiempo += Time.deltaTime * 3.5f;
                float pulso = 1f + Mathf.Sin(tiempo) * 0.035f;
                btnObj.transform.localScale = original * pulso;
                yield return null;
            }
        }

        /// <summary>
        /// Borra todos los PlayerPrefs, cambia a la música del menú y carga la escena principal.
        /// </summary>
        public void VolverAlInicio()
        {
            Debug.Log("CinematicaFinalCapitulo2: Reiniciando progreso y volviendo a " + nombreEscenaMenu);

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Intento seguro de cambiar música usando reflexión para máxima compatibilidad
            try
            {
                var musicManagerType = System.Type.GetType("MusicManager") ?? System.Type.GetType("MusicManager, Assembly-CSharp");
                if (musicManagerType != null && musicaMenu != null)
                {
                    var propInstance = musicManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var instance = propInstance != null ? propInstance.GetValue(null) : null;
                    if (instance != null)
                    {
                        var methodCambiar = musicManagerType.GetMethod("CambiarMusica");
                        if (methodCambiar != null)
                        {
                            methodCambiar.Invoke(instance, new object[] { musicaMenu });
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("CinematicaFinalCapitulo2: Error opcional en MusicManager: " + ex.Message);
            }

            SceneManager.LoadScene(nombreEscenaMenu);
        }

        private CanvasGroup ObtenerOAgregarCanvasGroup(GameObject obj)
        {
            if (obj == null) return null;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            return cg;
        }

        private void ActivarPadresHastaCanvas(GameObject obj)
        {
            if (obj == null) return;
            Transform actual = obj.transform;
            while (actual != null)
            {
                actual.gameObject.SetActive(true);
                if (actual.GetComponent<Canvas>() != null) break;
                actual = actual.parent;
            }
        }
    }
}
