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
        [SerializeField] private float velocidadTypewriter = 0.03f;

        [Header("--- UI de Preguntas y Respuestas ---")]
        [SerializeField] private GameObject panelRespuestas;
        [SerializeField] private Button botonOpcionA;
        [SerializeField] private TMP_Text textoOpcionA;
        [SerializeField] private GameObject checkOpcionA;
        [Space(4)]
        [SerializeField] private Button botonOpcionB;
        [SerializeField] private TMP_Text textoOpcionB;
        [SerializeField] private GameObject checkOpcionB;
        [Space(4)]
        [SerializeField] private Button botonResponder;

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
        [SerializeField] private float duracionFadeRevelacion = 0.6f;
        [SerializeField] private float duracionFadeNegro = 0.5f;

        [Header("--- Animaciones de Personajes ---")]
        [SerializeField] private Animator guiaAnimator;
        [SerializeField] private string parametroHablando = "Hablando";
        [SerializeField] private string estadoAplaudir = "Clapping";
        [SerializeField] private string estadoIdle = "Idle";
        [SerializeField] private float duracionAplauso = 2.2f;

        [Header("--- Fade a Negro de Pantalla ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;

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
        private Coroutine coroutinePopBoton;
        private Coroutine coroutinePopCheck;

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
        }

        private void OnEnable()
        {
            ConectarConMapa();
        }

        private void ConectarConMapa()
        {
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>();
            if (mapa != null)
            {
                mapa.onZonaConfirmada.RemoveListener(OnZonaConfirmadaDesdeMapa);
                mapa.onZonaConfirmada.AddListener(OnZonaConfirmadaDesdeMapa);
            }
        }

        private void Start()
        {
            ConectarConMapa();

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

            if (iniciarAlEmpezar)
            {
                IniciarEstacion();
            }
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
            yield return StartCoroutine(MostrarDialogo("El cerebro no copia el mundo, lo interpreta."));
            yield return StartCoroutine(MostrarDialogo("Te voy a mostrar tres imágenes. En cada una, tu cerebro va a querer decirte algo…"));
            yield return StartCoroutine(MostrarDialogo("…Vamos a ver si tiene razón."));
            yield return StartCoroutine(MostrarDialogo("¡Primera pregunta!"));

            // FASE 1: ILUSIÓN 1 — TABLERO DE ADELSON
            if (panelIlusion1 != null)
            {
                panelIlusion1.SetActive(true);
                panelIlusion1.transform.SetAsLastSibling();
            }

            if (panelDialogo != null) panelDialogo.transform.SetAsLastSibling();
            if (panelRespuestas != null) panelRespuestas.transform.SetAsLastSibling();

            if (franjaRevelacion1 != null) { franjaRevelacion1.alpha = 0f; franjaRevelacion1.gameObject.SetActive(false); }

            yield return StartCoroutine(MostrarDialogo("Las casillas A y B... ¿Son de diferente color?"));
            yield return StartCoroutine(EsperarRespuesta("A y B son el mismo color.", "Son diferente color."));

            if (franjaRevelacion1 != null)
            {
                StartCoroutine(AnimarFadeFranja(franjaRevelacion1));
            }

            yield return StartCoroutine(MostrarDialogo("¡Es el mismo color!"));
            yield return StartCoroutine(MostrarDialogo("El cerebro piensa que una casilla debe ser más oscura solo por tener sombra"));
            yield return StartCoroutine(MostrarDialogo("…En vez de comparar el color real."));
            yield return StartCoroutine(MostrarDialogo("Es un atajo que hace nuestro cerebro para reconocer rápidamente objetos bajo diferentes luces!"));
            yield return StartCoroutine(MostrarDialogo("¡Segunda pregunta!"));

            if (panelIlusion1 != null) panelIlusion1.SetActive(false);

            // FASE 2: ILUSIÓN 2 — EL VESTIDO
            if (panelIlusion2 != null)
            {
                panelIlusion2.SetActive(true);
                panelIlusion2.transform.SetAsLastSibling();
            }

            if (panelDialogo != null) panelDialogo.transform.SetAsLastSibling();
            if (panelRespuestas != null) panelRespuestas.transform.SetAsLastSibling();

            if (franjaRevelacion2 != null) { franjaRevelacion2.alpha = 0f; franjaRevelacion2.gameObject.SetActive(false); }

            yield return StartCoroutine(MostrarDialogo("Observa estos vestidos. ¿Tienen colores iguales?"));
            yield return StartCoroutine(EsperarRespuesta("Son diferentes.", "Son iguales."));

            if (franjaRevelacion2 != null)
            {
                StartCoroutine(AnimarFadeFranja(franjaRevelacion2));
            }

            yield return StartCoroutine(MostrarDialogo("¡Es el mismo color!"));
            yield return StartCoroutine(MostrarDialogo("Mira las franjas que conectan ambos vestidos, " + nombreJugador + "… ¡son exactamente el mismo color!"));
            yield return StartCoroutine(MostrarDialogo("Pero al estar rodeadas de azul en un vestido y de amarillo en el otro, parecen colores distintos."));
            yield return StartCoroutine(MostrarDialogo("¡Esto pasa porque tu cerebro no ve cada color de forma separada, siempre ve la escena completa y lo interpreta!"));
            yield return StartCoroutine(MostrarDialogo("¡Última pregunta!"));

            if (panelIlusion2 != null) panelIlusion2.SetActive(false);

            // FASE 3: ILUSIÓN 3 — MÜLLER-LYER
            if (panelIlusion3 != null)
            {
                panelIlusion3.SetActive(true);
                panelIlusion3.transform.SetAsLastSibling();
            }

            if (panelDialogo != null) panelDialogo.transform.SetAsLastSibling();
            if (panelRespuestas != null) panelRespuestas.transform.SetAsLastSibling();

            if (franjaRevelacion3 != null) { franjaRevelacion3.alpha = 0f; franjaRevelacion3.gameObject.SetActive(false); }

            yield return StartCoroutine(MostrarDialogo("Por último, estas dos líneas. ¿Miden lo mismo?"));
            yield return StartCoroutine(EsperarRespuesta("Sí, son iguales.", "No, una es más larga."));

            if (franjaRevelacion3 != null)
            {
                StartCoroutine(AnimarFadeFranja(franjaRevelacion3));
            }

            yield return StartCoroutine(MostrarDialogo("¡Son del mismo tamaño!"));
            yield return StartCoroutine(MostrarDialogo("Las flechas en las puntas engañan a tu cerebro."));
            yield return StartCoroutine(MostrarDialogo("Las que apuntan hacia afuera hacen ver la línea más larga..."));
            yield return StartCoroutine(MostrarDialogo("Las que apuntan hacia adentro la hacen ver más corta."));
            yield return StartCoroutine(MostrarDialogo("Pero si tapas las flechas y solo miras las líneas… son idénticas."));

            if (panelIlusion3 != null) panelIlusion3.SetActive(false);

            // FASE 4: CIERRE Y APLAUSO
            yield return StartCoroutine(AnimarAplauso());

            yield return StartCoroutine(MostrarDialogo("¡Genial, " + nombreJugador + "! Completaste la estación: El tablero de Adelson."));
           yield return StartCoroutine(MostrarDialogo("La percepción visual no siempre refleja la realidad física."));
            yield return StartCoroutine(MostrarDialogo("Pero ver la grieta entre sensación e interpretación ya es una victoria."));
            yield return StartCoroutine(MostrarDialogo("¡Excelente trabajo, " + nombreJugador + "! Volvamos para seguir explorando"));

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

            // Restaurar cámaras si se utilizó cámara dedicada para la estación
            if (camaraEstacion != null)
            {
                camaraEstacion.SetActive(false);
                Camera[] todasLasCamaras = Resources.FindObjectsOfTypeAll<Camera>();
                foreach (Camera c in todasLasCamaras)
                {
                    if (c.gameObject != camaraEstacion && !c.transform.IsChildOf(camaraEstacion.transform))
                    {
                        if (c.gameObject.scene.isLoaded)
                        {
                            c.gameObject.SetActive(true);
                            c.enabled = true;
                        }
                    }
                }
            }

            // Ocultar interfaces de la estación
            if (panelIlusion1 != null) panelIlusion1.SetActive(false);
            if (panelIlusion2 != null) panelIlusion2.SetActive(false);
            if (panelIlusion3 != null) panelIlusion3.SetActive(false);
            if (panelRespuestas != null) panelRespuestas.SetActive(false);

            // Abrir el mapa con el progreso actualizado (marcará con check y bloqueará la estación 1)
            MapaSeleccionCapitulo2 mapaSeleccion = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapaSeleccion != null)
            {
                mapaSeleccion.gameObject.SetActive(true);
                mapaSeleccion.AbrirMapa();
            }

            // Fade desde negro para mostrar el mapa limpiamente
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

                Camera camEstacionComp = camaraEstacion.GetComponent<Camera>();
                if (camEstacionComp == null) camEstacionComp = camaraEstacion.GetComponentInChildren<Camera>();

                Camera[] todasLasCamaras = FindObjectsOfType<Camera>();
                foreach (Camera c in todasLasCamaras)
                {
                    if (c.gameObject != camaraEstacion && !c.transform.IsChildOf(camaraEstacion.transform))
                    {
                        if (c.cullingMask != (1 << LayerMask.NameToLayer("UI")))
                        {
                            c.gameObject.SetActive(false);
                        }
                    }
                }

                camaraEstacion.SetActive(true);
                if (camEstacionComp != null)
                {
                    camEstacionComp.enabled = true;
                    camEstacionComp.depth = 20;
                }
            }
            else if (transformCamara != null && puntoDestinoCamara != null)
            {
                transformCamara.position = puntoDestinoCamara.position;
                transformCamara.rotation = puntoDestinoCamara.rotation;
            }

            Physics.SyncTransforms();
        }

        // Muestra un texto con efecto Typewriter y espera a que el jugador pulse Continuar
        private IEnumerator MostrarDialogo(string texto)
        {
            if (panelDialogo != null) panelDialogo.SetActive(true);
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
                yield return new WaitForSeconds(velocidadTypewriter);
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
            respuestaConfirmada = false;
            opcionSeleccionada = 0;

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
            if (panelRespuestas != null) panelRespuestas.SetActive(true);

            while (!respuestaConfirmada)
            {
                yield return null;
            }

            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelDialogo != null) panelDialogo.SetActive(true);
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

        private IEnumerator AnimarFadeFranja(CanvasGroup franja)
        {
            if (franja == null) yield break;

            franja.gameObject.SetActive(true);
            franja.alpha = 0f;

            float t = 0f;
            while (t < duracionFadeRevelacion)
            {
                t += Time.deltaTime;
                franja.alpha = Mathf.Clamp01(t / duracionFadeRevelacion);
                yield return null;
            }

            franja.alpha = 1f;
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
