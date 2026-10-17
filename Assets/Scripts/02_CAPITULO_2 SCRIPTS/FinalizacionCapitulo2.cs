using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

namespace Capitulo2
{
    /// <summary>
    /// Gestiona la primera parte de la finalización del Capítulo 2:
    /// 1. Detección de las 4 estaciones completadas en cualquier orden.
    /// 2. Retorno con fade a negro al punto de inicio (donde ocurrió la bienvenida).
    /// 3. El Guía aplaude y felicita al jugador.
    /// 4. Efecto POP del Objeto 3D en escena + Luz + Guía vuelve a aplaudir.
    /// 5. Panel de logro "Lente de la Interpretación" con fade in, botón "Recibir Lente" y fade out.
    /// 6. Diálogo de despedida e inspiración sobre la interpretación vs daño real.
    /// </summary>
    public class FinalizacionCapitulo2 : MonoBehaviour
    {
        public static FinalizacionCapitulo2 Instance { get; private set; }

        [Header("--- Cámaras y Puntos de Retorno ---")]
        [Tooltip("Cámara inicial frente al Guía (la que se usó en el diálogo de bienvenida)")]
        [SerializeField] private GameObject camaraGameplay;
        [Tooltip("Punto (GameObject / Transform) donde se posicionará y rotará la Cámara sí o sí al regresar tras completar las 4 estaciones")]
        [SerializeField] private Transform puntoRetornoCamara;
        [Tooltip("Punto donde debe ubicarse el Guía al regresar")]
        [SerializeField] private Transform puntoRetornoGuia;
        [Tooltip("Punto donde debe ubicarse el Jugador al regresar")]
        [SerializeField] private Transform puntoRetornoJugador;

        public Transform PuntoRetornoCamara => puntoRetornoCamara;

        [Header("--- Personajes y Animaciones ---")]
        [SerializeField] private Animator guiaAnimator;
        [SerializeField] private string parametroHablando = "Hablando";
        [SerializeField] private string estadoAplaudir = "Clapping";
        [SerializeField] private string estadoIdle = "Idle";
        [SerializeField] private float duracionAplauso = 2.5f;

        [Header("--- Avatares del Jugador (Hombre / Mujer) ---")]
        [SerializeField] private GameObject jugadorHombre;
        [SerializeField] private GameObject jugadorMujer;

        [Header("--- Objeto 3D del Premio & Efectos ---")]
        [Tooltip("Modelo 3D del premio que aparece en la escena con efecto POP")]
        [SerializeField] private GameObject objeto3DPremio;
        [Tooltip("Luz dedicada que ilumina el objeto 3D al aparecer")]
        [SerializeField] private Light luzPremio;
        [Tooltip("Intensidad máxima deseada para la luz del premio")]
        [SerializeField] private float intensidadLuzPremio = 3f;
        [Tooltip("Escala final del modelo 3D en la escena")]
        [SerializeField] private Vector3 escalaPremio3D = Vector3.one;
        [Tooltip("Si está activo, el objeto 3D gira suavemente sobre su eje mientras se exhibe")]
        [SerializeField] private bool girarObjetoPremio = true;
        [SerializeField] private float velocidadGiroY = 35f;

        [Header("--- Panel de Logro UI ('LENTE DE LA INTERPRETACIÓN') ---")]
        [Tooltip("Panel UI del logro ganado")]
        [SerializeField] private GameObject panelLogroPremio;
        [Tooltip("Botón para recibir el lente y continuar")]
        [SerializeField] private Button botonRecibirLente;
        [Tooltip("Objeto o botón en estado verde que hace POP al pulsar 'Recibir Lente'")]
        [SerializeField] private GameObject botonVerdeRecibirLente;
        [SerializeField] private float duracionFadePanelLogro = 0.8f;
        [Tooltip("Velocidad de la animación de aplauso del guía (más calmado y natural)")]
        [Range(0.4f, 1.2f)] [SerializeField] private float velocidadAnimAplauso = 0.8f;

        [Header("--- UI de Diálogos del Guía ('DIALOGOS FIN') ---")]
        [Tooltip("(Opcional) Panel o contenedor padre de los diálogos finales (ej. 'DIALOGOS FIN'). Se activa solo aunque esté desactivado en el Inspector")]
        [SerializeField] private GameObject contenedorDialogosFin;
        [SerializeField] private GameObject panelDialogo;
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuar;
        [SerializeField] private float velocidadEscritura = 0.055f;

        [Header("--- Fade a Negro de Pantalla ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        [SerializeField] private float duracionFadeNegro = 0.7f;

        [Header("--- Eventos ---")]
        [Tooltip("Se dispara cuando se inicia la secuencia de finalización")]
        public UnityEvent onInicioFinalizacion;
        [Tooltip("Se dispara cuando el jugador pulsa 'Recibir Lente'")]
        public UnityEvent onLenteRecibido;
        [Tooltip("Se dispara al terminar toda esta primera parte de cierre")]
        public UnityEvent onFinPrimeraParte;

        [Header("--- Cinemática Final (Parte 2 - Broche de Oro) ---")]
        [Tooltip("(Opcional) Referencia a la Cinemática Final del Capítulo 2. Si no se asigna, se busca automáticamente en la escena")]
        [SerializeField] private CinematicaFinalCapitulo2 cinematicaFinal;
        [Tooltip("Si está activo, inicia automáticamente la cinemática final al concluir la despedida del guía")]
        [SerializeField] private bool encadenarCinematicaFinal = true;

        [Header("--- Debug / Pruebas ---")]
        [Tooltip("Si se activa, inicia la cinemática de finalización al dar Play para probarla de inmediato")]
        [SerializeField] private bool probarFinalizacionAlIniciar = false;

        // Variables de estado interno
        private bool finalizacionIniciada = false;
        private bool esperandoContinuar = false;
        private bool textoTerminado = false;
        private string textoCompletoActual = "";
        private float tiempoUltimoClick = 0f;
        private bool lenteRecibidoPresionado = false;
        private bool confirmacionLenteTerminada = false;
        private bool rotandoPremio = false;
        private float intensidadOriginalLuz = -1f;
        private Coroutine coroutineLatidoBoton;

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

            OcultarElementosIniciales();
        }

        private void Start()
        {
            AutoDetectarReferenciasSiFaltan();

            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveListener(OnClickContinuar);
                botonContinuar.onClick.AddListener(OnClickContinuar);
            }

            if (botonRecibirLente != null)
            {
                botonRecibirLente.onClick.RemoveListener(OnRecibirLenteClicked);
                botonRecibirLente.onClick.AddListener(OnRecibirLenteClicked);
            }

            if (botonVerdeRecibirLente != null)
            {
                Button btnV = botonVerdeRecibirLente.GetComponent<Button>();
                if (btnV != null)
                {
                    btnV.onClick.RemoveListener(OnRecibirLenteClicked);
                    btnV.onClick.AddListener(OnRecibirLenteClicked);
                }
            }

            if (probarFinalizacionAlIniciar)
            {
                IniciarFinalizacion();
            }
        }

        private void Update()
        {
            if (rotandoPremio && objeto3DPremio != null && objeto3DPremio.activeSelf)
            {
                objeto3DPremio.transform.Rotate(Vector3.up, velocidadGiroY * Time.deltaTime, Space.World);
            }
        }

        private void OcultarElementosIniciales()
        {
            if (objeto3DPremio != null) objeto3DPremio.SetActive(false);
            if (luzPremio != null)
            {
                if (intensidadOriginalLuz < 0f && luzPremio.intensity > 0.05f)
                {
                    intensidadOriginalLuz = luzPremio.intensity;
                }
                luzPremio.intensity = 0f;
                luzPremio.gameObject.SetActive(false);
            }
            if (panelLogroPremio != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelLogroPremio);
                if (cg != null) cg.alpha = 0f;
                panelLogroPremio.SetActive(false);
            }
            if (botonVerdeRecibirLente != null)
            {
                botonVerdeRecibirLente.SetActive(false);
            }
            if (panelDialogo != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelDialogo);
                if (cg != null) cg.alpha = 0f;
                panelDialogo.SetActive(false);
            }
            if (contenedorDialogosFin != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(contenedorDialogosFin);
                if (cg != null) cg.alpha = 0f;
                contenedorDialogosFin.SetActive(false);
            }
            if (textoDialogo != null)
            {
                textoDialogo.text = "";
            }
        }

        private void AutoDetectarReferenciasSiFaltan()
        {
            // 1. Detectar cámara de gameplay desde DialogosCapitulo2 o MainCamera
            if (camaraGameplay == null)
            {
                if (DialogosCapitulo2.Instance != null && DialogosCapitulo2.Instance.CamaraGameplay != null)
                {
                    camaraGameplay = DialogosCapitulo2.Instance.CamaraGameplay;
                }
                else
                {
                    Camera mainCam = Camera.main;
                    if (mainCam != null) camaraGameplay = mainCam.gameObject;
                }
            }

            // 1.5 Detectar o auto-crear Punto de Retorno de la Cámara
            if (puntoRetornoCamara == null)
            {
                GameObject[] todosGO = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject go in todosGO)
                {
                    if (go.hideFlags != HideFlags.None || !go.scene.isLoaded) continue;
                    string n = go.name.ToLower();
                    if (n == "puntoretornocamara" || n == "punto retorno camara" ||
                        n == "punto_retorno_camara" || n.Contains("retorno camara") ||
                        n.Contains("retornocamara") || n.Contains("punto camara final") ||
                        n.Contains("puntocamarafinal") || n == "punto_camara_retorno")
                    {
                        puntoRetornoCamara = go.transform;
                        break;
                    }
                }

                if (puntoRetornoCamara == null)
                {
                    GameObject nuevoPunto = new GameObject("Punto Retorno Camara");
                    if (camaraGameplay != null)
                    {
                        nuevoPunto.transform.position = camaraGameplay.transform.position;
                        nuevoPunto.transform.rotation = camaraGameplay.transform.rotation;
                    }
                    else if (puntoRetornoGuia != null)
                    {
                        nuevoPunto.transform.position = puntoRetornoGuia.position + puntoRetornoGuia.forward * 3.5f + Vector3.up * 1.5f;
                        nuevoPunto.transform.LookAt(puntoRetornoGuia.position + Vector3.up * 1.2f);
                    }
                    else
                    {
                        nuevoPunto.transform.position = new Vector3(0f, 1.6f, -3.5f);
                    }
                    puntoRetornoCamara = nuevoPunto.transform;
                }
            }

            // 2. Detectar Guía
            if (guiaAnimator == null)
            {
                DialogosCapitulo2 dComp = FindObjectOfType<DialogosCapitulo2>(true);
                if (dComp != null)
                {
                    guiaAnimator = dComp.GetComponentInChildren<Animator>(true);
                }
            }

            // 2.5 Detectar contenedor 'DIALOGOS FIN' si existe en escena o en cualquier Canvas
            if (contenedorDialogosFin == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas cv in todosCanvas)
                {
                    Transform[] transforms = cv.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in transforms)
                    {
                        string n = t.name.ToLower();
                        if (n.Contains("dialogos fin") || n.Contains("dialogo fin") || n.Contains("dialogosfin") || n == "dialogos_fin")
                        {
                            contenedorDialogosFin = t.gameObject;
                            break;
                        }
                    }
                    if (contenedorDialogosFin != null) break;
                }

                if (contenedorDialogosFin == null)
                {
                    GameObject[] todosGO = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject go in todosGO)
                    {
                        if (!go.scene.isLoaded) continue;
                        string n = go.name.ToLower();
                        if (n.Contains("dialogos fin") || n.Contains("dialogo fin") || n.Contains("dialogosfin"))
                        {
                            contenedorDialogosFin = go;
                            break;
                        }
                    }
                }
            }

            // 3. Detectar panel de diálogo
            if (panelDialogo == null)
            {
                if (contenedorDialogosFin != null)
                {
                    Transform tDia = contenedorDialogosFin.transform.Find("Panel Dialogo");
                    if (tDia == null) tDia = contenedorDialogosFin.transform.Find("PanelDialogo");
                    if (tDia == null) tDia = contenedorDialogosFin.transform.Find("Dialogo");
                    if (tDia != null) panelDialogo = tDia.gameObject;
                    else panelDialogo = contenedorDialogosFin;
                }
            }

            if (panelDialogo == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas cv in todosCanvas)
                {
                    Transform[] transforms = cv.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in transforms)
                    {
                        string n = t.name.ToLower();
                        if (n.Contains("dialogos fin") || n.Contains("dialogo fin") || n == "panel dialogo" || n == "paneldialogo" || n == "dialogo")
                        {
                            panelDialogo = t.gameObject;
                            break;
                        }
                    }
                    if (panelDialogo != null) break;
                }
            }

            // Buscar texto de diálogo y botón continuar dentro de panelDialogo o contenedorDialogosFin
            if (textoDialogo == null)
            {
                if (panelDialogo != null) textoDialogo = panelDialogo.GetComponentInChildren<TMP_Text>(true);
                if (textoDialogo == null && contenedorDialogosFin != null) textoDialogo = contenedorDialogosFin.GetComponentInChildren<TMP_Text>(true);
            }

            if (botonContinuar == null)
            {
                GameObject objBuscar = panelDialogo != null ? panelDialogo : contenedorDialogosFin;
                if (objBuscar != null)
                {
                    Button[] btns = objBuscar.GetComponentsInChildren<Button>(true);
                    foreach (Button b in btns)
                    {
                        string n = b.name.ToLower();
                        if (n.Contains("continuar") || n.Contains("siguiente") || n.Contains("next") || n.Contains("dialogo"))
                        {
                            botonContinuar = b;
                            break;
                        }
                    }
                    if (botonContinuar == null && btns.Length > 0)
                    {
                        botonContinuar = btns[0];
                    }
                }
            }

            // 4. Detectar panel de fade a negro si no está asignado
            if (panelFadeNegro == null)
            {
                MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
                if (mapa != null && mapa.PanelFadeNegro != null)
                {
                    panelFadeNegro = mapa.PanelFadeNegro;
                }
                else
                {
                    Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                    foreach (Canvas cv in todosCanvas)
                    {
                        Transform tFade = cv.transform.Find("Fade Negro");
                        if (tFade == null) tFade = cv.transform.Find("Panel Fade Negro");
                        if (tFade == null) tFade = cv.transform.Find("FadeNegro");
                        if (tFade != null)
                        {
                            panelFadeNegro = tFade.GetComponent<CanvasGroup>();
                            if (panelFadeNegro != null) break;
                        }
                    }
                }
            }

            // 5. Detectar panel de logro o botón recibir lente si faltan
            if (panelLogroPremio == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas cv in todosCanvas)
                {
                    Transform tLogro = cv.transform.Find("Panel Logro");
                    if (tLogro == null) tLogro = cv.transform.Find("Panel Premio");
                    if (tLogro == null) tLogro = cv.transform.Find("Panel Lente");
                    if (tLogro == null) tLogro = cv.transform.Find("Panel Recompensa");
                    if (tLogro != null)
                    {
                        panelLogroPremio = tLogro.gameObject;
                        break;
                    }
                }
            }

            if (botonRecibirLente == null && panelLogroPremio != null)
            {
                Button[] btns = panelLogroPremio.GetComponentsInChildren<Button>(true);
                foreach (Button b in btns)
                {
                    string n = b.name.ToLower();
                    if (n.Contains("recibir") || n.Contains("lente") || n.Contains("aceptar") || n.Contains("premio") || n.Contains("claim"))
                    {
                        botonRecibirLente = b;
                        break;
                    }
                }
                if (botonRecibirLente == null && btns.Length > 0)
                {
                    botonRecibirLente = btns[0];
                }
            }

            if (botonVerdeRecibirLente == null && panelLogroPremio != null)
            {
                Transform[] transforms = panelLogroPremio.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in transforms)
                {
                    string n = t.name.ToLower();
                    if (n.Contains("verde") || n.Contains("check") || n.Contains("confirmado") || n.Contains("pop"))
                    {
                        if (botonRecibirLente == null || t.gameObject != botonRecibirLente.gameObject)
                        {
                            botonVerdeRecibirLente = t.gameObject;
                            break;
                        }
                    }
                }
            }

            if (objeto3DPremio == null)
            {
                GameObject[] todosLosObjetos = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject go in todosLosObjetos)
                {
                    if (go.hideFlags != HideFlags.None || !go.scene.isLoaded) continue;
                    string n = go.name.ToLower();
                    if (n.Contains("lente") || n.Contains("premio") || n.Contains("recompensa") || n.Contains("trofeo"))
                    {
                        if (go.GetComponent<RectTransform>() == null)
                        {
                            objeto3DPremio = go;
                            break;
                        }
                    }
                }
            }

            if (luzPremio == null)
            {
                if (objeto3DPremio != null)
                {
                    luzPremio = objeto3DPremio.GetComponentInChildren<Light>(true);
                }
                if (luzPremio == null)
                {
                    Light[] luces = Resources.FindObjectsOfTypeAll<Light>();
                    foreach (Light l in luces)
                    {
                        if (l.hideFlags != HideFlags.None || !l.gameObject.scene.isLoaded) continue;
                        string n = l.name.ToLower();
                        if (n.Contains("premio") || n.Contains("lente") || n.Contains("recompensa") || n.Contains("foco"))
                        {
                            luzPremio = l;
                            break;
                        }
                    }
                }
            }

            if (luzPremio != null && intensidadOriginalLuz < 0f && luzPremio.intensity > 0.05f)
            {
                intensidadOriginalLuz = luzPremio.intensity;
            }

            if (cinematicaFinal == null)
            {
                cinematicaFinal = FindObjectOfType<CinematicaFinalCapitulo2>(true);
            }
        }

        /// <summary>
        /// Comprueba si las 4 estaciones están completadas en PlayerPrefs.
        /// </summary>
        public static bool EstanTodasLasEstacionesCompletadas()
        {
            for (int i = 1; i <= 4; i++)
            {
                bool completada = PlayerPrefs.GetInt("Cap2_Estacion" + i + "_Completada", 0) == 1
                               || PlayerPrefs.GetInt("Estacion" + i + "_Completada", 0) == 1;
                if (!completada) return false;
            }
            return true;
        }

        /// <summary>
        /// Inicia la secuencia de cierre y felicitación del Capítulo 2.
        /// </summary>
        public void IniciarFinalizacion()
        {
            if (finalizacionIniciada) return;
            finalizacionIniciada = true;

            gameObject.SetActive(true);
            ActivarPadresHastaCanvas(gameObject);
            AutoDetectarReferenciasSiFaltan();

            StopAllCoroutines();
            StartCoroutine(SecuenciaFinalizacionCompleta());
        }

        private IEnumerator SecuenciaFinalizacionCompleta()
        {
            onInicioFinalizacion?.Invoke();

            // PASO 1: Fade a negro asegurado
            if (panelFadeNegro == null)
            {
                AutoDetectarReferenciasSiFaltan();
            }

            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                panelFadeNegro.blocksRaycasts = true;

                if (panelFadeNegro.alpha < 0.95f)
                {
                    float t = 0f;
                    float alphaInicial = panelFadeNegro.alpha;
                    while (t < duracionFadeNegro)
                    {
                        t += Time.deltaTime;
                        panelFadeNegro.alpha = Mathf.Lerp(alphaInicial, 1f, t / duracionFadeNegro);
                        yield return null;
                    }
                }
                panelFadeNegro.alpha = 1f;
            }

            // El fade final ya está opaco: retirar los de las estaciones y del mapa para no dejar la vista tapada.
            OcultarFundidosAnteriores();

            // Ocultar mapa si está activo
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapa != null) mapa.OcultarMapa();

            Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
            foreach (Canvas c in todosCanvas)
            {
                Transform tMapa1 = c.transform.Find("Panel Mapa");
                if (tMapa1 != null) tMapa1.gameObject.SetActive(false);
                Transform tMapa2 = c.transform.Find("Mapa");
                if (tMapa2 != null) tMapa2.gameObject.SetActive(false);
            }

            // Configurar entorno inicial (retorno al punto de bienvenida)
            ConfigurarRetornoInicial();

            yield return new WaitForSeconds(0.35f);

            // Fade desde negro
            if (panelFadeNegro != null)
            {
                float t = 0f;
                while (t < duracionFadeNegro)
                {
                    t += Time.deltaTime;
                    panelFadeNegro.alpha = Mathf.Clamp01(1f - (t / duracionFadeNegro));
                    yield return null;
                }
                panelFadeNegro.alpha = 0f;
                panelFadeNegro.blocksRaycasts = false;
                panelFadeNegro.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.25f);

            // PASO 2: El Guía se queda aplaudiendo y felicita al explorador
            IniciarAplauso();

            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");

            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(MostrarDialogo("¡Increíble, " + nombreJugador + "! ¡Completaste con éxito las 4 estaciones del Capítulo!"));
            yield return StartCoroutine(MostrarDialogo("Recorrimos juntos el tablero de Adelson, la ilusión de Ferrari, la junta directiva y el puente de cristal."));
            yield return StartCoroutine(MostrarDialogo("Has demostrado una agudeza extraordinaria para distinguir la realidad de lo que tu mente interpreta."));

            DetenerAplauso();

            // Ocultar diálogo con fade out suave antes de exhibir el objeto 3D
            yield return StartCoroutine(OcultarPanelDialogoSuave(0.3f));

            yield return new WaitForSeconds(0.2f);

            // PASO 3: POP del Objeto 3D en la escena + Luz + Guía aplaude de nuevo
            IniciarAplauso();

            yield return StartCoroutine(AnimarPopObjeto3DPremio());

            // Giro ligeramente suave para que el explorador lo admire antes de pasar al panel
            yield return new WaitForSeconds(1.8f);

            yield return StartCoroutine(MostrarDialogo("Por haber completado este gran entrenamiento, has desbloqueado un artefacto legendario."));

            DetenerAplauso();

            // Ocultar diálogo con fade out suave antes de mostrar el panel de logro
            yield return StartCoroutine(OcultarPanelDialogoSuave(0.3f));

            // PASO 4: Aparece el Panel de Logro ("LENTE DE LA INTERPRETACIÓN") con Fade In
            if (botonVerdeRecibirLente != null) botonVerdeRecibirLente.SetActive(false);

            if (panelLogroPremio != null)
            {
                ActivarPadresHastaCanvas(panelLogroPremio);
                yield return StartCoroutine(FadeCanvasGroup(ObtenerOAgregarCanvasGroup(panelLogroPremio), 0f, 1f, duracionFadePanelLogro));
            }

            // Iniciar efecto de latido muy suave en el botón Recibir Lente
            if (botonRecibirLente != null)
            {
                if (coroutineLatidoBoton != null) StopCoroutine(coroutineLatidoBoton);
                coroutineLatidoBoton = StartCoroutine(AnimarLatidoBoton(botonRecibirLente.gameObject));
            }

            // Esperar a que el jugador pulse "Recibir Lente" y se complete la animación del botón verde
            lenteRecibidoPresionado = false;
            confirmacionLenteTerminada = false;
            while (!confirmacionLenteTerminada)
            {
                yield return null;
            }

            if (coroutineLatidoBoton != null)
            {
                StopCoroutine(coroutineLatidoBoton);
                coroutineLatidoBoton = null;
            }

            onLenteRecibido?.Invoke();

            // Guardar que el jugador ya tiene el lente en PlayerPrefs
            PlayerPrefs.SetInt("Cap2_LenteObtenido", 1);
            PlayerPrefs.SetInt("Cap2_CompletadoTotal", 1);
            PlayerPrefs.Save();

            // Fade Out del Panel de Logro
            if (panelLogroPremio != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(ObtenerOAgregarCanvasGroup(panelLogroPremio), 1f, 0f, duracionFadePanelLogro));
                panelLogroPremio.SetActive(false);
            }

            // El objeto 3D hace pop out o se guarda suavemente
            yield return StartCoroutine(AnimarDespedidaObjeto3D());

            yield return new WaitForSeconds(0.25f);

            // PASO 5: El Guía felicita y da el mensaje de despedida e inspiración (sin dos puntos)
            IniciarAplauso();

            yield return StartCoroutine(MostrarDialogo("¡El Lente de la Interpretación ahora es tuyo!"));

            DetenerAplauso();

            yield return StartCoroutine(MostrarDialogo("Recuerda que tu cerebro no es una cámara, sino un narrador que interpreta lo que sientes."));
            yield return StartCoroutine(MostrarDialogo("Muchas veces el dolor busca protegerte, incluso sin un daño físico real."));
            yield return StartCoroutine(MostrarDialogo("¡Diste un paso gigante hoy, " + nombreJugador + "! ¡Nos vemos pronto!"));

            yield return StartCoroutine(OcultarPanelDialogoSuave(0.35f));

            // Fin de la primera parte
            onFinPrimeraParte?.Invoke();
            Debug.Log("FinalizacionCapitulo2: Primera parte completada exitosamente.");

            // Encadenar automáticamente la Cinemática Final (Parte 2 - Broche de Oro)
            if (encadenarCinematicaFinal)
            {
                if (cinematicaFinal == null)
                {
                    cinematicaFinal = FindObjectOfType<CinematicaFinalCapitulo2>(true);
                }

                if (cinematicaFinal != null)
                {
                    cinematicaFinal.Iniciar();
                }
            }
        }

        private void OcultarFundidosAnteriores()
        {
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            EstacionAdelsonCapitulo2 adelson = FindObjectOfType<EstacionAdelsonCapitulo2>(true);
            EstacionFerrariCapitulo2 ferrari = FindObjectOfType<EstacionFerrariCapitulo2>(true);
            MinijuegoFerrariCapitulo2 minijuego = FindObjectOfType<MinijuegoFerrariCapitulo2>(true);
            EstacionJuntaCapitulo2 junta = FindObjectOfType<EstacionJuntaCapitulo2>(true);
            EstacionPuenteCapitulo2 puente = FindObjectOfType<EstacionPuenteCapitulo2>(true);
            OcultarFundidoAnterior(mapa != null ? mapa.PanelFadeNegro : null);
            OcultarFundidoAnterior(adelson != null ? adelson.PanelFadeNegro : null);
            OcultarFundidoAnterior(ferrari != null ? ferrari.PanelFadeNegro : null);
            OcultarFundidoAnterior(minijuego != null ? minijuego.PanelFadeNegro : null);
            OcultarFundidoAnterior(junta != null ? junta.PanelFadeNegro : null);
            OcultarFundidoAnterior(puente != null ? puente.PanelFadeNegro : null);
        }

        private void OcultarFundidoAnterior(CanvasGroup fundido)
        {
            if (fundido == null || fundido == panelFadeNegro) return;
            fundido.alpha = 0f;
            fundido.blocksRaycasts = false;
            fundido.gameObject.SetActive(false);
        }

        private void ConfigurarRetornoInicial()
        {
            // 1. Ubicar la cámara de bienvenida / gameplay
            Transform camTransform = null;
            if (camaraGameplay != null)
            {
                Camera camComp = CamarasCapitulo2.Activar(camaraGameplay);
                if (camComp != null) camComp.depth = 35;
                camTransform = camaraGameplay.transform;
            }
            else if (Camera.main != null)
            {
                camTransform = Camera.main.transform;
                CamarasCapitulo2.Activar(camTransform.gameObject);
            }

            // Aplicar de forma estricta la posición y rotación del punto de retorno de la cámara
            if (camTransform != null && puntoRetornoCamara != null)
            {
                camTransform.position = puntoRetornoCamara.position;
                camTransform.rotation = puntoRetornoCamara.rotation;
            }

            // 2. Ubicar Guía en su punto de inicio
            if (guiaAnimator != null && puntoRetornoGuia != null)
            {
                guiaAnimator.applyRootMotion = false;
                guiaAnimator.transform.position = puntoRetornoGuia.position;
                guiaAnimator.transform.rotation = puntoRetornoGuia.rotation;
            }

            // 3. Activar avatar del jugador (Hombre o Mujer según elección)
            int avatarElegido = PlayerPrefs.GetInt("Avatar", 1);
            if (jugadorHombre != null) jugadorHombre.SetActive(avatarElegido == 1);
            if (jugadorMujer != null) jugadorMujer.SetActive(avatarElegido == 2);

            GameObject jugadorActivo = (avatarElegido == 1) ? jugadorHombre : jugadorMujer;
            if (jugadorActivo != null && puntoRetornoJugador != null)
            {
                Animator animJugador = jugadorActivo.GetComponent<Animator>();
                if (animJugador != null) animJugador.applyRootMotion = false;

                jugadorActivo.transform.position = puntoRetornoJugador.position;
                jugadorActivo.transform.rotation = puntoRetornoJugador.rotation;
            }

            Physics.SyncTransforms();
        }

        private IEnumerator AnimarPopObjeto3DPremio()
        {
            float targetIntensidad = (intensidadOriginalLuz > 0f) ? intensidadOriginalLuz : Mathf.Max(intensidadLuzPremio, 5f);

            if (luzPremio != null)
            {
                ActivarPadresHastaCanvas(luzPremio.gameObject);
                luzPremio.gameObject.SetActive(true);
                luzPremio.enabled = true;
                luzPremio.intensity = 0f;
                if (luzPremio.range <= 1f) luzPremio.range = 8f;
            }

            Vector3 escalaDestino = (escalaPremio3D != Vector3.zero) ? escalaPremio3D : Vector3.one;

            if (objeto3DPremio != null)
            {
                ActivarPadresHastaCanvas(objeto3DPremio);
                objeto3DPremio.SetActive(true);
                objeto3DPremio.transform.localScale = Vector3.zero;
            }

            rotandoPremio = girarObjetoPremio;

            // Animación POP elástica suave (0 -> 1.2f -> 1.0f)
            float duracion = 0.6f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float progreso = Mathf.Clamp01(tiempo / duracion);

                float factorEscala;
                if (progreso < 0.65f)
                {
                    factorEscala = Mathf.Lerp(0f, 1.2f, progreso / 0.65f);
                }
                else
                {
                    factorEscala = Mathf.Lerp(1.2f, 1.0f, (progreso - 0.65f) / 0.35f);
                }

                if (objeto3DPremio != null)
                {
                    objeto3DPremio.transform.localScale = escalaDestino * factorEscala;
                }

                if (luzPremio != null)
                {
                    luzPremio.intensity = Mathf.Lerp(0f, targetIntensidad, progreso);
                }

                yield return null;
            }

            if (objeto3DPremio != null)
            {
                objeto3DPremio.transform.localScale = escalaDestino;
            }
            if (luzPremio != null)
            {
                luzPremio.intensity = targetIntensidad;
            }
        }

        private IEnumerator AnimarDespedidaObjeto3D()
        {
            Vector3 escalaInicial = (objeto3DPremio != null) ? objeto3DPremio.transform.localScale : Vector3.one;
            float targetIntensidad = (luzPremio != null) ? luzPremio.intensity : 0f;
            float duracion = 0.4f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float p = Mathf.Clamp01(tiempo / duracion);

                if (objeto3DPremio != null)
                {
                    objeto3DPremio.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, p);
                }

                if (luzPremio != null)
                {
                    luzPremio.intensity = Mathf.Lerp(targetIntensidad, 0f, p);
                }

                yield return null;
            }

            rotandoPremio = false;
            if (objeto3DPremio != null) objeto3DPremio.SetActive(false);
            if (luzPremio != null)
            {
                luzPremio.intensity = 0f;
                luzPremio.gameObject.SetActive(false);
            }
        }

        public void OnRecibirLenteClicked()
        {
            if (lenteRecibidoPresionado) return;
            lenteRecibidoPresionado = true;

            if (coroutineLatidoBoton != null)
            {
                StopCoroutine(coroutineLatidoBoton);
                coroutineLatidoBoton = null;
            }

            StartCoroutine(SecuenciaConfirmarLente());
        }

        private IEnumerator SecuenciaConfirmarLente()
        {
            if (botonVerdeRecibirLente != null)
            {
                ActivarPadresHastaCanvas(botonVerdeRecibirLente);
                botonVerdeRecibirLente.SetActive(true);
                yield return StartCoroutine(AnimacionPopSuave(botonVerdeRecibirLente, 1.10f, 0.12f));
            }
            else if (botonRecibirLente != null)
            {
                yield return StartCoroutine(AnimacionPopBoton(botonRecibirLente.gameObject));
            }

            yield return new WaitForSeconds(0.08f);
            confirmacionLenteTerminada = true;
        }

        private IEnumerator AnimarLatidoBoton(GameObject btnObj)
        {
            if (btnObj == null) yield break;
            Vector3 escalaOriginal = btnObj.transform.localScale;
            if (escalaOriginal == Vector3.zero || escalaOriginal.x <= 0.05f) escalaOriginal = Vector3.one;

            float tiempo = 0f;
            while (true)
            {
                tiempo += Time.deltaTime * 4.5f;
                float pulso = 1f + Mathf.Sin(tiempo) * 0.025f;
                btnObj.transform.localScale = escalaOriginal * pulso;
                yield return null;
            }
        }

        private IEnumerator AnimacionPopSuave(GameObject targetObj, float escalaMax = 1.15f, float duracion = 0.22f)
        {
            if (targetObj == null) yield break;

            Vector3 escalaDestino = targetObj.transform.localScale;
            if (escalaDestino == Vector3.zero || escalaDestino.x <= 0.05f)
            {
                escalaDestino = Vector3.one;
            }

            targetObj.SetActive(true);
            Transform tr = targetObj.transform;
            tr.localScale = Vector3.zero;

            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float progreso = Mathf.Clamp01(tiempo / duracion);

                float escalaMultiplicador;
                if (progreso < 0.65f)
                {
                    escalaMultiplicador = Mathf.Lerp(0f, escalaMax, progreso / 0.65f);
                }
                else
                {
                    escalaMultiplicador = Mathf.Lerp(escalaMax, 1.0f, (progreso - 0.65f) / 0.35f);
                }

                tr.localScale = escalaDestino * escalaMultiplicador;
                yield return null;
            }

            tr.localScale = escalaDestino;
        }

        private void IniciarAplauso(float velocidad = -1f)
        {
            if (guiaAnimator != null)
            {
                guiaAnimator.speed = (velocidad > 0f) ? velocidad : velocidadAnimAplauso;
                guiaAnimator.CrossFade(estadoAplaudir, 0.15f);
            }
        }

        private void IniciarAplausoRapido()
        {
            IniciarAplauso();
        }

        private void DetenerAplauso()
        {
            if (guiaAnimator != null)
            {
                guiaAnimator.speed = 1f;
                guiaAnimator.CrossFade(estadoIdle, 0.2f);
            }
        }

        private IEnumerator AnimacionPopBoton(GameObject btn)
        {
            if (btn == null) yield break;
            Vector3 original = btn.transform.localScale;
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                btn.transform.localScale = original * Mathf.Lerp(1f, 1.15f, t / 0.15f);
                yield return null;
            }
            t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                btn.transform.localScale = original * Mathf.Lerp(1.15f, 1f, t / 0.15f);
                yield return null;
            }
            btn.transform.localScale = original;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
        {
            if (cg == null) yield break;

            cg.gameObject.SetActive(true);
            cg.blocksRaycasts = (end > 0.5f);
            cg.interactable = (end > 0.5f);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, end, t / duration);
                yield return null;
            }
            cg.alpha = end;
        }

        private IEnumerator MostrarDialogo(string texto)
        {
            // Limpiar texto INMEDIATAMENTE para que no se vea el diálogo anterior durante el fade in
            if (textoDialogo != null) textoDialogo.text = "";
            textoCompletoActual = texto;
            textoTerminado = false;
            esperandoContinuar = true;

            if (contenedorDialogosFin != null)
            {
                ActivarPadresHastaCanvas(contenedorDialogosFin);
                contenedorDialogosFin.SetActive(true);
            }

            if (panelDialogo != null)
            {
                ActivarPadresHastaCanvas(panelDialogo);
                panelDialogo.SetActive(true);
                panelDialogo.transform.SetAsLastSibling();

                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelDialogo);
                if (cg != null && cg.alpha < 0.95f)
                {
                    yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 1f, 0.25f));
                }
                else if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                }
            }
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);

            if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, true);

            int i = 0;
            while (i < texto.Length)
            {
                if (textoTerminado) break;

                if (texto[i] == '<')
                {
                    int finTag = texto.IndexOf('>', i);
                    if (finTag != -1)
                    {
                        string tag = texto.Substring(i, finTag - i + 1);
                        if (textoDialogo != null) textoDialogo.text += tag;
                        i = finTag + 1;
                        continue;
                    }
                }

                if (textoDialogo != null) textoDialogo.text += texto[i];
                i++;
                yield return new WaitForSeconds(Mathf.Max(0.01f, velocidadEscritura * 0.8f));
            }

            if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
            textoTerminado = true;
            if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, false);

            while (esperandoContinuar)
            {
                yield return null;
            }
        }

        public void OnClickContinuar()
        {
            if (Time.time - tiempoUltimoClick < 0.12f) return;
            tiempoUltimoClick = Time.time;

            if (!textoTerminado)
            {
                textoTerminado = true;
                if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
                if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, false);
                return;
            }

            if (esperandoContinuar)
            {
                esperandoContinuar = false;
            }
        }

        private IEnumerator OcultarPanelDialogoSuave(float duracion = 0.25f)
        {
            if (panelDialogo != null && panelDialogo.activeSelf)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelDialogo);
                if (cg != null)
                {
                    cg.blocksRaycasts = false;
                    cg.interactable = false;
                    float t = 0f;
                    float alphaInicial = cg.alpha;
                    while (t < duracion)
                    {
                        t += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(alphaInicial, 0f, t / duracion);
                        yield return null;
                    }
                    cg.alpha = 0f;
                }
                panelDialogo.SetActive(false);
            }

            if (contenedorDialogosFin != null && contenedorDialogosFin.activeSelf && contenedorDialogosFin != panelDialogo)
            {
                CanvasGroup cgCont = contenedorDialogosFin.GetComponent<CanvasGroup>();
                if (cgCont != null)
                {
                    float t = 0f;
                    float alphaInicial = cgCont.alpha;
                    while (t < duracion)
                    {
                        t += Time.deltaTime;
                        cgCont.alpha = Mathf.Lerp(alphaInicial, 0f, t / duracion);
                        yield return null;
                    }
                    cgCont.alpha = 0f;
                }
                contenedorDialogosFin.SetActive(false);
            }

            if (textoDialogo != null)
            {
                textoDialogo.text = "";
            }
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

        [ContextMenu("Crear o Alinear Punto de Retorno de Camara")]
        public void CrearOAlinearPuntoRetornoCamara()
        {
            if (puntoRetornoCamara == null)
            {
                GameObject nuevoPunto = new GameObject("Punto Retorno Camara");
                puntoRetornoCamara = nuevoPunto.transform;
            }

            if (camaraGameplay != null)
            {
                puntoRetornoCamara.position = camaraGameplay.transform.position;
                puntoRetornoCamara.rotation = camaraGameplay.transform.rotation;
                Debug.Log("[FinalizacionCapitulo2] Punto de retorno de cámara alineado con la cámara de gameplay.");
            }
            else if (puntoRetornoGuia != null)
            {
                puntoRetornoCamara.position = puntoRetornoGuia.position + puntoRetornoGuia.forward * 3.5f + Vector3.up * 1.5f;
                puntoRetornoCamara.LookAt(puntoRetornoGuia.position + Vector3.up * 1.2f);
                Debug.Log("[FinalizacionCapitulo2] Punto de retorno de cámara posicionado frente al Guía.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (puntoRetornoCamara != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(puntoRetornoCamara.position, new Vector3(0.5f, 0.4f, 0.6f));
                Gizmos.DrawRay(puntoRetornoCamara.position, puntoRetornoCamara.forward * 2f);
            }
            if (puntoRetornoGuia != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(puntoRetornoGuia.position + Vector3.up * 0.9f, 0.35f);
                Gizmos.DrawRay(puntoRetornoGuia.position + Vector3.up * 0.9f, puntoRetornoGuia.forward * 1.5f);
            }
            if (puntoRetornoJugador != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(puntoRetornoJugador.position + Vector3.up * 0.9f, 0.35f);
                Gizmos.DrawRay(puntoRetornoJugador.position + Vector3.up * 0.9f, puntoRetornoJugador.forward * 1.5f);
            }
        }
    }
}
