using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

namespace Capitulo2
{
    [AddComponentMenu("Capitulo 2/Estacion Adelson")]
    public class EstacionAdelsonCapitulo2 : MonoBehaviour
    {
        [Header("--- Teletransporte y Entorno ---")]
        [Tooltip("Transform del Guía en la escena")]
        [SerializeField] private Transform transformGuia;
        [Tooltip("Punto de destino del Guía en la plataforma de Adelson")]
        [SerializeField] private Transform puntoDestinoGuia;

        [Space(6)]
        [Tooltip("Objeto o Transform del jugador")]
        [SerializeField] private Transform transformJugador;
        [Tooltip("Avatar Hombre del jugador (si existen separados)")]
        [SerializeField] private GameObject jugadorHombre;
        [Tooltip("Avatar Mujer del jugador (si existen separados)")]
        [SerializeField] private GameObject jugadorMujer;
        [Tooltip("Punto de destino del jugador en la plataforma")]
        [SerializeField] private Transform puntoDestinoJugador;

        [Space(6)]
        [Tooltip("(Opcional) Cámara dedicada para esta estación. Si se asigna, se enciende al llegar")]
        [SerializeField] private GameObject camaraEstacion;
        [Tooltip("(Opcional) Transform de la cámara principal si se prefiere mover en lugar de cambiar")]
        [SerializeField] private Transform transformCamara;
        [Tooltip("(Opcional) Destino para mover la cámara principal")]
        [SerializeField] private Transform puntoDestinoCamara;

        [Header("--- UI de Diálogo ---")]
        [SerializeField] private GameObject panelDialogo;
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuar;
        [SerializeField] private float velocidadTypewriter = 0.055f;

        [Header("--- UI de Preguntas y Respuestas ---")]
        [SerializeField] private GameObject panelRespuestas;
        [Tooltip("(Opcional) Texto dentro del panel de respuestas para recordar la pregunta")]
        [SerializeField] private TMP_Text textoPreguntaEnRespuestas;
        [SerializeField] private Button botonOpcionA;
        [SerializeField] private TMP_Text textoOpcionA;
        [SerializeField] private GameObject checkOpcionA;
        [Space(4)]
        [SerializeField] private Button botonOpcionB;
        [SerializeField] private TMP_Text textoOpcionB;
        [SerializeField] private GameObject checkOpcionB;
        [Space(4)]
        [SerializeField] private Button botonResponder;

        [Header("--- UI Nivel de Confianza (1 al 5) ---")]
        [Tooltip("Panel donde el jugador elige qué tan seguro está (1 a 5) antes de revelar la ilusión")]
        [SerializeField] private GameObject panelConfianza;
        [Tooltip("Los 5 botones de confianza (1 = Poca confianza, 5 = Total seguridad)")]
        [SerializeField] private Button[] botonesConfianza = new Button[5];
        [Tooltip("Check o efecto verde para cada uno de los 5 botones (opcional)")]
        [SerializeField] private GameObject[] checksConfianza = new GameObject[5];
        [Tooltip("Botón para confirmar el nivel de confianza y aplicar el filtro")]
        [SerializeField] private Button botonConfirmarConfianza;

        [Header("--- Ilusión 1: Tablero de Adelson ---")]
        [Tooltip("Contenedor o imagen del Tablero de Adelson en Canvas")]
        [SerializeField] private GameObject panelIlusion1;
        [Tooltip("Franja o línea con CanvasGroup que conecta las casillas revelando el mismo color")]
        [SerializeField] private CanvasGroup franjaRevelacion1;

        [Header("--- Ilusión 2: El Vestido ---")]
        [Tooltip("Contenedor o imagen de los vestidos en Canvas")]
        [SerializeField] private GameObject panelIlusion2;
        [Tooltip("Franja o línea con CanvasGroup que revela el color base idéntico")]
        [SerializeField] private CanvasGroup franjaRevelacion2;

        [Header("--- Ilusión 3: Müller-Lyer ---")]
        [Tooltip("Contenedor o imagen de las líneas Müller-Lyer en Canvas")]
        [SerializeField] private GameObject panelIlusion3;
        [Tooltip("Líneas con CanvasGroup que demuestran que miden exactamente lo mismo")]
        [SerializeField] private CanvasGroup franjaRevelacion3;

        [Header("--- Duración de Efectos y Animaciones ---")]
        [SerializeField] private float duracionFadeRevelacion = 0.8f;
        [SerializeField] private float duracionFadeNegro = 0.7f;

        [Header("--- Animaciones de Personajes ---")]
        [SerializeField] private Animator guiaAnimator;
        [SerializeField] private string parametroHablando = "Hablando";
        [SerializeField] private string estadoAplaudir = "Clapping";
        [SerializeField] private string estadoIdle = "Idle";
        [SerializeField] private float duracionAplauso = 2.2f;

        [Header("--- Fade a Negro de Pantalla ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        public CanvasGroup PanelFadeNegro => panelFadeNegro;

        [Header("--- Guardado de Progreso y Eventos ---")]
        [Tooltip("Clave de PlayerPrefs donde se guarda el avance de esta estación")]
        [SerializeField] private string claveGuardadoEstacion = "Cap2_Estacion1_Completada";
        [Tooltip("Se dispara cuando se termina toda la actividad y el diálogo de cierre")]
        public UnityEvent onEstacionCompletada;

        [Header("--- Debug / Pruebas ---")]
        [Tooltip("Si se activa, la estación inicia sola al dar Play (ideal para probar sin pasar por el mapa)")]
        [SerializeField] private bool iniciarAlEmpezar = false;

        // Variables de estado interno
        private bool estacionIniciada = false;
        private bool esperandoContinuar = false;
        private bool textoTerminado = false;
        private string textoCompletoActual = "";
        private float tiempoUltimoClick = 0f;

        private Vector3 escalaBaseCheckA = Vector3.one;
        private Vector3 escalaBaseCheckB = Vector3.one;
        private Vector3 escalaBaseBotonResponder = Vector3.one;

        private bool respuestaConfirmada = false;
        private int opcionSeleccionada = 0; // 1 = A, 2 = B
        private int nivelConfianzaSeleccionado = 0; // 1 a 5
        private bool confianzaConfirmada = false;
        private Coroutine coroutinePopBoton;
        private Coroutine coroutinePopCheck;
        private Coroutine coroutinePopBotonConfianza;

        private void Awake()
        {
            ConectarConMapa();

            if (checkOpcionA != null && checkOpcionA.transform.localScale.x > 0.1f)
                escalaBaseCheckA = checkOpcionA.transform.localScale;
            else
                escalaBaseCheckA = Vector3.one;

            if (checkOpcionB != null && checkOpcionB.transform.localScale.x > 0.1f)
                escalaBaseCheckB = checkOpcionB.transform.localScale;
            else
                escalaBaseCheckB = Vector3.one;

            if (botonResponder != null && botonResponder.transform.localScale.x > 0.1f)
                escalaBaseBotonResponder = botonResponder.transform.localScale;
            else
                escalaBaseBotonResponder = Vector3.one;

            if (checkOpcionA != null) checkOpcionA.SetActive(false);
            if (checkOpcionB != null) checkOpcionB.SetActive(false);
            if (panelConfianza != null) panelConfianza.SetActive(false);

            if (checksConfianza != null)
            {
                for (int i = 0; i < checksConfianza.Length; i++)
                {
                    if (checksConfianza[i] != null) checksConfianza[i].SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            ConectarConMapa();
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

        private void ConectarConMapa()
        {
#if UNITY_2023_1_OR_NEWER
            MapaSeleccionCapitulo2 mapa = Object.FindFirstObjectByType<MapaSeleccionCapitulo2>(FindObjectsInactive.Include);
#else
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
#endif
            if (mapa != null)
            {
                mapa.onZonaConfirmada.RemoveListener(OnZonaConfirmadaDesdeMapa);
                mapa.onZonaConfirmada.AddListener(OnZonaConfirmadaDesdeMapa);
            }
        }

        private void Start()
        {
            ConectarConMapa();
            AutoDetectarReferenciasSiFaltan();

            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveListener(OnClickContinuar);
                botonContinuar.onClick.AddListener(OnClickContinuar);
            }

            if (botonOpcionA != null)
            {
                botonOpcionA.onClick.RemoveListener(SeleccionarOpcionA);
                botonOpcionA.onClick.AddListener(SeleccionarOpcionA);
            }

            if (botonOpcionB != null)
            {
                botonOpcionB.onClick.RemoveListener(SeleccionarOpcionB);
                botonOpcionB.onClick.AddListener(SeleccionarOpcionB);
            }

            if (botonResponder != null)
            {
                botonResponder.onClick.RemoveListener(ConfirmarRespuesta);
                botonResponder.onClick.AddListener(ConfirmarRespuesta);
            }

            // Conectar botones de nivel de confianza (1 al 5)
            if (botonesConfianza != null)
            {
                for (int i = 0; i < botonesConfianza.Length; i++)
                {
                    int nivel = i + 1;
                    if (botonesConfianza[i] != null)
                    {
                        botonesConfianza[i].onClick.RemoveAllListeners();
                        botonesConfianza[i].onClick.AddListener(() => SeleccionarNivelConfianza(nivel));
                    }
                }
            }

            if (botonConfirmarConfianza != null)
            {
                botonConfirmarConfianza.onClick.RemoveListener(ConfirmarNivelConfianza);
                botonConfirmarConfianza.onClick.AddListener(ConfirmarNivelConfianza);
            }

            if (iniciarAlEmpezar)
            {
                IniciarEstacion();
            }
        }

        private void AutoDetectarReferenciasSiFaltan()
        {
            // Auto-detectar paneles de diálogo y respuestas si faltan en Inspector
            Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
            if (panelDialogo == null || panelRespuestas == null || panelConfianza == null)
            {
                foreach (Canvas cv in todosCanvas)
                {
                    if (panelDialogo == null)
                    {
                        Transform t = cv.transform.Find("Panel Dialogo");
                        if (t == null) t = cv.transform.Find("Dialogo");
                        if (t != null) panelDialogo = t.gameObject;
                    }
                    if (panelRespuestas == null)
                    {
                        Transform t = cv.transform.Find("Panel Respuestas");
                        if (t == null) t = cv.transform.Find("Respuestas");
                        if (t != null) panelRespuestas = t.gameObject;
                    }
                    if (panelConfianza == null)
                    {
                        Transform t = cv.transform.Find("Panel Confianza");
                        if (t == null) t = cv.transform.Find("Confianza");
                        if (t != null) panelConfianza = t.gameObject;
                    }
                }
            }

            // Auto-detectar paneles de ilusión si faltan en Inspector
            if (panelIlusion1 == null || panelIlusion2 == null || panelIlusion3 == null)
            {
                foreach (Canvas cv in todosCanvas)
                {
                    Transform[] transforms = cv.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in transforms)
                    {
                        string n = t.name.ToLower();

                        // Ilusión 1: Adelson
                        if (panelIlusion1 == null)
                        {
                            if (n == "panel ilusion 1" || n == "ilusion 1" || n == "panelilusion1" || n == "ilusion1" ||
                                n == "tablero adelson" || n == "adelson" || n == "panel adelson" || n == "tablero")
                            {
                                panelIlusion1 = t.gameObject;
                            }
                        }

                        // Ilusión 2: El Vestido
                        if (panelIlusion2 == null)
                        {
                            if (n == "panel ilusion 2" || n == "ilusion 2" || n == "panelilusion2" || n == "ilusion2" ||
                                n == "vestido" || n == "panel vestido" || n == "vestidos" || n == "panel vestidos")
                            {
                                panelIlusion2 = t.gameObject;
                            }
                        }

                        // Ilusión 3: Müller-Lyer
                        if (panelIlusion3 == null)
                        {
                            if (n == "panel ilusion 3" || n == "ilusion 3" || n == "panelilusion3" || n == "ilusion3" ||
                                n.Contains("muller") || n == "lineas" || n == "panel lineas")
                            {
                                panelIlusion3 = t.gameObject;
                            }
                        }
                    }
                }
            }

            // Auto-detectar franjas de revelación si faltan, revisando dentro de cada ilusión o canvas
            if (franjaRevelacion1 == null && panelIlusion1 != null)
            {
                franjaRevelacion1 = BuscarFranjaEnPanel(panelIlusion1, "1", "adelson");
            }
            if (franjaRevelacion2 == null && panelIlusion2 != null)
            {
                franjaRevelacion2 = BuscarFranjaEnPanel(panelIlusion2, "2", "vestido");
            }
            if (franjaRevelacion3 == null && panelIlusion3 != null)
            {
                franjaRevelacion3 = BuscarFranjaEnPanel(panelIlusion3, "3", "muller");
            }
        }

        private CanvasGroup BuscarFranjaEnPanel(GameObject panel, string num, string clave)
        {
            if (panel == null) return null;
            Transform[] transforms = panel.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t.gameObject == panel) continue;
                string n = t.name.ToLower();
                if (n.Contains("franja") || n.Contains("filtro") || n.Contains("revel") || n.Contains(num) || n.Contains(clave))
                {
                    CanvasGroup cg = t.GetComponent<CanvasGroup>();
                    if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
                    return cg;
                }
            }
            return null;
        }

        private IEnumerator MostrarIlusionConFade(GameObject panelIlusion, CanvasGroup franjaAsociada, float duracion = 0.5f)
        {
            if (panelIlusion == null) yield break;

            // 1. Activar todos los padres hasta el Canvas para que se vea aunque el contenedor esté inactivo en el Inspector
            ActivarPadresHastaCanvas(panelIlusion);
            panelIlusion.SetActive(true);
            panelIlusion.transform.SetAsLastSibling();

            if (panelIlusion.transform.localScale == Vector3.zero || panelIlusion.transform.localScale.x <= 0.05f)
            {
                panelIlusion.transform.localScale = Vector3.one;
            }

            CanvasGroup cg = panelIlusion.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelIlusion.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.blocksRaycasts = true;
            cg.interactable = true;

            // 2. Activar todas las imágenes e hijos dentro del panel de la ilusión (excepto la franja de revelación que empieza oculta)
            Transform[] todosHijos = panelIlusion.GetComponentsInChildren<Transform>(true);
            foreach (Transform h in todosHijos)
            {
                if (h != null && h != panelIlusion.transform)
                {
                    if (franjaAsociada != null && (h.gameObject == franjaAsociada.gameObject || h.IsChildOf(franjaAsociada.transform)))
                    {
                        h.gameObject.SetActive(false);
                        continue;
                    }

                    h.gameObject.SetActive(true);
                    if (h.localScale == Vector3.zero || h.localScale.x <= 0.05f)
                    {
                        h.localScale = Vector3.one;
                    }

                    Image img = h.GetComponent<Image>();
                    if (img != null) img.enabled = true;

                    RawImage raw = h.GetComponent<RawImage>();
                    if (raw != null) raw.enabled = true;
                }
            }

            // La franja debe permanecer invisible hasta el momento de revelación
            if (franjaAsociada != null)
            {
                franjaAsociada.alpha = 0f;
                franjaAsociada.gameObject.SetActive(false);
            }

            // Fade suave para que la ilusión aparezca de manera elegante y no de golpe
            float t = 0f;
            float dur = duracion > 0.05f ? duracion : 0.5f;
            while (t < dur)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(t / dur);
                yield return null;
            }
            cg.alpha = 1f;
        }

        private void MostrarIlusionCompleta(GameObject panelIlusion, CanvasGroup franjaAsociada)
        {
            StartCoroutine(MostrarIlusionConFade(panelIlusion, franjaAsociada, duracionFadeRevelacion));
        }

        /// <summary>
        /// Conectar este método al evento OnZonaConfirmada del mapa.
        /// Si la zona confirmada es 1 (Adelson), inicia la estación.
        /// </summary>
        public void OnZonaConfirmadaDesdeMapa(int zona)
        {
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            int seleccionReal = (mapa != null && mapa.ZonaSeleccionada > 0) ? mapa.ZonaSeleccionada : PlayerPrefs.GetInt("Cap2_EstacionSeleccionada", zona);

            if (seleccionReal == 1)
            {
                IniciarEstacion();
            }
        }

        /// <summary>
        /// Arranca la estación completa: teletransporte con fade, diálogos, 3 ilusiones, aplauso y guardado.
        /// </summary>
        public void IniciarEstacion()
        {
            if (estacionIniciada) return;
            estacionIniciada = true;

            gameObject.SetActive(true);
            ActivarPadresHastaCanvas(gameObject);
            AutoDetectarReferenciasSiFaltan();

            StopAllCoroutines();
            StartCoroutine(SecuenciaEstacionCompleta());
        }

        private IEnumerator SecuenciaEstacionCompleta()
        {
            // Ocultar mapa si estuviera abierto
            MapaSeleccionCapitulo2[] mapas = FindObjectsOfType<MapaSeleccionCapitulo2>(true);
            foreach (MapaSeleccionCapitulo2 m in mapas)
            {
                m.OcultarMapa();
            }

            Canvas[] todosLosCanvas = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in todosLosCanvas)
            {
                Transform tMapa1 = canvas.transform.Find("Panel Mapa");
                if (tMapa1 != null) tMapa1.gameObject.SetActive(false);
                Transform tMapa2 = canvas.transform.Find("Mapa");
                if (tMapa2 != null) tMapa2.gameObject.SetActive(false);
            }

            // Ocultar interfaces iniciales
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelConfianza != null) panelConfianza.SetActive(false);

            if (panelIlusion1 != null) panelIlusion1.SetActive(false);
            if (panelIlusion2 != null) panelIlusion2.SetActive(false);
            if (panelIlusion3 != null) panelIlusion3.SetActive(false);

            if (franjaRevelacion1 != null) { franjaRevelacion1.alpha = 0f; franjaRevelacion1.gameObject.SetActive(false); }
            if (franjaRevelacion2 != null) { franjaRevelacion2.alpha = 0f; franjaRevelacion2.gameObject.SetActive(false); }
            if (franjaRevelacion3 != null) { franjaRevelacion3.alpha = 0f; franjaRevelacion3.gameObject.SetActive(false); }

            // Fade a negro inicial
            if (panelFadeNegro == null)
            {
                MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>();
                if (mapa != null)
                {
                    panelFadeNegro = mapa.GetComponentInChildren<CanvasGroup>(true);
                }

                if (panelFadeNegro == null)
                {
                    CanvasGroup[] todosCG = FindObjectsOfType<CanvasGroup>(true);
                    foreach (CanvasGroup cg in todosCG)
                    {
                        string n = cg.gameObject.name.ToLower();
                        if (n.Contains("fade") || n.Contains("negro"))
                        {
                            panelFadeNegro = cg;
                            break;
                        }
                    }
                }
            }

            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                panelFadeNegro.blocksRaycasts = true;
                panelFadeNegro.alpha = 1f;
            }

            // Teletransportar al Guía y la cámara (el jugador permanece en su sitio)
            RealizarTeletransporte();

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

            yield return new WaitForSeconds(0.2f);

            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");

            // FASE 0: INTRODUCCIÓN
            yield return StartCoroutine(MostrarDialogo("¡El cerebro no copia el mundo, lo interpreta! Te mostraré 3 imágenes para ver si tus ojos te engañan."));

            // FASE 1: ILUSIÓN 1 — TABLERO DE ADELSON
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelConfianza != null) panelConfianza.SetActive(false);
            yield return StartCoroutine(MostrarIlusionConFade(panelIlusion1, franjaRevelacion1, duracionFadeRevelacion));

            yield return StartCoroutine(MostrarDialogo("Las casillas A y B... ¿Son de diferente color?"));
            yield return StartCoroutine(EsperarRespuesta("Las casillas A y B... ¿Son de diferente color?", "Son del mismo color.", "Un color parece más oscuro."));

            // Ancla de confianza antes de aplicar el filtro (según el Excel)
            yield return StartCoroutine(MostrarDialogo("Antes de aplicar el filtro, dime una cosa: ¿cuánto apostarías a que tienes razón?"));
            yield return StartCoroutine(EsperarNivelConfianza());

            // Ocultar diálogo temporalmente para que el jugador aprecie la revelación
            if (panelDialogo != null) panelDialogo.SetActive(false);
            yield return new WaitForSeconds(0.2f);

            // Revelación con el filtro/franja
            if (franjaRevelacion1 != null)
            {
                yield return StartCoroutine(AnimarFadeFranja(franjaRevelacion1));
            }
            yield return new WaitForSeconds(0.4f);

            // Explicación simple y divertida para todas las edades
            yield return StartCoroutine(MostrarDialogo("¡Son exactamente del mismo color!"));
            yield return StartCoroutine(MostrarDialogo("Al ver una sombra, tu cerebro inventa que la casilla es más oscura. ¡Construye lo que cree ver!"));

            if (panelIlusion1 != null) panelIlusion1.SetActive(false);
            if (franjaRevelacion1 != null) franjaRevelacion1.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.25f);

            // FASE 2: ILUSIÓN 2 — EL VESTIDO
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelConfianza != null) panelConfianza.SetActive(false);
            yield return StartCoroutine(MostrarIlusionConFade(panelIlusion2, franjaRevelacion2, duracionFadeRevelacion));

            yield return StartCoroutine(MostrarDialogo("Observa estos vestidos. ¿Tienen colores iguales o diferentes?"));
            yield return StartCoroutine(EsperarRespuesta("Observa estos vestidos. ¿Tienen colores iguales o diferentes?", "Son diferentes.", "Son iguales."));

            yield return StartCoroutine(MostrarDialogo("¿Qué tanta seguridad tienes en lo que viste? Elige tu nivel de confianza."));
            yield return StartCoroutine(EsperarNivelConfianza());

            // Ocultar diálogo temporalmente para la revelación
            if (panelDialogo != null) panelDialogo.SetActive(false);
            yield return new WaitForSeconds(0.2f);

            if (franjaRevelacion2 != null)
            {
                yield return StartCoroutine(AnimarFadeFranja(franjaRevelacion2));
            }
            yield return new WaitForSeconds(0.4f);

            yield return StartCoroutine(MostrarDialogo("¡El color base es idéntico!"));
            yield return StartCoroutine(MostrarDialogo("Los colores y luces alrededor engañan a tu cerebro. ¡El contexto cambia todo!"));

            if (panelIlusion2 != null) panelIlusion2.SetActive(false);
            if (franjaRevelacion2 != null) franjaRevelacion2.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.25f);

            // FASE 3: ILUSIÓN 3 — MÜLLER-LYER
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelConfianza != null) panelConfianza.SetActive(false);
            yield return StartCoroutine(MostrarIlusionConFade(panelIlusion3, franjaRevelacion3, duracionFadeRevelacion));

            yield return StartCoroutine(MostrarDialogo("Estas dos líneas. ¿Miden lo mismo?"));
            yield return StartCoroutine(EsperarRespuesta("Estas dos líneas. ¿Miden lo mismo?", "Una parece más larga.", "Miden lo mismo."));

            yield return StartCoroutine(MostrarDialogo("¿Cuánto apostarías esta vez?"));
            yield return StartCoroutine(EsperarNivelConfianza());

            // Ocultar diálogo temporalmente para la revelación
            if (panelDialogo != null) panelDialogo.SetActive(false);
            yield return new WaitForSeconds(0.2f);

            if (franjaRevelacion3 != null)
            {
                yield return StartCoroutine(AnimarFadeFranja(franjaRevelacion3));
            }
            yield return new WaitForSeconds(0.4f);

            yield return StartCoroutine(MostrarDialogo("¡Miden exactamente lo mismo!"));
            yield return StartCoroutine(MostrarDialogo("Las flechas engañaron a tu mente. Lo que vemos o sentimos siempre pasa por una interpretación."));

            if (panelIlusion3 != null) panelIlusion3.SetActive(false);
            if (franjaRevelacion3 != null) franjaRevelacion3.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.25f);

            // FASE 4: CIERRE Y APLAUSO
            yield return StartCoroutine(AnimarAplauso());

            yield return StartCoroutine(MostrarDialogo("¡Genial, " + nombreJugador + "! Ya descubriste que lo que percibimos no siempre es la realidad física."));
            yield return StartCoroutine(MostrarDialogo("¡Gran entrenamiento para tu mente!"));

            if (panelDialogo != null) panelDialogo.SetActive(false);

            // Guardar progreso de esta estación
            PlayerPrefs.SetInt(claveGuardadoEstacion, 1);
            PlayerPrefs.Save();

            // Fade a negro para salir con suavidad
            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                panelFadeNegro.blocksRaycasts = true;
                float t = 0f;
                while (t < duracionFadeNegro)
                {
                    t += Time.deltaTime;
                    panelFadeNegro.alpha = Mathf.Clamp01(t / duracionFadeNegro);
                    yield return null;
                }
                panelFadeNegro.alpha = 1f;
            }

            yield return new WaitForSeconds(0.25f);

            // AbrirMapa recuperará la cámara de retorno sin reactivar las otras estaciones.

            // Ocultar interfaces de la estación
            if (panelIlusion1 != null) panelIlusion1.SetActive(false);
            if (panelIlusion2 != null) panelIlusion2.SetActive(false);
            if (panelIlusion3 != null) panelIlusion3.SetActive(false);
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelConfianza != null) panelConfianza.SetActive(false);

            // Abrir el mapa con el progreso actualizado (marcará con check y bloqueará la estación 1)
            MapaSeleccionCapitulo2 mapaSeleccion = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            bool todasCompletadas = false;
            if (mapaSeleccion != null)
            {
                todasCompletadas = mapaSeleccion.EstanTodasLasEstacionesCompletadas();
                mapaSeleccion.gameObject.SetActive(true);
                mapaSeleccion.AbrirMapa();
            }
            else
            {
                todasCompletadas = (PlayerPrefs.GetInt("Cap2_Estacion1_Completada", 0) == 1)
                                && (PlayerPrefs.GetInt("Cap2_Estacion2_Completada", 0) == 1)
                                && (PlayerPrefs.GetInt("Cap2_Estacion3_Completada", 0) == 1)
                                && (PlayerPrefs.GetInt("Cap2_Estacion4_Completada", 0) == 1);
            }

            // Solo desvanecer el fade negro para mostrar el mapa si NO se han completado todas las estaciones
            if (!todasCompletadas && panelFadeNegro != null)
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

            onEstacionCompletada?.Invoke();
        }

        // Teletransporta al Guía y ajusta la cámara (el jugador permanece en su sitio para concentrarse en la imagen)
        private void RealizarTeletransporte()
        {
            // 1. Guía
            if (transformGuia != null && puntoDestinoGuia != null)
            {
                if (guiaAnimator != null) guiaAnimator.applyRootMotion = false;
                transformGuia.position = puntoDestinoGuia.position;
                transformGuia.rotation = puntoDestinoGuia.rotation;
            }

            // 2. Cámara de la estación
            if (camaraEstacion != null)
            {
                if (puntoDestinoCamara != null)
                {
                    camaraEstacion.transform.position = puntoDestinoCamara.position;
                    camaraEstacion.transform.rotation = puntoDestinoCamara.rotation;
                }

                Camera camEstacionComp = CamarasCapitulo2.Activar(camaraEstacion);
                if (camEstacionComp != null) camEstacionComp.depth = 20;
            }
            else if (transformCamara != null && puntoDestinoCamara != null)
            {
                transformCamara.position = puntoDestinoCamara.position;
                transformCamara.rotation = puntoDestinoCamara.rotation;
                CamarasCapitulo2.Activar(transformCamara.gameObject);
            }

            Physics.SyncTransforms();
        }

        // Muestra un texto con efecto Typewriter y espera a que el jugador pulse Continuar
        private IEnumerator MostrarDialogo(string texto)
        {
            if (panelDialogo != null)
            {
                ActivarPadresHastaCanvas(panelDialogo);
                panelDialogo.SetActive(true);
            }
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);

            textoCompletoActual = texto;
            textoTerminado = false;
            esperandoContinuar = true;

            if (textoDialogo != null) textoDialogo.text = "";
            if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, true);

            foreach (char letra in texto)
            {
                if (textoTerminado) break;

                if (textoDialogo != null) textoDialogo.text += letra;
                yield return new WaitForSeconds(Mathf.Max(0.01f, velocidadTypewriter * 0.8f));
            }

            if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
            textoTerminado = true;
            if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, false);

            // Esperar el clic de continuación del jugador
            while (esperandoContinuar)
            {
                yield return null;
            }
        }

        // Avanza el diálogo o completa el typewriter si aún estaba escribiendo
        public void OnClickContinuar()
        {
            if (Time.time - tiempoUltimoClick < 0.12f) return;
            tiempoUltimoClick = Time.time;

            // Si está a mitad de escribir, mostrarlo de golpe inmediatamente
            if (!textoTerminado)
            {
                textoTerminado = true;
                if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
                if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, false);
                return;
            }

            // Si ya estaba completo, avanzar al siguiente diálogo
            if (esperandoContinuar)
            {
                esperandoContinuar = false;
            }
        }

        private IEnumerator EsperarRespuesta(string textoA, string textoB)
        {
            return EsperarRespuesta("", textoA, textoB);
        }

        private IEnumerator EsperarRespuesta(string pregunta, string textoA, string textoB)
        {
            respuestaConfirmada = false;
            opcionSeleccionada = 0;

            if (textoPreguntaEnRespuestas != null && !string.IsNullOrEmpty(pregunta))
            {
                textoPreguntaEnRespuestas.text = pregunta;
            }

            if (textoOpcionA != null) textoOpcionA.text = textoA;
            if (textoOpcionB != null) textoOpcionB.text = textoB;

            if (checkOpcionA != null)
            {
                checkOpcionA.SetActive(false);
                checkOpcionA.transform.localScale = escalaBaseCheckA;
            }
            if (checkOpcionB != null)
            {
                checkOpcionB.SetActive(false);
                checkOpcionB.transform.localScale = escalaBaseCheckB;
            }

            if (botonResponder != null)
            {
                botonResponder.gameObject.SetActive(false);
                botonResponder.transform.localScale = escalaBaseBotonResponder;
            }

            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelRespuestas != null)
            {
                ActivarPadresHastaCanvas(panelRespuestas);
                panelRespuestas.SetActive(true);
            }

            while (!respuestaConfirmada)
            {
                yield return null;
            }

            if (panelRespuestas != null) panelRespuestas.SetActive(false);
        }

        public void SeleccionarOpcionA()
        {
            opcionSeleccionada = 1;

            if (checkOpcionB != null)
            {
                checkOpcionB.SetActive(false);
                checkOpcionB.transform.localScale = escalaBaseCheckB;
            }

            if (checkOpcionA != null)
            {
                if (coroutinePopCheck != null) StopCoroutine(coroutinePopCheck);
                coroutinePopCheck = StartCoroutine(AnimacionPopSuave(checkOpcionA, escalaBaseCheckA));
            }

            MostrarBotonResponderConPop();
        }

        public void SeleccionarOpcionB()
        {
            opcionSeleccionada = 2;

            if (checkOpcionA != null)
            {
                checkOpcionA.SetActive(false);
                checkOpcionA.transform.localScale = escalaBaseCheckA;
            }

            if (checkOpcionB != null)
            {
                if (coroutinePopCheck != null) StopCoroutine(coroutinePopCheck);
                coroutinePopCheck = StartCoroutine(AnimacionPopSuave(checkOpcionB, escalaBaseCheckB));
            }

            MostrarBotonResponderConPop();
        }

        private void MostrarBotonResponderConPop()
        {
            if (botonResponder != null && !botonResponder.gameObject.activeSelf)
            {
                if (coroutinePopBoton != null) StopCoroutine(coroutinePopBoton);
                coroutinePopBoton = StartCoroutine(AnimacionPopSuave(botonResponder.gameObject, escalaBaseBotonResponder));
            }
        }

        public void ConfirmarRespuesta()
        {
            if (opcionSeleccionada == 0) return;
            respuestaConfirmada = true;
        }

        private IEnumerator EsperarNivelConfianza()
        {
            // Failsafe: si el panel de confianza no está asignado todavía en Unity, no bloquea el juego
            if (panelConfianza == null) yield break;

            confianzaConfirmada = false;
            nivelConfianzaSeleccionado = 0;

            if (checksConfianza != null)
            {
                for (int i = 0; i < checksConfianza.Length; i++)
                {
                    if (checksConfianza[i] != null) checksConfianza[i].SetActive(false);
                }
            }

            if (botonConfirmarConfianza != null)
            {
                botonConfirmarConfianza.gameObject.SetActive(false);
            }

            if (panelDialogo != null) panelDialogo.SetActive(false);
            ActivarPadresHastaCanvas(panelConfianza);
            panelConfianza.SetActive(true);
            panelConfianza.transform.SetAsLastSibling();

            while (!confianzaConfirmada)
            {
                yield return null;
            }

            panelConfianza.SetActive(false);
        }

        public void SeleccionarNivelConfianza(int nivel)
        {
            nivelConfianzaSeleccionado = nivel;

            // Desactivar otros checks y activar el elegido con pop
            if (checksConfianza != null)
            {
                for (int i = 0; i < checksConfianza.Length; i++)
                {
                    if (checksConfianza[i] != null)
                    {
                        if (i == nivel - 1)
                        {
                            StartCoroutine(AnimacionPopSuave(checksConfianza[i]));
                        }
                        else
                        {
                            checksConfianza[i].SetActive(false);
                        }
                    }
                }
            }

            // Mostrar el botón de confirmar con pop si aún no estaba visible
            if (botonConfirmarConfianza != null && !botonConfirmarConfianza.gameObject.activeSelf)
            {
                if (coroutinePopBotonConfianza != null) StopCoroutine(coroutinePopBotonConfianza);
                coroutinePopBotonConfianza = StartCoroutine(AnimacionPopSuave(botonConfirmarConfianza.gameObject));
            }
        }

        public void ConfirmarNivelConfianza()
        {
            if (nivelConfianzaSeleccionado == 0) return;
            confianzaConfirmada = true;
        }

        private IEnumerator AnimarFadeFranja(CanvasGroup franja)
        {
            if (franja == null) yield break;

            ActivarPadresHastaCanvas(franja.gameObject);
            franja.gameObject.SetActive(true);
            franja.transform.SetAsLastSibling();
            franja.blocksRaycasts = false;
            franja.alpha = 0f;

            if (franja.transform.localScale == Vector3.zero || franja.transform.localScale.x <= 0.05f)
            {
                franja.transform.localScale = Vector3.one;
            }

            // Activar todos los componentes e imágenes dentro de la franja por si estaban desactivados
            Transform[] hijosFranja = franja.GetComponentsInChildren<Transform>(true);
            foreach (Transform hijoTr in hijosFranja)
            {
                if (hijoTr != null)
                {
                    hijoTr.gameObject.SetActive(true);
                    if (hijoTr.localScale == Vector3.zero || hijoTr.localScale.x <= 0.05f)
                    {
                        hijoTr.localScale = Vector3.one;
                    }
                    Image img = hijoTr.GetComponent<Image>();
                    if (img != null) img.enabled = true;
                    RawImage raw = hijoTr.GetComponent<RawImage>();
                    if (raw != null) raw.enabled = true;
                }
            }

            float tiempoFade = 0f;
            while (tiempoFade < duracionFadeRevelacion)
            {
                tiempoFade += Time.deltaTime;
                franja.alpha = Mathf.Clamp01(tiempoFade / duracionFadeRevelacion);
                yield return null;
            }

            franja.alpha = 1f;

            // Mantener el diálogo al frente si está activo
            if (panelDialogo != null && panelDialogo.activeSelf)
            {
                panelDialogo.transform.SetAsLastSibling();
            }
        }

        private IEnumerator AnimarAplauso()
        {
            if (guiaAnimator != null)
            {
                guiaAnimator.SetBool(parametroHablando, false);
                guiaAnimator.CrossFade(estadoAplaudir, 0.15f);
            }

            yield return new WaitForSeconds(duracionAplauso);

            if (guiaAnimator != null)
            {
                guiaAnimator.CrossFade(estadoIdle, 0.2f);
            }
        }

        private IEnumerator AnimacionPopSuave(GameObject targetObj, float escalaMax = 1.15f, float duracion = 0.22f)
        {
            Vector3 escalaDestino = Vector3.one;
            if (targetObj != null && targetObj.transform.localScale.x > 0.1f)
            {
                escalaDestino = targetObj.transform.localScale;
            }
            return AnimacionPopSuave(targetObj, escalaDestino, escalaMax, duracion);
        }

        private IEnumerator AnimacionPopSuave(GameObject targetObj, Vector3 escalaFinal, float escalaMax = 1.15f, float duracion = 0.22f)
        {
            if (targetObj == null) yield break;

            if (escalaFinal == Vector3.zero || escalaFinal.x <= 0.05f)
            {
                escalaFinal = Vector3.one;
            }

            targetObj.SetActive(true);
            Transform t = targetObj.transform;
            t.localScale = Vector3.zero;

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

                t.localScale = escalaFinal * escalaMultiplicador;
                yield return null;
            }

            t.localScale = escalaFinal;
        }
    }
}
