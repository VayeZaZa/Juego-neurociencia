using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

namespace Capitulo2
{
    public class DialogosCapitulo2 : MonoBehaviour
    {
        public static DialogosCapitulo2 Instance { get; private set; }

        [Header("UI Principal de Diálogos")]
        [SerializeField] private GameObject panelDialogo;
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuar;

        [Header("UI de Respuestas (Pregunta y Elecciones)")]
        [SerializeField] private GameObject panelRespuestas;
        [SerializeField] private Button botonOpcion1;       // Opción 1: "Sí, muchas veces."
        [SerializeField] private GameObject checkOpcion1;  // Check / icono activo de la opción 1
        [SerializeField] private Button botonOpcion2;       // Opción 2: "No estoy seguro/a."
        [SerializeField] private GameObject checkOpcion2;  // Check / icono activo de la opción 2
        [SerializeField] private Button botonResponder;     // Botón para confirmar respuesta

        [Header("Cámaras - Tour de las 4 Estaciones")]
        [SerializeField] private GameObject camaraGameplay;   // Cámara de juego normal (frente al Guía)
        [SerializeField] private GameObject camaraEstacion1;  // Cámara mostrando estación 1
        [SerializeField] private GameObject camaraEstacion2;  // Cámara mostrando estación 2
        [SerializeField] private GameObject camaraEstacion3;  // Cámara mostrando estación 3
        [SerializeField] private GameObject camaraEstacion4;  // Cámara mostrando estación 4
        public GameObject CamaraEstacion4 => camaraEstacion4;
        public GameObject CamaraGameplay => camaraGameplay;
        [SerializeField] private float velocidadPaneoLateral = 0.25f; // Movimiento suave de ladito

        [Header("Fade a Negro")]
        [SerializeField] private CanvasGroup panelFadeNegro;  // Imagen o panel negro con CanvasGroup
        [SerializeField] private float duracionFadeNegro = 0.7f;

        [Header("Personaje / Guía")]
        [SerializeField] private Animator avatarAnimator;

        [Header("Configuración Typewriter")]
        [SerializeField] private float velocidadEscritura = 0.055f;

        [Header("Eventos")]
        [Tooltip("Se dispara al terminar toda la introducción y presentación de las 4 estaciones")]
        public UnityEvent onFinIntroduccion;

        // Diálogos internos del Capítulo 2
        private string[] dialogosIntro;
        private int indiceActual = 0;
        private bool escribiendo = false;
        private string textoCompletoActual = "";
        private Coroutine coroutineActual;
        private Coroutine coroutinePop;
        private float tiempoUltimoAvance = 0f;
        private GameObject camaraEstacionActual = null;
        private bool transicionandoCamara = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Ocultar paneles inicialmente
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelRespuestas != null) panelRespuestas.SetActive(false);

            if (panelFadeNegro != null)
            {
                panelFadeNegro.alpha = 0f;
                panelFadeNegro.blocksRaycasts = false;
                panelFadeNegro.gameObject.SetActive(false);
            }

            // Auto-vincular botones limpiando listeners previos
            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveListener(SiguienteDialogo);
                botonContinuar.onClick.AddListener(SiguienteDialogo);
            }
            if (botonOpcion1 != null)
            {
                botonOpcion1.onClick.RemoveAllListeners();
                botonOpcion1.onClick.AddListener(() => SeleccionarOpcion(1));
            }
            if (botonOpcion2 != null)
            {
                botonOpcion2.onClick.RemoveAllListeners();
                botonOpcion2.onClick.AddListener(() => SeleccionarOpcion(2));
            }
            if (botonResponder != null)
            {
                botonResponder.onClick.RemoveListener(ConfirmarRespuesta);
                botonResponder.onClick.AddListener(ConfirmarRespuesta);
            }
        }

        private void Update()
        {
            // Movimiento suave de ladito mientras se muestra la cámara de una estación
            if (camaraEstacionActual != null && velocidadPaneoLateral != 0f)
            {
                camaraEstacionActual.transform.Translate(Vector3.right * velocidadPaneoLateral * Time.deltaTime, Space.Self);
            }
        }

        /// <summary>
        /// Inicia la secuencia completa de diálogos del Capítulo 2.
        /// Conéctalo al evento AlCompletarCinematica de CinematicaCapitulo2.
        /// </summary>
        public void IniciarDialogoIntroduccion()
        {
            tiempoUltimoAvance = Time.time;
            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");

            // Diálogos estructurados en orden: Bienvenida corta -> Pregunta directa -> 4 Estaciones -> Cierre
            dialogosIntro = new string[]
            {
                // FASE 1: Bienvenida corta y directa
                "¡Hola, " + nombreJugador + " 😊! ¡Qué alegría verte por aquí!",                                          // 0
                "Hoy descubriremos juntos un gran secreto sobre nuestro cerebro.",                                         // 1
                "¿Alguna vez sentiste un dolor muy fuerte sin entender bien por qué?",                                    // 2 -> PANEL RESPUESTAS

                // FASE 1: Continuación tras responder
                "¡Es completamente normal! A todos nos pasa.",                                                            // 3
                "Frente a ti hay 4 caminos con retos para poner a prueba tu mente.",                                      // 4

                // FASE 2: Presentación de las 4 estaciones con cámara
                "¡Aquí vas a dudar de tus propios ojos!",                                                                 // 5 -> CÁMARA ESTACIÓN 1
                "¡Aquí decidirás en un segundo qué es más urgente!",                                                      // 6 -> CÁMARA ESTACIÓN 2
                "¡Aquí conocerás las voces y emociones que negocian tus respuestas!",                                     // 7 -> CÁMARA ESTACIÓN 3
                "¡Y aquí descubrirás la diferencia entre daño y protección!",                                             // 8 -> CÁMARA ESTACIÓN 4

                // FASE 2: Cierre (Cámara vuelve a Gameplay frente al Guía)
                "¡Elige por cuál empezar en el mapa y diviértete, " + nombreJugador + "!"                                 // 9
            };

            indiceActual = 0;

            if (panelDialogo != null) panelDialogo.SetActive(true);
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);

            MostrarLineaActual();
        }

        private void MostrarLineaActual()
        {
            if (coroutineActual != null)
            {
                StopCoroutine(coroutineActual);
                coroutineActual = null;
            }

            // Solo la primera línea (índice 0) ejecuta el saludo Greeting
            bool esSaludoInicial = (indiceActual == 0);
            coroutineActual = StartCoroutine(EscribirTexto(dialogosIntro[indiceActual], esSaludoInicial));
        }

        public void SiguienteDialogo()
        {
            if (transicionandoCamara) return;
            if (Time.time - tiempoUltimoAvance < 0.15f) return;

            // Si está escribiendo el typewriter, al presionar continuar sale el texto completo de golpe
            if (escribiendo)
            {
                tiempoUltimoAvance = Time.time;
                escribiendo = false;
                if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
                if (avatarAnimator != null) avatarAnimator.SetBool("Hablando", false);
                return;
            }

            tiempoUltimoAvance = Time.time;

            // Si acabamos de ver la pregunta (índice 2), mostramos las respuestas
            if (indiceActual == 2)
            {
                MostrarPanelRespuestas();
                return;
            }

            // Transiciones a las 4 estaciones con fade suave y cambio de cámara
            if (indiceActual == 4)
            {
                StartCoroutine(CambiarEstacionConFade(camaraEstacion1, 5));
                return;
            }
            if (indiceActual == 5)
            {
                StartCoroutine(CambiarEstacionConFade(camaraEstacion2, 6));
                return;
            }
            if (indiceActual == 6)
            {
                StartCoroutine(CambiarEstacionConFade(camaraEstacion3, 7));
                return;
            }
            if (indiceActual == 7)
            {
                StartCoroutine(CambiarEstacionConFade(camaraEstacion4, 8));
                return;
            }
            if (indiceActual == 8)
            {
                // Regresar a la cámara de gameplay frente al Guía para el cierre
                StartCoroutine(CambiarEstacionConFade(camaraGameplay, 9));
                return;
            }

            indiceActual++;

            // Si aún quedan diálogos por mostrar
            if (indiceActual < dialogosIntro.Length)
            {
                MostrarLineaActual();
            }
            else
            {
                // Fin de toda la introducción
                FinalizarIntroduccion();
            }
        }

        private IEnumerator CambiarEstacionConFade(GameObject nuevaCamara, int siguienteIndice)
        {
            transicionandoCamara = true;

            // 1. Fade a negro
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

            // 2. Cambiar cámara
            ActivarCamara(nuevaCamara);

            // 3. Preparar siguiente diálogo
            indiceActual = siguienteIndice;

            yield return new WaitForSeconds(0.1f);

            // 4. Fade desde negro (reaparece suavemente la nueva vista)
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

            transicionandoCamara = false;

            // Mostrar el diálogo correspondiente a esta estación
            MostrarLineaActual();
        }

        private void ActivarCamara(GameObject nuevaCamara)
        {
            // Apagar las cámaras gestionadas
            if (camaraGameplay != null) camaraGameplay.SetActive(false);
            if (camaraEstacion1 != null) camaraEstacion1.SetActive(false);
            if (camaraEstacion2 != null) camaraEstacion2.SetActive(false);
            if (camaraEstacion3 != null) camaraEstacion3.SetActive(false);
            if (camaraEstacion4 != null) camaraEstacion4.SetActive(false);

            // Encender la nueva cámara
            if (nuevaCamara != null)
            {
                nuevaCamara.SetActive(true);

                // Solo las cámaras de estaciones hacen paneo lateral (la de gameplay queda fija)
                if (nuevaCamara != camaraGameplay)
                {
                    camaraEstacionActual = nuevaCamara;
                }
                else
                {
                    camaraEstacionActual = null;
                }
            }
            else
            {
                camaraEstacionActual = null;
            }
        }

        private void MostrarPanelRespuestas()
        {
            if (avatarAnimator != null) avatarAnimator.SetBool("Hablando", false);

            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelRespuestas != null) panelRespuestas.SetActive(true);

            if (checkOpcion1 != null)
            {
                checkOpcion1.SetActive(false);
                checkOpcion1.transform.localScale = Vector3.one;
            }
            if (checkOpcion2 != null)
            {
                checkOpcion2.SetActive(false);
                checkOpcion2.transform.localScale = Vector3.one;
            }

            if (botonResponder != null) botonResponder.gameObject.SetActive(false);
        }

        public void SeleccionarOpcion(int opcion)
        {
            if (coroutinePop != null)
            {
                StopCoroutine(coroutinePop);
                coroutinePop = null;
            }

            if (opcion == 1)
            {
                if (checkOpcion2 != null)
                {
                    checkOpcion2.SetActive(false);
                    checkOpcion2.transform.localScale = Vector3.one;
                }
                if (checkOpcion1 != null)
                {
                    coroutinePop = StartCoroutine(AnimacionPopCheck(checkOpcion1));
                }
            }
            else if (opcion == 2)
            {
                if (checkOpcion1 != null)
                {
                    checkOpcion1.SetActive(false);
                    checkOpcion1.transform.localScale = Vector3.one;
                }
                if (checkOpcion2 != null)
                {
                    coroutinePop = StartCoroutine(AnimacionPopCheck(checkOpcion2));
                }
            }

            if (botonResponder != null) botonResponder.gameObject.SetActive(true);
        }

        // Efecto POP suave y sutil para el check
        private IEnumerator AnimacionPopCheck(GameObject checkObj)
        {
            if (checkObj == null) yield break;

            checkObj.SetActive(true);
            Transform t = checkObj.transform;

            Vector3 escalaFinal = Vector3.one;
            t.localScale = Vector3.zero;

            float duracion = 0.22f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float progreso = tiempo / duracion;

                float escalaMultiplicador;
                if (progreso < 0.65f)
                {
                    escalaMultiplicador = Mathf.Lerp(0f, 1.15f, progreso / 0.65f);
                }
                else
                {
                    escalaMultiplicador = Mathf.Lerp(1.15f, 1.0f, (progreso - 0.65f) / 0.35f);
                }

                t.localScale = escalaFinal * escalaMultiplicador;
                yield return null;
            }

            t.localScale = escalaFinal;
        }

        public void ConfirmarRespuesta()
        {
            tiempoUltimoAvance = Time.time;

            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (panelDialogo != null) panelDialogo.SetActive(true);
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);

            // Continuar con el diálogo 3 ("¡Es completamente normal!...")
            indiceActual = 3;
            MostrarLineaActual();
        }

        private void FinalizarIntroduccion()
        {
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelRespuestas != null) panelRespuestas.SetActive(false);
            if (avatarAnimator != null) avatarAnimator.SetBool("Hablando", false);

            Debug.Log("DialogosCapitulo2: Introducción y presentación de las 4 estaciones completadas exitosamente.");
            onFinIntroduccion?.Invoke();

            // Garantizar que el mapa se despierte y se abra solo al terminar los diálogos iniciales
#if UNITY_2023_1_OR_NEWER
            MapaSeleccionCapitulo2 mapa = Object.FindFirstObjectByType<MapaSeleccionCapitulo2>(FindObjectsInactive.Include);
#else
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
#endif
            if (mapa != null)
            {
                mapa.gameObject.SetActive(true);
                mapa.AbrirMapa();
            }
        }

        private IEnumerator EscribirTexto(string texto, bool esSaludo)
        {
            escribiendo = true;
            textoCompletoActual = texto;

            if (avatarAnimator != null)
            {
                if (esSaludo)
                {
                    avatarAnimator.Play("Greeting");
                    yield return new WaitForSeconds(0.8f);
                    avatarAnimator.SetBool("Hablando", true);
                }
                else
                {
                    avatarAnimator.SetBool("Hablando", true);
                }
            }

            if (textoDialogo != null) textoDialogo.text = "";

            foreach (char letra in texto)
            {
                if (!escribiendo)
                {
                    if (textoDialogo != null) textoDialogo.text = textoCompletoActual;
                    if (avatarAnimator != null) avatarAnimator.SetBool("Hablando", false);
                    yield break;
                }

                if (textoDialogo != null) textoDialogo.text += letra;
                yield return new WaitForSeconds(Mathf.Max(0.01f, velocidadEscritura * 0.8f));
            }

            escribiendo = false;

            if (avatarAnimator != null)
                avatarAnimator.SetBool("Hablando", false);
        }
    }
}
