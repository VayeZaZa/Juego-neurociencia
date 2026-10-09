using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

namespace Capitulo2
{
    [AddComponentMenu("Capitulo 2/Minijuego Ferrari")]
    public class MinijuegoFerrariCapitulo2 : MonoBehaviour
    {
        [Header("--- Avatares del Jugador ---")]
        [Tooltip("Modelo Hombre en la escena")]
        [SerializeField] private GameObject jugadorHombre;
        [Tooltip("Modelo Mujer en la escena")]
        [SerializeField] private GameObject jugadorMujer;
        [SerializeField] private string animCorrerHerido = "Injured Run";
        [SerializeField] private string animIdle = "Happy Idle";
        [SerializeField] private float duracionTransicionIdle = 1.2f;

        [Header("--- Puntos de Recorrido del Jugador ---")]
        [Tooltip("Posición inicial en la acera")]
        [SerializeField] private Transform puntoInicioJugador;
        [Tooltip("Punto tras cruzar el carril 1")]
        [SerializeField] private Transform puntoCruce1;
        [Tooltip("Punto tras cruzar el carril 2")]
        [SerializeField] private Transform puntoCruce2;
        [Tooltip("Punto final seguro fuera de la carretera")]
        [SerializeField] private Transform puntoFinalSeguro;
        [SerializeField] private float duracionAvanceJugador = 1.2f;

        [Header("--- Guía esperando en la zona segura ---")]
        [Tooltip("Transform raíz del Guía que aparecerá esperando al jugador")]
        [SerializeField] private Transform transformGuia;
        [Tooltip("Punto donde se ubicará el Guía durante este minijuego")]
        [SerializeField] private Transform puntoGuiaEsperando;
        [SerializeField, Min(1f)] private float multiplicadorTamanoGuia = 1.08f;
        [SerializeField] private float alturaExtraGuia = 0.08f;

        [Header("--- Fase 3: Caminata lenta (sin peligro) ---")]
        [Tooltip("Animación que se usa al caminar sin peligro. Si tienes 'Walking' en el Animator, puedes ponerla aquí")]
        [SerializeField] private string animCaminataSegura = "Injured Run";
        [Tooltip("1 = velocidad normal. Menos de 1 = más lento")]
        [SerializeField] private float velocidadAnimCaminataSegura = 0.4f;
        [SerializeField] private float duracionCaminataSegura = 4.5f;

        [Header("--- UI Recuerdo (texto libre del jugador) ---")]
        [SerializeField] private GameObject panelRecuerdo;
        [SerializeField] private CanvasGroup grupoFadeRecuerdo;
        [SerializeField] private TMP_Text tituloRecuerdo;
        [SerializeField] private TMP_Text textoInstruccionRecuerdo;
        [SerializeField] private TMP_Text textoPreviewRecuerdo;
        [SerializeField] private GameObject panelRecuerdoPaso1;
        [SerializeField] private GameObject panelRecuerdoPaso2;
        [SerializeField] private Button botonSiguienteRecuerdo;
        [SerializeField] private GameObject botonSiguienteVerde;
        [SerializeField] private TMP_InputField campoRecuerdo;
        [SerializeField] private Button botonGuardarRecuerdo;
        [SerializeField] private GameObject botonGuardarVerde;
        [SerializeField] private float duracionFadeRecuerdo = 0.35f;
        [SerializeField] private float velocidadTypewriterRecuerdo = 0.03f;
        [SerializeField] private string claveRecuerdo = "Cap2_RecuerdoDolor";

        [Header("--- Cámara del Minijuego ---")]
        [SerializeField] private GameObject camaraMinijuego;
        [SerializeField] private Transform transformCamara;
        [Tooltip("Si está activo, la cámara sigue al personaje automáticamente manteniendo su distancia y ángulo lateral")]
        [SerializeField] private bool seguirJugadorLateral = true;
        [Tooltip("Posición lateral inicial de la cámara junto al personaje")]
        [SerializeField] private Transform puntoCamaraInicio;
        [Tooltip("(Opcional) Si seguirJugadorLateral es falso, se usan estos puntos")]
        [SerializeField] private Transform puntoCamaraCruce1;
        [SerializeField] private Transform puntoCamaraCruce2;
        [SerializeField] private Transform puntoCamaraFinal;
        [SerializeField] private float duracionMoverCamara = 1.0f;

        [Header("--- Los 3 Ferraris / Carros ---")]
        [Tooltip("Los 3 carros que avanzarán en línea recta")]
        [SerializeField] private Transform[] carros = new Transform[3];
        [SerializeField] private Transform[] puntosInicioCarro = new Transform[3];
        [SerializeField] private Transform[] puntosFinCarro = new Transform[3];
        [SerializeField] private float duracionPasoCarro = 1.6f;

        [Header("--- UI Fase 1: Botón ¡CORRE! ---")]
        [SerializeField] private GameObject panelFase1Corre;
        [SerializeField] private Button botonCorre;
        [Tooltip("Barra horizontal (RectTransform o Image) que se encoge hacia el centro")]
        [SerializeField] private RectTransform barraTiempoFase1;
        [SerializeField] private Image imagenBarraFase1;
        [SerializeField] private float tiempoLimiteFase1 = 2.4f;

        [Header("--- UI Fase 2: Decisión (2A y 2B) ---")]
        [SerializeField] private GameObject panelFase2Decision;
        [SerializeField] private Button botonOpcion2A;
        [SerializeField] private Button botonOpcion2B;
        [SerializeField] private RectTransform barraTiempoFase2;
        [SerializeField] private Image imagenBarraFase2;
        [SerializeField] private float tiempoLimiteFase2 = 2.8f;

        [Header("--- UI Pantalla de Fallo (Rojo) ---")]
        [SerializeField] private CanvasGroup panelFalloRojo;
        [SerializeField] private Button botonReintentar;

        [Header("--- UI Diálogos y Conclusión ---")]
        [SerializeField] private GameObject panelDialogo;
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuarDialogo;
        [SerializeField] private float velocidadTypewriter = 0.03f;

        [Header("--- Fade a Negro de Pantalla ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        [SerializeField] private float duracionFadeNegro = 0.5f;

        [Header("--- Guardado de Progreso ---")]
        [SerializeField] private string claveGuardadoEstacion = "Cap2_Estacion2_Completada";
        public UnityEvent onMinijuegoCompletado;

        [Header("--- Debug / Pruebas ---")]
        [Tooltip("Si está activo, el minijuego arranca solo al dar Play en Unity")]
        [SerializeField] private bool iniciarAlEmpezar = false;

        // Variables internas
        private GameObject jugadorActivo;
        private Animator animJugadorActivo;
        private Vector3 posicionOriginalGuia;
        private Quaternion rotacionOriginalGuia;
        private Vector3 escalaOriginalGuia;
        private bool guiaActivoOriginalmente;
        private bool posicionOriginalGuiaGuardada;
        private bool enJuego = false;
        private bool esperandoContinuar = false;
        private bool textoTerminado = false;
        private string textoCompletoActual = "";
        private float tiempoUltimoClick = 0f;
        private Vector3 offsetCamaraLateral = Vector3.zero;
        private Quaternion rotacionCamaraLateral = Quaternion.identity;

        private void Awake()
        {
            // Seguridad: Si este componente fue añadido a un punto vacío sin configurar, auto-desactivarse
            if (puntoInicioJugador == null && botonCorre == null)
            {
                enabled = false;
                return;
            }

            ConectarConCinematica();

            if (panelFalloRojo != null)
            {
                panelFalloRojo.alpha = 0f;
                panelFalloRojo.blocksRaycasts = false;
                panelFalloRojo.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (puntoInicioJugador == null && botonCorre == null)
            {
                enabled = false;
                return;
            }
            ConectarConCinematica();
        }

        private void ConectarConCinematica()
        {
            EstacionFerrariCapitulo2 cinematica = BuscarCinematicaFerrari();
            if (cinematica != null)
            {
                cinematica.alTerminarCinematica.RemoveListener(IniciarMinijuego);
                cinematica.alTerminarCinematica.AddListener(IniciarMinijuego);
            }
        }

        private EstacionFerrariCapitulo2 BuscarCinematicaFerrari()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<EstacionFerrariCapitulo2>();
#else
            return FindObjectOfType<EstacionFerrariCapitulo2>();
#endif
        }

        private void Start()
        {
            if (puntoInicioJugador == null && botonCorre == null) return;

            ConectarConCinematica();

            // Desactivar paneles inicialmente
            if (panelFase1Corre != null) panelFase1Corre.SetActive(false);
            if (panelFase2Decision != null) panelFase2Decision.SetActive(false);
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelRecuerdo != null) panelRecuerdo.SetActive(false);

            if (botonReintentar != null)
            {
                botonReintentar.onClick.RemoveListener(ReiniciarMinijuego);
                botonReintentar.onClick.AddListener(ReiniciarMinijuego);
            }

            if (botonContinuarDialogo != null)
            {
                botonContinuarDialogo.onClick.RemoveListener(OnClickContinuarDialogo);
                botonContinuarDialogo.onClick.AddListener(OnClickContinuarDialogo);
            }

            if (campoRecuerdo != null)
            {
                campoRecuerdo.onValueChanged.RemoveListener(ActualizarPreviewRecuerdo);
                campoRecuerdo.onValueChanged.AddListener(ActualizarPreviewRecuerdo);
            }

            if (panelRecuerdo != null)
            {
                panelRecuerdo.SetActive(false);
                if (grupoFadeRecuerdo == null)
                {
                    grupoFadeRecuerdo = panelRecuerdo.GetComponent<CanvasGroup>();
                }
                if (grupoFadeRecuerdo != null)
                {
                    grupoFadeRecuerdo.alpha = 0f;
                    grupoFadeRecuerdo.interactable = false;
                    grupoFadeRecuerdo.blocksRaycasts = false;
                }
            }

            if (botonSiguienteVerde != null) botonSiguienteVerde.SetActive(false);
            if (botonGuardarVerde != null) botonGuardarVerde.SetActive(false);
            if (botonSiguienteRecuerdo != null) botonSiguienteRecuerdo.gameObject.SetActive(true);
            if (botonGuardarRecuerdo != null) botonGuardarRecuerdo.gameObject.SetActive(true);

            if (iniciarAlEmpezar)
            {
                IniciarMinijuego();
            }
        }

        /// <summary>
        /// Inicia el minijuego de los 3 cruces.
        /// </summary>
        public void IniciarMinijuego()
        {
            if (enJuego) return;
            enJuego = true;

            StopAllCoroutines();
            StartCoroutine(SecuenciaMinijuegoCompleta());
        }

        public void ReiniciarMinijuego()
        {
            StopAllCoroutines();
            enJuego = false;
            IniciarMinijuego();
        }

        private IEnumerator SecuenciaMinijuegoCompleta()
        {
            // 0. Auto-completar referencias desde la cinemática si faltan
            EstacionFerrariCapitulo2 cinematicaRef = BuscarCinematicaFerrari();
            if (cinematicaRef != null)
            {
                if (jugadorHombre == null) jugadorHombre = cinematicaRef.JugadorHombre;
                if (jugadorMujer == null)  jugadorMujer  = cinematicaRef.JugadorMujer;

                // Apagar la cámara de la cinemática si es diferente a la del minijuego para que no tape la vista (depth 20)
                if (cinematicaRef.CamaraEstacion != null && cinematicaRef.CamaraEstacion != camaraMinijuego)
                {
                    cinematicaRef.CamaraEstacion.SetActive(false);
                }

                // Ocultar cualquier fade negro remanente de la cinemática
                if (cinematicaRef.PanelFadeNegro != null && cinematicaRef.PanelFadeNegro != panelFadeNegro)
                {
                    cinematicaRef.PanelFadeNegro.alpha = 0f;
                    cinematicaRef.PanelFadeNegro.blocksRaycasts = false;
                    cinematicaRef.PanelFadeNegro.gameObject.SetActive(false);
                }
            }

            // Ocultar cualquier UI de fallo
            if (panelFalloRojo != null)
            {
                panelFalloRojo.alpha = 0f;
                panelFalloRojo.blocksRaycasts = false;
                panelFalloRojo.gameObject.SetActive(false);
            }
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelFase2Decision != null) panelFase2Decision.SetActive(false);
            if (panelRecuerdo != null) panelRecuerdo.SetActive(false);
            if (botonReintentar != null) botonReintentar.gameObject.SetActive(false);

            // 1. Configurar avatar del jugador según su selección previa
            int avatarElegido = PlayerPrefs.GetInt("Avatar", 1);
            if (avatarElegido == 1)
            {
                if (jugadorHombre != null) jugadorHombre.SetActive(true);
                if (jugadorMujer != null) jugadorMujer.SetActive(false);
                jugadorActivo = jugadorHombre;
            }
            else
            {
                if (jugadorMujer != null) jugadorMujer.SetActive(true);
                if (jugadorHombre != null) jugadorHombre.SetActive(false);
                jugadorActivo = jugadorMujer;
            }

            if (jugadorActivo != null)
            {
                animJugadorActivo = jugadorActivo.GetComponent<Animator>();
                if (animJugadorActivo == null) animJugadorActivo = jugadorActivo.GetComponentInChildren<Animator>();

                if (puntoInicioJugador != null)
                {
                    if (animJugadorActivo != null) animJugadorActivo.applyRootMotion = false;
                    jugadorActivo.transform.position = puntoInicioJugador.position;
                    jugadorActivo.transform.rotation = puntoInicioJugador.rotation;

                    if (jugadorActivo.transform.childCount > 0)
                    {
                        jugadorActivo.transform.GetChild(0).localRotation = Quaternion.identity;
                    }
                }

                if (animJugadorActivo != null)
                {
                    animJugadorActivo.speed = 1f;
                    animJugadorActivo.Play(animCorrerHerido, -1, 0f);
                }
            }

            ColocarGuiaEsperando();

            // Ubicar carros en sus puntos de origen
            ResetearPosicionesCarros();

            // Configurar y activar cámara del minijuego
            ConfigurarCamara(puntoCamaraInicio);

            yield return new WaitForSeconds(0.1f);

            // Fade desde negro inicial
            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                float tFade = 0f;
                while (tFade < duracionFadeNegro)
                {
                    tFade += Time.deltaTime;
                    panelFadeNegro.alpha = Mathf.Clamp01(1f - (tFade / duracionFadeNegro));
                    yield return null;
                }
                panelFadeNegro.alpha = 0f;
                panelFadeNegro.blocksRaycasts = false;
                panelFadeNegro.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.15f);

            // ==========================================
            // FASE 1: PRIMER CARRO Y BOTÓN "¡CORRE!"
            // ==========================================
            bool pulsoCorre = false;

            // Asegurar que el contenedor padre y sus ancestros de Canvas estén activos y al frente
            if (panelFase1Corre != null)
            {
                panelFase1Corre.SetActive(true);
                panelFase1Corre.transform.SetAsLastSibling();
                if (panelFase1Corre.transform.localScale.x <= 0.05f)
                    panelFase1Corre.transform.localScale = Vector3.one;

                Transform p = panelFase1Corre.transform.parent;
                while (p != null)
                {
                    p.gameObject.SetActive(true);
                    p = p.parent;
                }

                CanvasGroup cg1 = panelFase1Corre.GetComponent<CanvasGroup>();
                if (cg1 != null)
                {
                    cg1.alpha = 1f;
                    cg1.interactable = true;
                    cg1.blocksRaycasts = true;
                }
            }

            if (botonCorre != null)
            {
                botonCorre.onClick.RemoveAllListeners();
                botonCorre.onClick.AddListener(() => pulsoCorre = true);
                botonCorre.gameObject.SetActive(true);
                botonCorre.transform.SetAsLastSibling();
                if (botonCorre.transform.localScale.x <= 0.05f)
                    botonCorre.transform.localScale = Vector3.one;
            }

            if (barraTiempoFase1 != null)
            {
                barraTiempoFase1.gameObject.SetActive(true);
                if (barraTiempoFase1.transform.localScale.y <= 0.05f)
                    barraTiempoFase1.transform.localScale = Vector3.one;
            }
            if (imagenBarraFase1 != null)
            {
                imagenBarraFase1.gameObject.SetActive(true);
            }

            if (panelFase2Decision != null) panelFase2Decision.SetActive(false);
            ActualizarBarra(barraTiempoFase1, imagenBarraFase1, 1f);

            // Iniciar avance veloz del carro 1
            if (carros.Length > 0 && carros[0] != null)
            {
                StartCoroutine(MoverCarroRecta(carros[0], puntosInicioCarro[0], puntosFinCarro[0], duracionPasoCarro));
            }

            float tTimer1 = 0f;
            while (tTimer1 < tiempoLimiteFase1 && !pulsoCorre)
            {
                tTimer1 += Time.deltaTime;
                float ratio = Mathf.Clamp01(1f - (tTimer1 / tiempoLimiteFase1));
                ActualizarBarra(barraTiempoFase1, imagenBarraFase1, ratio);
                yield return null;
            }

            if (!pulsoCorre)
            {
                // Falló en Fase 1
                yield return StartCoroutine(SecuenciaFallo());
                yield break;
            }

            // Acierto Fase 1: Ocultar botón Corre
            if (botonCorre != null) botonCorre.gameObject.SetActive(false);

            if (!seguirJugadorLateral && puntoCamaraCruce1 != null)
            {
                StartCoroutine(MoverCamara(puntoCamaraCruce1, duracionAvanceJugador));
            }
            yield return StartCoroutine(MoverJugadorHacia(puntoCruce1, duracionAvanceJugador));

            yield return new WaitForSeconds(0.25f);

            // ==========================================
            // FASE 2: SEGUNDO CARRO Y DECISIÓN (2A y 2B)
            // ==========================================
            int decisionElegida = 0; // 1 = 2A, 2 = 2B
            if (botonOpcion2A != null)
            {
                botonOpcion2A.onClick.RemoveAllListeners();
                botonOpcion2A.onClick.AddListener(() =>
                {
                    decisionElegida = 1;
                    StartCoroutine(EfectoPopBoton(botonOpcion2A.transform));
                    if (panelFase2Decision != null) panelFase2Decision.SetActive(false);
                });
                botonOpcion2A.gameObject.SetActive(true);
                if (botonOpcion2A.transform.localScale.x <= 0.05f)
                    botonOpcion2A.transform.localScale = Vector3.one;
            }
            if (botonOpcion2B != null)
            {
                botonOpcion2B.onClick.RemoveAllListeners();
                botonOpcion2B.onClick.AddListener(() =>
                {
                    decisionElegida = 2;
                    StartCoroutine(EfectoPopBoton(botonOpcion2B.transform));
                    if (panelFase2Decision != null) panelFase2Decision.SetActive(false);
                });
                botonOpcion2B.gameObject.SetActive(true);
                if (botonOpcion2B.transform.localScale.x <= 0.05f)
                    botonOpcion2B.transform.localScale = Vector3.one;
            }

            // Asegurar que el contenedor padre está activo y al frente
            if (panelFase1Corre != null)
            {
                panelFase1Corre.SetActive(true);
                panelFase1Corre.transform.SetAsLastSibling();
            }

            if (panelFase2Decision != null)
            {
                panelFase2Decision.SetActive(true);
                panelFase2Decision.transform.SetAsLastSibling();
                if (panelFase2Decision.transform.localScale.x <= 0.05f)
                    panelFase2Decision.transform.localScale = Vector3.one;

                CanvasGroup cgF2 = panelFase2Decision.GetComponent<CanvasGroup>();
                if (cgF2 != null)
                {
                    cgF2.alpha = 1f;
                    cgF2.interactable = true;
                    cgF2.blocksRaycasts = true;
                }
            }

            if (barraTiempoFase2 != null)
            {
                barraTiempoFase2.gameObject.SetActive(true);
            }
            if (imagenBarraFase2 != null)
            {
                imagenBarraFase2.gameObject.SetActive(true);
            }

            ActualizarBarra(barraTiempoFase2, imagenBarraFase2, 1f);

            // Iniciar avance veloz del carro 2
            if (carros.Length > 1 && carros[1] != null)
            {
                StartCoroutine(MoverCarroRecta(carros[1], puntosInicioCarro[1], puntosFinCarro[1], duracionPasoCarro));
            }

            float tTimer2 = 0f;
            while (tTimer2 < tiempoLimiteFase2 && decisionElegida == 0)
            {
                tTimer2 += Time.deltaTime;
                float ratio = Mathf.Clamp01(1f - (tTimer2 / tiempoLimiteFase2));
                ActualizarBarra(barraTiempoFase2, imagenBarraFase2, ratio);
                yield return null;
            }

            if (decisionElegida == 0)
            {
                // Falló en Fase 2
                yield return StartCoroutine(SecuenciaFallo());
                yield break;
            }

            // Acierto Fase 2: Ocultar panel de respuestas
            if (!seguirJugadorLateral && puntoCamaraCruce2 != null)
            {
                StartCoroutine(MoverCamara(puntoCamaraCruce2, duracionAvanceJugador));
            }
            yield return StartCoroutine(MoverJugadorHacia(puntoCruce2, duracionAvanceJugador));

            // Diálogo según la opción elegida
            if (decisionElegida == 1)
            {
                yield return StartCoroutine(MostrarDialogo("Tu reacción es válida. Ahora observa cómo el contexto cambia la prioridad."));
            }
            else
            {
                yield return StartCoroutine(MostrarDialogo("El sistema puede modular la intensidad del dolor según el contexto."));
            }

            if (panelDialogo != null) panelDialogo.SetActive(false);

            yield return new WaitForSeconds(0.2f);

            // ==========================================
            // FASE 3: CAMINATA LENTA FINAL A SALVO
            // ==========================================
            // Ya no sale el carro 3: no hay peligro, el personaje camina lento.
            if (animJugadorActivo != null)
            {
                if (animCaminataSegura != animCorrerHerido)
                    animJugadorActivo.CrossFade(animCaminataSegura, 0.2f);
                animJugadorActivo.speed = velocidadAnimCaminataSegura;
            }

            if (!seguirJugadorLateral && puntoCamaraFinal != null)
            {
                StartCoroutine(MoverCamara(puntoCamaraFinal, duracionCaminataSegura));
            }
            yield return StartCoroutine(MoverJugadorHacia(puntoFinalSeguro, duracionCaminataSegura));

            OcultarUIFasesMinijuego();

            if (animJugadorActivo != null)
            {
                animJugadorActivo.SetBool("Walking", false);
                animJugadorActivo.speed = 1f;
                animJugadorActivo.CrossFadeInFixedTime(animIdle, duracionTransicionIdle);
                yield return null; // Esperar un frame para que Unity aplique el estado
            }

            yield return new WaitForSeconds(0.3f);

            // ==========================================
            // FASE 4: DIÁLOGOS DE CONCLUSIÓN Y GUARDADO
            // ==========================================
       yield return StartCoroutine(MostrarDialogo("Hace un momento corrías sin sentirlo. Ahora que estás a salvo, puedes notarlo más."));
       yield return StartCoroutine(MostrarDialogo("Cuando hay peligro, el cerebro puede priorizar escapar y dejar el dolor en segundo plano."));
       yield return StartCoroutine(MostrarDialogo("Por eso, a veces sentimos un golpe o un dolor más fuerte después de que pasa el peligro."));
       yield return StartCoroutine(MostrarDialogo("El dolor no depende solo de una lesión. También depende de lo que está pasando a nuestro alrededor."));
       yield return StartCoroutine(MostrarDialogo("¿Te ha pasado algo parecido? Cuéntalo con tus palabras."));
            if (panelDialogo != null) panelDialogo.SetActive(false);

            yield return StartCoroutine(PedirRecuerdo());

            // Guardar progreso de esta estación (Estación 2)
            PlayerPrefs.SetInt(claveGuardadoEstacion, 1);
            PlayerPrefs.Save();

            // Fade a negro para salir al mapa
            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                panelFadeNegro.blocksRaycasts = true;
                float tFade = 0f;
                while (tFade < duracionFadeNegro)
                {
                    tFade += Time.deltaTime;
                    panelFadeNegro.alpha = Mathf.Clamp01(tFade / duracionFadeNegro);
                    yield return null;
                }
                panelFadeNegro.alpha = 1f;
            }

            yield return new WaitForSeconds(0.2f);

            if (camaraMinijuego != null)
            {
                camaraMinijuego.SetActive(false);
            }

            yield return null;
            RestaurarGuia();

            // Abrir el mapa con el progreso actualizado
            MapaSeleccionCapitulo2 mapaSeleccion = BuscarMapaSeleccion();
            if (mapaSeleccion != null)
            {
                mapaSeleccion.gameObject.SetActive(true);
                mapaSeleccion.AbrirMapa();
            }
            else
            {
                Debug.LogError("No se encontró el mapa de selección del Capítulo 2 al terminar la estación Ferrari.", this);
            }

            if (panelFadeNegro != null)
            {
                float tFade = 0f;
                while (tFade < duracionFadeNegro)
                {
                    tFade += Time.deltaTime;
                    panelFadeNegro.alpha = Mathf.Clamp01(1f - (tFade / duracionFadeNegro));
                    yield return null;
                }
                panelFadeNegro.alpha = 0f;
                panelFadeNegro.blocksRaycasts = false;
                panelFadeNegro.gameObject.SetActive(false);
            }

            enJuego = false;
            onMinijuegoCompletado?.Invoke();
        }

        private void ColocarGuiaEsperando()
        {
            if (transformGuia == null || puntoGuiaEsperando == null) return;

            if (!posicionOriginalGuiaGuardada)
            {
                posicionOriginalGuia = transformGuia.position;
                rotacionOriginalGuia = transformGuia.rotation;
                escalaOriginalGuia = transformGuia.localScale;
                guiaActivoOriginalmente = transformGuia.gameObject.activeSelf;
                posicionOriginalGuiaGuardada = true;
            }

            transformGuia.SetPositionAndRotation(
                puntoGuiaEsperando.position + Vector3.up * alturaExtraGuia,
                puntoGuiaEsperando.rotation);
            transformGuia.localScale = escalaOriginalGuia * multiplicadorTamanoGuia;
            transformGuia.gameObject.SetActive(true);
        }

        private void RestaurarGuia()
        {
            if (!posicionOriginalGuiaGuardada || transformGuia == null) return;

            transformGuia.SetPositionAndRotation(posicionOriginalGuia, rotacionOriginalGuia);
            transformGuia.localScale = escalaOriginalGuia;
            transformGuia.gameObject.SetActive(guiaActivoOriginalmente);
            posicionOriginalGuiaGuardada = false;
        }

        private void OcultarUIFasesMinijuego()
        {
            if (panelFase1Corre != null) panelFase1Corre.SetActive(false);
            if (panelFase2Decision != null) panelFase2Decision.SetActive(false);
            if (botonCorre != null) botonCorre.gameObject.SetActive(false);
            if (botonOpcion2A != null) botonOpcion2A.gameObject.SetActive(false);
            if (botonOpcion2B != null) botonOpcion2B.gameObject.SetActive(false);
            if (barraTiempoFase1 != null) barraTiempoFase1.gameObject.SetActive(false);
            if (barraTiempoFase2 != null) barraTiempoFase2.gameObject.SetActive(false);
            if (imagenBarraFase1 != null) imagenBarraFase1.gameObject.SetActive(false);
            if (imagenBarraFase2 != null) imagenBarraFase2.gameObject.SetActive(false);
        }

        private MapaSeleccionCapitulo2 BuscarMapaSeleccion()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<MapaSeleccionCapitulo2>(FindObjectsInactive.Include);
#else
            return FindObjectOfType<MapaSeleccionCapitulo2>(true);
#endif
        }

        // Simula la barra que se encoge hacia el centro <-----> <-->
        private void ActualizarBarra(RectTransform barraRect, Image barraImg, float progreso)
        {
            if (barraRect != null)
            {
                barraRect.gameObject.SetActive(true);
                barraRect.localScale = new Vector3(progreso, 1f, 1f);
            }
            if (barraImg != null)
            {
                barraImg.gameObject.SetActive(true);
                if (barraImg.type == Image.Type.Filled)
                {
                    barraImg.fillAmount = progreso;
                }
            }
        }

        private IEnumerator SecuenciaFallo()
        {
            // Ocultar botón Corre y opciones
            if (botonCorre != null) botonCorre.gameObject.SetActive(false);
            if (panelFase2Decision != null) panelFase2Decision.SetActive(false);

            // 1. Mostrar pantalla roja de fallo en el fondo
            if (panelFalloRojo != null)
            {
                panelFalloRojo.gameObject.SetActive(true);
                panelFalloRojo.alpha = 1f;
                panelFalloRojo.blocksRaycasts = true;
                panelFalloRojo.transform.SetAsLastSibling();
            }

            // 2. Mostrar botón Volver a intentar POR ENCIMA de la pantalla roja
            if (botonReintentar != null)
            {
                if (panelFase1Corre != null)
                {
                    panelFase1Corre.SetActive(true);
                    panelFase1Corre.transform.SetAsLastSibling(); // Trae el contenedor al frente, por encima del rojo
                }
                if (botonReintentar.transform.parent != null)
                {
                    botonReintentar.transform.parent.gameObject.SetActive(true);
                    botonReintentar.transform.parent.SetAsLastSibling();
                }
                botonReintentar.gameObject.SetActive(true);
                botonReintentar.transform.SetAsLastSibling();

                if (botonReintentar.transform.localScale.x <= 0.05f)
                    botonReintentar.transform.localScale = Vector3.one;

                CanvasGroup cgRetry = botonReintentar.GetComponent<CanvasGroup>();
                if (cgRetry != null)
                {
                    cgRetry.alpha = 1f;
                    cgRetry.interactable = true;
                    cgRetry.blocksRaycasts = true;
                }
            }

            enJuego = false;
            yield break;
        }

        private IEnumerator MoverJugadorHacia(Transform destino, float duracion)
        {
            if (jugadorActivo == null || destino == null) yield break;

            Vector3 origen = jugadorActivo.transform.position;
            Quaternion rotOrigen = jugadorActivo.transform.rotation;

            Vector3 dir = (destino.position - origen);
            dir.y = 0f;
            Quaternion rotDestino = dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir) : rotOrigen;

            Transform camT = ObtenerTransformCamara();

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                float suave = Mathf.SmoothStep(0f, 1f, p);

                jugadorActivo.transform.position = Vector3.Lerp(origen, destino.position, suave);
                jugadorActivo.transform.rotation = Quaternion.Slerp(rotOrigen, rotDestino, suave);

                if (seguirJugadorLateral && camT != null)
                {
                    camT.position = jugadorActivo.transform.position + offsetCamaraLateral;
                    camT.rotation = rotacionCamaraLateral;
                }

                yield return null;
            }

            jugadorActivo.transform.position = destino.position;
            jugadorActivo.transform.rotation = destino.rotation;

            if (seguirJugadorLateral && camT != null)
            {
                camT.position = destino.position + offsetCamaraLateral;
                camT.rotation = rotacionCamaraLateral;
            }
        }

        private IEnumerator MoverCamara(Transform destino, float duracion)
        {
            Transform cTransform = ObtenerTransformCamara();
            if (cTransform == null || destino == null) yield break;

            Vector3 posOrigen = cTransform.position;
            Quaternion rotOrigen = cTransform.rotation;

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                float suave = Mathf.SmoothStep(0f, 1f, p);

                cTransform.position = Vector3.Lerp(posOrigen, destino.position, suave);
                cTransform.rotation = Quaternion.Slerp(rotOrigen, destino.rotation, suave);
                yield return null;
            }

            cTransform.position = destino.position;
            cTransform.rotation = destino.rotation;
        }

        private IEnumerator MoverCarroRecta(Transform carro, Transform inicio, Transform fin, float duracion)
        {
            if (carro == null) yield break;

            if (inicio != null)
            {
                carro.position = inicio.position;
                carro.rotation = inicio.rotation;
            }

            Vector3 posOrigen = carro.position;
            Vector3 posFin = fin != null ? fin.position : (posOrigen + carro.forward * 25f);

            carro.gameObject.SetActive(true);

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                carro.position = Vector3.Lerp(posOrigen, posFin, p);
                yield return null;
            }

            carro.position = posFin;
        }

        private void ResetearPosicionesCarros()
        {
            for (int i = 0; i < carros.Length; i++)
            {
                if (carros[i] != null && i < puntosInicioCarro.Length && puntosInicioCarro[i] != null)
                {
                    carros[i].position = puntosInicioCarro[i].position;
                    carros[i].rotation = puntosInicioCarro[i].rotation;
                }
            }
        }

        private void ConfigurarCamara(Transform punto)
        {
            Transform cTransform = ObtenerTransformCamara();

            if (camaraMinijuego != null)
            {
                camaraMinijuego.SetActive(true);
                Camera cComp = camaraMinijuego.GetComponent<Camera>();
                if (cComp == null) cComp = camaraMinijuego.GetComponentInChildren<Camera>();
                if (cComp != null)
                {
                    cComp.enabled = true;
                    cComp.depth = 25; // Prioridad superior a la cámara de la cinemática (que tiene depth 20)
                }
            }

            if (cTransform != null && punto != null)
            {
                cTransform.position = punto.position;
                cTransform.rotation = punto.rotation;
            }

            if (cTransform != null && jugadorActivo != null)
            {
                offsetCamaraLateral = cTransform.position - jugadorActivo.transform.position;
                rotacionCamaraLateral = cTransform.rotation;
            }
        }

        private Transform ObtenerTransformCamara()
        {
            if (camaraMinijuego != null) return camaraMinijuego.transform;
            if (transformCamara != null) return transformCamara;
            if (Camera.main != null) return Camera.main.transform;
            return null;
        }

        private IEnumerator MostrarDialogo(string texto)
        {
            ActivarPanelDialogo(true);

            textoCompletoActual = texto;
            textoTerminado = false;
            esperandoContinuar = true;

            if (textoDialogo != null) textoDialogo.text = "";

            foreach (char letra in texto)
            {
                if (textoTerminado) break;
                if (textoDialogo != null) textoDialogo.text += letra;
                yield return new WaitForSeconds(velocidadTypewriter);
            }

            if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
            textoTerminado = true;

            while (esperandoContinuar)
            {
                yield return null;
            }
        }

        public void OnClickContinuarDialogo()
        {
            if (Time.time - tiempoUltimoClick < 0.12f) return;
            tiempoUltimoClick = Time.time;

            if (!textoTerminado)
            {
                textoTerminado = true;
                if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
                return;
            }

            if (esperandoContinuar)
            {
                esperandoContinuar = false;
            }
        }

        private void ActivarPanelDialogo(bool activar)
        {
            if (panelDialogo == null)
            {
#if UNITY_2023_1_OR_NEWER
                Canvas[] todosCanvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
#endif
                foreach (Canvas cv in todosCanvas)
                {
                    Transform[] todosHijos = cv.GetComponentsInChildren<Transform>(true);
                    foreach (Transform h in todosHijos)
                    {
                        string n = h.name.ToLower();
                        if (n.Contains("dialogo") && (n.Contains("panel") || h.GetComponent<Image>() != null || h.GetComponent<CanvasGroup>() != null))
                        {
                            panelDialogo = h.gameObject;
                            break;
                        }
                    }
                    if (panelDialogo != null) break;
                }
            }

            if (panelDialogo != null)
            {
                if (textoDialogo == null) textoDialogo = panelDialogo.GetComponentInChildren<TMP_Text>(true);
                if (botonContinuarDialogo == null)
                {
                    botonContinuarDialogo = panelDialogo.GetComponentInChildren<Button>(true);
                    if (botonContinuarDialogo != null)
                    {
                        botonContinuarDialogo.onClick.RemoveListener(OnClickContinuarDialogo);
                        botonContinuarDialogo.onClick.AddListener(OnClickContinuarDialogo);
                    }
                }

                panelDialogo.SetActive(activar);
                if (activar)
                {
                    panelDialogo.transform.SetAsLastSibling();

                    if (panelDialogo.transform.localScale.x <= 0.05f)
                        panelDialogo.transform.localScale = Vector3.one;

                    CanvasGroup cg = panelDialogo.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 1f;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    }

                    if (botonContinuarDialogo != null)
                    {
                        botonContinuarDialogo.gameObject.SetActive(true);
                    }
                }
            }
        }

        private IEnumerator PedirRecuerdo()
        {
            if (panelRecuerdo == null)
            {
                Debug.LogWarning("No hay panel de recuerdo configurado para este minijuego.", this);
                yield break;
            }

            panelRecuerdo.SetActive(true);
            panelRecuerdo.transform.SetAsLastSibling();

            if (grupoFadeRecuerdo != null)
            {
                grupoFadeRecuerdo.alpha = 0f;
                grupoFadeRecuerdo.interactable = true;
                grupoFadeRecuerdo.blocksRaycasts = true;
            }

            if (campoRecuerdo == null || botonGuardarRecuerdo == null)
            {
                Debug.LogWarning("El panel de recuerdo está asignado, pero falta configurar el campo de texto o el botón Guardar.", this);
                yield break;
            }

            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");
            if (tituloRecuerdo != null)
            {
                tituloRecuerdo.text = "Recuerdos de " + nombreJugador;
            }

            if (grupoFadeRecuerdo != null)
            {
                float t = 0f;
                while (t < duracionFadeRecuerdo)
                {
                    t += Time.deltaTime;
                    grupoFadeRecuerdo.alpha = Mathf.Clamp01(t / duracionFadeRecuerdo);
                    yield return null;
                }
                grupoFadeRecuerdo.alpha = 1f;
            }

            if (panelRecuerdoPaso1 != null) panelRecuerdoPaso1.SetActive(true);
            if (panelRecuerdoPaso2 != null) panelRecuerdoPaso2.SetActive(false);

            if (botonSiguienteVerde != null) botonSiguienteVerde.SetActive(false);
            if (botonGuardarVerde != null) botonGuardarVerde.SetActive(false);
            if (botonSiguienteRecuerdo != null) botonSiguienteRecuerdo.gameObject.SetActive(true);
            botonGuardarRecuerdo.gameObject.SetActive(true);

            if (textoInstruccionRecuerdo != null)
            {
                textoInstruccionRecuerdo.gameObject.SetActive(true);
                textoInstruccionRecuerdo.text = "";
                string textoIntro = "Escribe una experiencia similar en la que sentías que, a pesar del dolor, algo dentro de ti seguía buscando comprenderlo, cuidarte o sostenerte.\n\nNo es obligatorio, pero puede ayudarte a personalizar tu experiencia.";
                yield return StartCoroutine(TypewriterTextoRecuerdo(textoInstruccionRecuerdo, textoIntro));
            }

            bool siguientePulsado = false;
            if (botonSiguienteRecuerdo != null)
            {
                botonSiguienteRecuerdo.onClick.RemoveAllListeners();
                botonSiguienteRecuerdo.onClick.AddListener(() =>
                {
                    siguientePulsado = true;
                    if (botonSiguienteVerde != null)
                    {
                        botonSiguienteVerde.SetActive(true);
                        botonSiguienteVerde.transform.SetAsLastSibling();
                        StartCoroutine(EfectoPopBoton(botonSiguienteVerde.transform));
                    }
                    else
                    {
                        StartCoroutine(EfectoPopBoton(botonSiguienteRecuerdo.transform));
                    }

                    if (panelRecuerdoPaso1 != null) panelRecuerdoPaso1.SetActive(false);
                    if (panelRecuerdoPaso2 != null) panelRecuerdoPaso2.SetActive(true);
                    if (textoInstruccionRecuerdo != null)
                        textoInstruccionRecuerdo.gameObject.SetActive(false);

                    if (campoRecuerdo != null)
                    {
                        campoRecuerdo.text = "";
                        ActualizarPreviewRecuerdo("");
                        campoRecuerdo.Select();
                        campoRecuerdo.ActivateInputField();
                    }
                });
            }

            if (botonSiguienteRecuerdo != null)
            {
                while (!siguientePulsado) yield return null;
            }
            else
            {
                if (panelRecuerdoPaso1 != null) panelRecuerdoPaso1.SetActive(false);
                if (panelRecuerdoPaso2 != null) panelRecuerdoPaso2.SetActive(true);
                if (textoInstruccionRecuerdo != null)
                    textoInstruccionRecuerdo.gameObject.SetActive(false);
            }

            if (campoRecuerdo != null)
            {
                campoRecuerdo.text = "";
                ActualizarPreviewRecuerdo("");
                campoRecuerdo.Select();
                campoRecuerdo.ActivateInputField();
            }

            bool guardado = false;
            botonGuardarRecuerdo.onClick.RemoveAllListeners();
            botonGuardarRecuerdo.onClick.AddListener(() =>
            {
                guardado = true;
                if (botonGuardarVerde != null)
                {
                    botonGuardarVerde.SetActive(true);
                    botonGuardarVerde.transform.SetAsLastSibling();
                    StartCoroutine(EfectoPopBoton(botonGuardarVerde.transform));
                }
                else
                {
                    StartCoroutine(EfectoPopBoton(botonGuardarRecuerdo.transform));
                }
            });

            while (!guardado) yield return null;

            string texto = campoRecuerdo.text.Trim();
            if (string.IsNullOrEmpty(texto))
            {
                PlayerPrefs.DeleteKey(claveRecuerdo);
            }
            else
            {
                PlayerPrefs.SetString(claveRecuerdo, texto);
            }
            PlayerPrefs.Save();

            campoRecuerdo.DeactivateInputField();

            if (grupoFadeRecuerdo != null)
            {
                float t = 0f;
                while (t < duracionFadeRecuerdo)
                {
                    t += Time.deltaTime;
                    grupoFadeRecuerdo.alpha = Mathf.Clamp01(1f - (t / duracionFadeRecuerdo));
                    yield return null;
                }
                grupoFadeRecuerdo.alpha = 0f;
                grupoFadeRecuerdo.interactable = false;
                grupoFadeRecuerdo.blocksRaycasts = false;
            }

            panelRecuerdo.SetActive(false);
        }

        private IEnumerator TypewriterTextoRecuerdo(TMP_Text textoUI, string texto)
        {
            if (textoUI == null) yield break;

            textoUI.text = "";
            foreach (char letra in texto)
            {
                textoUI.text += letra;
                yield return new WaitForSeconds(velocidadTypewriterRecuerdo);
            }
        }

        private void ActualizarPreviewRecuerdo(string nuevaTexto)
        {
            if (textoPreviewRecuerdo == null) return;

            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");
            string texto = string.IsNullOrEmpty(nuevaTexto) ? "" : nuevaTexto.Trim();

            if (string.IsNullOrEmpty(texto))
            {
                textoPreviewRecuerdo.text = nombreJugador + " recuerda: \"\"";
            }
            else
            {
                textoPreviewRecuerdo.text = nombreJugador + " recuerda: \"" + texto + "\"";
            }
        }

        /// <summary>
        /// Efecto visual POP: el botón escala de 1 → 1.25 → 1 en 0.15s para dar feedback inmediato.
        /// </summary>
        private IEnumerator EfectoPopBoton(Transform botonTransform)
        {
            if (botonTransform == null) yield break;

            Vector3 escalaOriginal = botonTransform.localScale;
            Vector3 escalaMax = escalaOriginal * 1.25f;
            float duracion = 0.08f;

            // Escala UP
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                botonTransform.localScale = Vector3.Lerp(escalaOriginal, escalaMax, t / duracion);
                yield return null;
            }

            // Escala DOWN
            t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                botonTransform.localScale = Vector3.Lerp(escalaMax, escalaOriginal, t / duracion);
                yield return null;
            }

            botonTransform.localScale = escalaOriginal;
        }
    }
}
