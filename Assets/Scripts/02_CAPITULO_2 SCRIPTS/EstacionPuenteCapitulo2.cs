using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Capitulo2
{
    [AddComponentMenu("Capitulo 2/Estacion Puente de Cristal")]
    public class EstacionPuenteCapitulo2 : MonoBehaviour
    {
        [System.Serializable]
        public class TarjetaSituacion
        {
            [Tooltip("Identificador o nombre de la situación")]
            public string titulo = "Tarjeta 1";
            [TextArea(2, 4)]
            [Tooltip("Texto descriptivo de la situación del paciente")]
            public string textoSituacion;
            [Tooltip("TRUE = Daño Confirmado (🔴) | FALSE = Protección Interpretativa (🔵)")]
            public bool esDanoConfirmado = true;
            [TextArea(2, 4)]
            [Tooltip("Mensaje del Guía si el jugador acierta")]
            public string retroAcierto;
            [TextArea(2, 4)]
            [Tooltip("Mensaje del Guía si el jugador falla")]
            public string retroFallo;
            [Tooltip("(Opcional) Si tienes una tarjeta GameObject dedicada para esta situación")]
            public GameObject objetoTarjetaPropia;
        }

        [Header("--- Teletransporte y Entorno ---")]
        [Tooltip("Transform del Guía en la escena")]
        [SerializeField] private Transform transformGuia;
        [Tooltip("Punto de destino del Guía en la plataforma del Puente")]
        [SerializeField] private Transform puntoDestinoGuia;

        [Space(6)]
        [Tooltip("(Recomendado) Cámara dedicada para esta estación. Se enciende al entrar y se apaga al salir")]
        [SerializeField] private GameObject camaraEstacion;
        [Tooltip("(Opcional) Transform de la cámara principal")]
        [SerializeField] private Transform transformCamara;
        [Tooltip("(Opcional) Destino para mover la cámara principal")]
        [SerializeField] private Transform puntoDestinoCamara;

        [Header("--- UI de Diálogo del Guía ---")]
        [SerializeField] private GameObject panelDialogo;
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuar;
        [SerializeField] private float velocidadTypewriter = 0.055f;
        [SerializeField] private Animator guiaAnimator;
        [SerializeField] private string parametroHablando = "Hablando";

        [Header("--- Conceptos: Daño y Protección (Imágenes Ilustrativas) ---")]
        [Tooltip("CanvasGroup o panel de la imagen de Daño Confirmado (ej. fractura/corte)")]
        [SerializeField] private CanvasGroup imagenDanoConfirmado;
        [Tooltip("CanvasGroup o panel de la imagen de Protección Interpretativa (ej. estrés/miedo/insomnio)")]
        [SerializeField] private CanvasGroup imagenProteccionInterpretativa;
        [SerializeField] private float duracionFadeImagen = 0.65f;

        [Header("--- Bucle de Confirmación (Paso 5) ---")]
        [Tooltip("Panel que contiene los dos botones de confirmación de conceptos")]
        [SerializeField] private GameObject panelConfirmacionConceptos;
        [Tooltip("Texto dentro del panel donde se repite la pregunta")]
        [SerializeField] private TMP_Text textoPreguntaConfirmacion;
        [Tooltip("Texto de la pregunta repetida en el panel")]
        [TextArea(1, 3)]
        [SerializeField] private string textoPreguntaConfirmacionStr = "¿Entendiste los conceptos DAÑO CONFIRMADO y PROTECCIÓN INTERPRETATIVA?";
        [Tooltip("Botón normal: 'Explicame de nuevo'")]
        [SerializeField] private Button botonExplicarDeNuevo;
        [Tooltip("Botón o check verde que hace POP al seleccionar 'Explicame de nuevo'")]
        [SerializeField] private GameObject botonVerdeExplicarDeNuevo;
        [Tooltip("Botón normal: 'Lo entendí'")]
        [SerializeField] private Button botonLoEntendi;
        [Tooltip("Botón o check verde que hace POP al seleccionar 'Lo entendí'")]
        [SerializeField] private GameObject botonVerdeLoEntendi;
        [Tooltip("Botón 'RESPONDER' que aparece suavemente para confirmar la selección")]
        [SerializeField] private Button botonResponderConfirmacion;

        [Header("--- Minijuego de Tarjetas (Paso 7) ---")]
        [Tooltip("Panel 2D que cubre la escena para clasificar las tarjetas")]
        [SerializeField] private GameObject panelMinijuegoCards;
        [Tooltip("Caja o zona de caída para DAÑO CONFIRMADO 🔴")]
        [SerializeField] private Button botonCajaDano;
        [Tooltip("Caja o zona de caída para PROTECCIÓN INTERPRETATIVA 🔵")]
        [SerializeField] private Button botonCajaProteccion;

        public RectTransform RectCajaDano => botonCajaDano != null ? botonCajaDano.GetComponent<RectTransform>() : null;
        public RectTransform RectCajaProteccion => botonCajaProteccion != null ? botonCajaProteccion.GetComponent<RectTransform>() : null;

        [Space(4)]
        [Tooltip("(Opcional) Si usas una sola tarjeta compartida, este es su contenedor")]
        [SerializeField] private GameObject tarjetaCompartidaUI;
        [Tooltip("(Opcional) Texto dentro de la tarjeta compartida")]
        [SerializeField] private TMP_Text textoTarjetaCompartida;

        [Space(4)]
        [Tooltip("Efecto de destello verde al acertar")]
        [SerializeField] private CanvasGroup flashVerde;
        [Tooltip("Efecto de destello rojo al equivocarse")]
        [SerializeField] private CanvasGroup flashRojo;
        [SerializeField] private float duracionFlash = 0.45f;

        [Space(6)]
        [Tooltip("Las 3 situaciones a clasificar")]
        [SerializeField]
        private TarjetaSituacion[] tarjetas = new TarjetaSituacion[]
        {
            new TarjetaSituacion
            {
                titulo = "Situación 1 (Vidrio)",
                textoSituacion = "Me corté con un vidrio y la herida está sangrando.",
                esDanoConfirmado = true,
                retroAcierto = "¡Correcto! Hay una herida visible que necesita atención física.",
                retroFallo = "¡Inténtalo de nuevo! Al haber un corte visible, es daño confirmado."
            },
            new TarjetaSituacion
            {
                titulo = "Situación 2 (Columna)",
                textoSituacion = "Me dijeron que tengo desgaste en la columna. Ahora tengo miedo de moverme y siento más dolor cuando pienso que puedo empeorar.",
                esDanoConfirmado = false,
                retroAcierto = "¡Exacto! El miedo enciende la alarma, pero no hay daño activo.",
                retroFallo = "¡Piénsalo bien! El temor aumenta la alarma sin que haya daño real."
            },
            new TarjetaSituacion
            {
                titulo = "Situación 3 (Sueño)",
                textoSituacion = "Dormí muy mal y hoy siento más dolor de espalda, aunque no me he lastimado.",
                esDanoConfirmado = false,
                retroAcierto = "¡Muy bien! Dormir mal sensibiliza el sistema, sin lesión alguna.",
                retroFallo = "¡Ojo! El mal descanso sube la alarma sin que exista una lesión."
            }
        };

        [Header("--- Pregunta Final de Comprensión (Paso 8: 4A / 4B) ---")]
        [Tooltip("Panel de la pregunta ¿Qué crees que es realmente el dolor?")]
        [SerializeField] private GameObject panelPreguntaFinal;
        [Tooltip("Opción 4A normal: El dolor siempre significa daño")]
        [SerializeField] private Button botonOpcion4A;
        [Tooltip("Botón o check verde que hace POP al seleccionar Opción 4A")]
        [SerializeField] private GameObject botonVerdeOpcion4A;
        [Tooltip("Opción 4B normal: Es una experiencia de protección que puede cambiar")]
        [SerializeField] private Button botonOpcion4B;
        [Tooltip("Botón o check verde que hace POP al seleccionar Opción 4B")]
        [SerializeField] private GameObject botonVerdeOpcion4B;
        [Tooltip("Botón 'RESPONDER' que aparece suavemente para confirmar la respuesta final")]
        [SerializeField] private Button botonResponderPreguntaFinal;

        [Header("--- Reflexión Opcional Escrita (Paso 9) ---")]
        [Tooltip("Panel del campo de texto libre para explicarle a alguien en casa")]
        [SerializeField] private GameObject panelReflexion;
        [Tooltip("Campo de texto donde el jugador puede escribir (hasta 300 caracteres)")]
        [SerializeField] private TMP_InputField inputReflexion;
        [Tooltip("Botón para guardar y continuar")]
        [SerializeField] private Button botonGuardarReflexion;
        [Tooltip("Botón verde con Pop que aparece al confirmar")]
        [SerializeField] private GameObject botonGuardarVerdeReflexion;

        [Header("--- Fade y Guardado ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        public CanvasGroup PanelFadeNegro => panelFadeNegro;
        [SerializeField] private float duracionFadeNegro = 0.7f;
        [SerializeField] private string claveGuardadoEstacion = "Cap2_Estacion4_Completada";
        [SerializeField] private string claveGuardadoReflexion = "Cap2_ReflexionEstacion4";
        public UnityEvent onEstacionCompletada;

        [Header("--- Debug ---")]
        [SerializeField] private bool iniciarAlEmpezar = false;

        // Variables de control interno
        private bool estacionIniciada = false;
        private bool esperandoContinuar = false;
        private bool textoTerminado = false;
        private string textoCompletoActual = "";
        private float tiempoUltimoClick = 0f;

        private int eleccionConfirmacion = 0; // 1 = Re-explicar, 2 = Entendido
        private int eleccionCajaMinijuego = 0; // 1 = Daño, 2 = Protección
        private int eleccionPreguntaFinal = 0; // 1 = 4A, 2 = 4B
        private bool reflexionGuardada = false;

        private int opcionSeleccionadaConfirmacion = 0; // 1 = Explicar de nuevo, 2 = Lo entendí
        private int opcionSeleccionadaFinal = 0; // 1 = 4A, 2 = 4B
        private Coroutine coroutinePopCheckConf;
        private Coroutine coroutinePopBotonConf;
        private Coroutine coroutinePopCheckFinal;
        private Coroutine coroutinePopBotonFinal;

        // Cámaras apagadas al entrar (para devolverlas al salir, igual que la Estación 3)
        private bool camaraExistenteMovida = false;
        private Vector3 posCamaraOriginal;
        private Quaternion rotCamaraOriginal;

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

        private void Awake()
        {
            ConectarConMapa();

            // Ocultar interfaces de entrada para que no estorben en la escena
            if (panelDialogo != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelDialogo);
                if (cg != null) cg.alpha = 0f;
                panelDialogo.SetActive(false);
            }
            if (panelConfirmacionConceptos != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelConfirmacionConceptos);
                if (cg != null) cg.alpha = 0f;
                panelConfirmacionConceptos.SetActive(false);
            }
            if (panelMinijuegoCards != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelMinijuegoCards);
                if (cg != null) cg.alpha = 0f;
                panelMinijuegoCards.SetActive(false);
            }
            if (panelPreguntaFinal != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelPreguntaFinal);
                if (cg != null) cg.alpha = 0f;
                panelPreguntaFinal.SetActive(false);
            }
            if (panelReflexion != null)
            {
                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelReflexion);
                if (cg != null) cg.alpha = 0f;
                panelReflexion.SetActive(false);
            }
            if (botonGuardarVerdeReflexion != null) botonGuardarVerdeReflexion.SetActive(false);

            if (botonVerdeExplicarDeNuevo != null) botonVerdeExplicarDeNuevo.SetActive(false);
            if (botonVerdeLoEntendi != null) botonVerdeLoEntendi.SetActive(false);
            if (botonResponderConfirmacion != null) botonResponderConfirmacion.gameObject.SetActive(false);

            if (botonVerdeOpcion4A != null) botonVerdeOpcion4A.SetActive(false);
            if (botonVerdeOpcion4B != null) botonVerdeOpcion4B.SetActive(false);
            if (botonResponderPreguntaFinal != null) botonResponderPreguntaFinal.gameObject.SetActive(false);

            if (imagenDanoConfirmado != null)
            {
                imagenDanoConfirmado.alpha = 0f;
                imagenDanoConfirmado.gameObject.SetActive(false);
            }
            if (imagenProteccionInterpretativa != null)
            {
                imagenProteccionInterpretativa.alpha = 0f;
                imagenProteccionInterpretativa.gameObject.SetActive(false);
            }
            if (flashVerde != null)
            {
                flashVerde.alpha = 0f;
                flashVerde.gameObject.SetActive(false);
            }
            if (flashRojo != null)
            {
                flashRojo.alpha = 0f;
                flashRojo.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            ConectarConMapa();
        }

        private void ConectarConMapa()
        {
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapa != null)
            {
                mapa.onZonaConfirmada.RemoveListener(OnZonaConfirmadaDesdeMapa);
                mapa.onZonaConfirmada.AddListener(OnZonaConfirmadaDesdeMapa);
            }
        }

        private void Start()
        {
            ConectarConMapa();
            ActualizarTextosCortosSiAplica();
            AutoDetectarReferenciasSiFaltan();

            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveListener(OnClickContinuar);
                botonContinuar.onClick.AddListener(OnClickContinuar);
            }

            if (botonExplicarDeNuevo != null)
            {
                botonExplicarDeNuevo.onClick.RemoveAllListeners();
                botonExplicarDeNuevo.onClick.AddListener(SeleccionarOpcionExplicarDeNuevo);
            }
            if (botonLoEntendi != null)
            {
                botonLoEntendi.onClick.RemoveAllListeners();
                botonLoEntendi.onClick.AddListener(SeleccionarOpcionLoEntendi);
            }
            if (botonResponderConfirmacion != null)
            {
                botonResponderConfirmacion.onClick.RemoveListener(ConfirmarRespuestaBucle);
                botonResponderConfirmacion.onClick.AddListener(ConfirmarRespuestaBucle);
            }

            Button btnVerdeExpl = botonVerdeExplicarDeNuevo != null ? botonVerdeExplicarDeNuevo.GetComponent<Button>() : null;
            if (btnVerdeExpl != null)
            {
                btnVerdeExpl.onClick.RemoveAllListeners();
                btnVerdeExpl.onClick.AddListener(SeleccionarOpcionExplicarDeNuevo);
            }
            Button btnVerdeEnt = botonVerdeLoEntendi != null ? botonVerdeLoEntendi.GetComponent<Button>() : null;
            if (btnVerdeEnt != null)
            {
                btnVerdeEnt.onClick.RemoveAllListeners();
                btnVerdeEnt.onClick.AddListener(SeleccionarOpcionLoEntendi);
            }

            // Las cajas no se seleccionan con click directo, sino arrastrando las tarjetas hacia ellas
            if (botonCajaDano != null) botonCajaDano.onClick.RemoveAllListeners();
            if (botonCajaProteccion != null) botonCajaProteccion.onClick.RemoveAllListeners();

            if (botonOpcion4A != null)
            {
                botonOpcion4A.onClick.RemoveAllListeners();
                botonOpcion4A.onClick.AddListener(SeleccionarOpcion4A);
            }
            if (botonOpcion4B != null)
            {
                botonOpcion4B.onClick.RemoveAllListeners();
                botonOpcion4B.onClick.AddListener(SeleccionarOpcion4B);
            }
            if (botonResponderPreguntaFinal != null)
            {
                botonResponderPreguntaFinal.onClick.RemoveListener(ConfirmarRespuestaFinal);
                botonResponderPreguntaFinal.onClick.AddListener(ConfirmarRespuestaFinal);
            }

            Button btnVerde4A = botonVerdeOpcion4A != null ? botonVerdeOpcion4A.GetComponent<Button>() : null;
            if (btnVerde4A != null)
            {
                btnVerde4A.onClick.RemoveAllListeners();
                btnVerde4A.onClick.AddListener(SeleccionarOpcion4A);
            }
            Button btnVerde4B = botonVerdeOpcion4B != null ? botonVerdeOpcion4B.GetComponent<Button>() : null;
            if (btnVerde4B != null)
            {
                btnVerde4B.onClick.RemoveAllListeners();
                btnVerde4B.onClick.AddListener(SeleccionarOpcion4B);
            }

            if (botonGuardarReflexion != null)
            {
                botonGuardarReflexion.onClick.RemoveAllListeners();
                botonGuardarReflexion.onClick.AddListener(OnConfirmarReflexion);
            }

            if (iniciarAlEmpezar)
            {
                IniciarEstacion();
            }
        }

        private void AutoDetectarReferenciasSiFaltan()
        {
            // 1. Detectar botones responder si faltan en Inspector
            if (botonResponderConfirmacion == null && panelConfirmacionConceptos != null)
            {
                Button[] btns = panelConfirmacionConceptos.GetComponentsInChildren<Button>(true);
                foreach (Button b in btns)
                {
                    string n = b.name.ToLower();
                    if ((n.Contains("responder") || n.Contains("confirmar") || n.Contains("aceptar")) && b != botonExplicarDeNuevo && b != botonLoEntendi)
                    {
                        botonResponderConfirmacion = b;
                        break;
                    }
                }
            }

            if (botonResponderPreguntaFinal == null && panelPreguntaFinal != null)
            {
                Button[] btns = panelPreguntaFinal.GetComponentsInChildren<Button>(true);
                foreach (Button b in btns)
                {
                    string n = b.name.ToLower();
                    if ((n.Contains("responder") || n.Contains("confirmar") || n.Contains("aceptar")) && b != botonOpcion4A && b != botonOpcion4B)
                    {
                        botonResponderPreguntaFinal = b;
                        break;
                    }
                }
            }

            // 2. Detectar botones verdes si faltan en Inspector
            if (panelConfirmacionConceptos != null)
            {
                if (botonVerdeExplicarDeNuevo == null)
                {
                    Transform t = panelConfirmacionConceptos.transform.Find("Boton Verde Explicar");
                    if (t == null) t = panelConfirmacionConceptos.transform.Find("Check Verde Explicar");
                    if (t == null) t = panelConfirmacionConceptos.transform.Find("Verde Explicar");
                    if (t != null) botonVerdeExplicarDeNuevo = t.gameObject;
                }
                if (botonVerdeLoEntendi == null)
                {
                    Transform t = panelConfirmacionConceptos.transform.Find("Boton Verde Lo Entendi");
                    if (t == null) t = panelConfirmacionConceptos.transform.Find("Check Verde Lo Entendi");
                    if (t == null) t = panelConfirmacionConceptos.transform.Find("Verde Lo Entendi");
                    if (t != null) botonVerdeLoEntendi = t.gameObject;
                }
            }

            if (panelPreguntaFinal != null)
            {
                if (botonVerdeOpcion4A == null)
                {
                    Transform t = panelPreguntaFinal.transform.Find("Boton Verde 4A");
                    if (t == null) t = panelPreguntaFinal.transform.Find("Check Verde 4A");
                    if (t == null) t = panelPreguntaFinal.transform.Find("Verde 4A");
                    if (t != null) botonVerdeOpcion4A = t.gameObject;
                }
                if (botonVerdeOpcion4B == null)
                {
                    Transform t = panelPreguntaFinal.transform.Find("Boton Verde 4B");
                    if (t == null) t = panelPreguntaFinal.transform.Find("Check Verde 4B");
                    if (t == null) t = panelPreguntaFinal.transform.Find("Verde 4B");
                    if (t != null) botonVerdeOpcion4B = t.gameObject;
                }
            }

            // 3. Detectar tarjetas en panelMinijuegoCards
            if (panelMinijuegoCards == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas cv in todosCanvas)
                {
                    Transform t = cv.transform.Find("Panel Minijuego Cards");
                    if (t == null) t = cv.transform.Find("Minijuego Cards");
                    if (t == null) t = cv.transform.Find("Panel Cards");
                    if (t == null) t = cv.transform.Find("Cards");
                    if (t != null)
                    {
                        panelMinijuegoCards = t.gameObject;
                        break;
                    }
                }
            }

            if (panelMinijuegoCards != null)
            {
                TarjetaDraggableUI[] drags = panelMinijuegoCards.GetComponentsInChildren<TarjetaDraggableUI>(true);
                for (int i = 0; i < drags.Length && i < tarjetas.Length; i++)
                {
                    if (tarjetas[i].objetoTarjetaPropia == null)
                    {
                        tarjetas[i].objetoTarjetaPropia = drags[i].gameObject;
                    }
                }

                for (int i = 0; i < tarjetas.Length; i++)
                {
                    if (tarjetas[i].objetoTarjetaPropia == null)
                    {
                        int num = i + 1;
                        Transform tHijo = panelMinijuegoCards.transform.Find("Tarjeta " + num);
                        if (tHijo == null) tHijo = panelMinijuegoCards.transform.Find("Tarjeta" + num);
                        if (tHijo == null) tHijo = panelMinijuegoCards.transform.Find("Card " + num);
                        if (tHijo == null) tHijo = panelMinijuegoCards.transform.Find("Card" + num);
                        if (tHijo == null) tHijo = panelMinijuegoCards.transform.Find("Situacion " + num);
                        if (tHijo == null) tHijo = panelMinijuegoCards.transform.Find("Situacion" + num);
                        if (tHijo != null) tarjetas[i].objetoTarjetaPropia = tHijo.gameObject;
                    }
                }

                if (tarjetaCompartidaUI == null && (tarjetas.Length == 0 || tarjetas[0].objetoTarjetaPropia == null))
                {
                    Transform tCompartida = panelMinijuegoCards.transform.Find("Tarjeta");
                    if (tCompartida == null) tCompartida = panelMinijuegoCards.transform.Find("Card");
                    if (tCompartida == null) tCompartida = panelMinijuegoCards.transform.Find("Tarjeta Compartida");
                    if (tCompartida != null) tarjetaCompartidaUI = tCompartida.gameObject;
                }
            }

            // 4. Detectar destellos/flashes si faltan en Inspector
            if (flashVerde == null || flashRojo == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas cv in todosCanvas)
                {
                    if (flashVerde == null)
                    {
                        Transform fv = cv.transform.Find("Flash Verde");
                        if (fv == null) fv = cv.transform.Find("FlashVerde");
                        if (fv != null) flashVerde = fv.GetComponent<CanvasGroup>();
                    }
                    if (flashRojo == null)
                    {
                        Transform fr = cv.transform.Find("Flash Rojo");
                        if (fr == null) fr = cv.transform.Find("FlashRojo");
                        if (fr == null) fr = cv.transform.Find("Flash Negro");
                        if (fr != null) flashRojo = fr.GetComponent<CanvasGroup>();
                    }
                }
            }

            // 5. Detectar cámara de la estación 4 si falta en Inspector
            if (camaraEstacion == null)
            {
                if (DialogosCapitulo2.Instance != null && DialogosCapitulo2.Instance.CamaraEstacion4 != null)
                {
                    camaraEstacion = DialogosCapitulo2.Instance.CamaraEstacion4;
                }
                else
                {
                    DialogosCapitulo2 dComp = FindObjectOfType<DialogosCapitulo2>(true);
                    if (dComp != null && dComp.CamaraEstacion4 != null)
                    {
                        camaraEstacion = dComp.CamaraEstacion4;
                    }
                }

                if (camaraEstacion == null)
                {
                    Camera[] todas = Resources.FindObjectsOfTypeAll<Camera>();
                    foreach (Camera c in todas)
                    {
                        if (!c.gameObject.scene.isLoaded) continue;
                        string n = c.gameObject.name.ToLower();
                        if (n.Contains("estacion 4") || n.Contains("estacion4") || n.Contains("puente") || n.Contains("camara 4") || n.Contains("camara4") || n.Contains("camera 4"))
                        {
                            camaraEstacion = c.gameObject;
                            break;
                        }
                    }
                }

                if (camaraEstacion == null)
                {
                    GameObject[] todosGO = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject go in todosGO)
                    {
                        if (!go.scene.isLoaded) continue;
                        string n = go.name.ToLower();
                        if (n.Contains("camara") || n.Contains("camera"))
                        {
                            if (n.Contains("estacion 4") || n.Contains("estacion4") || n.Contains("puente") || n.Contains("camara 4") || n.Contains("camara4") || n.Contains("camera 4"))
                            {
                                camaraEstacion = go;
                                break;
                            }
                        }
                    }
                }
            }

            // 6. Auto-detectar panel de diálogo, texto y botón continuar si faltan
            if (panelDialogo == null || textoDialogo == null || botonContinuar == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas cv in todosCanvas)
                {
                    if (panelDialogo == null)
                    {
                        Transform tDia = cv.transform.Find("Panel Dialogo");
                        if (tDia == null) tDia = cv.transform.Find("PanelDialogo");
                        if (tDia == null) tDia = cv.transform.Find("Dialogo");
                        if (tDia == null) tDia = cv.transform.Find("Panel_Dialogo");
                        if (tDia != null) panelDialogo = tDia.gameObject;
                    }

                    if (textoDialogo == null && panelDialogo != null)
                    {
                        textoDialogo = panelDialogo.GetComponentInChildren<TMP_Text>(true);
                    }

                    if (botonContinuar == null && panelDialogo != null)
                    {
                        Button[] btns = panelDialogo.GetComponentsInChildren<Button>(true);
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
            }

            // 7. Auto-detectar Guía si falta
            if (transformGuia == null)
            {
                if (guiaAnimator != null)
                {
                    transformGuia = guiaAnimator.transform;
                }
                else
                {
                    GameObject g = GameObject.Find("Guia") ?? GameObject.Find("Avatar_Guia") ?? GameObject.Find("Clinico");
                    if (g != null)
                    {
                        transformGuia = g.transform;
                        guiaAnimator = g.GetComponentInChildren<Animator>(true);
                    }
                }
            }
        }

        private void ActualizarTextosCortosSiAplica()
        {
            if (tarjetas == null) return;
            for (int i = 0; i < tarjetas.Length; i++)
            {
                if (i == 0)
                {
                    if (string.IsNullOrEmpty(tarjetas[i].retroAcierto) || tarjetas[i].retroAcierto.Length > 80)
                        tarjetas[i].retroAcierto = "¡Correcto! Hay una herida visible que necesita atención física.";
                    if (string.IsNullOrEmpty(tarjetas[i].retroFallo) || tarjetas[i].retroFallo.Length > 80)
                        tarjetas[i].retroFallo = "¡Inténtalo de nuevo! Al haber un corte visible, es daño confirmado.";
                }
                else if (i == 1)
                {
                    if (string.IsNullOrEmpty(tarjetas[i].retroAcierto) || tarjetas[i].retroAcierto.Length > 80)
                        tarjetas[i].retroAcierto = "¡Exacto! El miedo enciende la alarma, pero no hay daño activo.";
                    if (string.IsNullOrEmpty(tarjetas[i].retroFallo) || tarjetas[i].retroFallo.Length > 80)
                        tarjetas[i].retroFallo = "¡Piénsalo bien! El temor aumenta la alarma sin que haya daño real.";
                }
                else if (i == 2)
                {
                    if (string.IsNullOrEmpty(tarjetas[i].retroAcierto) || tarjetas[i].retroAcierto.Length > 80)
                        tarjetas[i].retroAcierto = "¡Muy bien! Dormir mal sensibiliza el sistema, sin lesión alguna.";
                    if (string.IsNullOrEmpty(tarjetas[i].retroFallo) || tarjetas[i].retroFallo.Length > 80)
                        tarjetas[i].retroFallo = "¡Ojo! El mal descanso sube la alarma sin que exista una lesión.";
                }
            }
        }

        private void MostrarBotonResponderConSuavidad(Button boton, ref Coroutine coroutinePop)
        {
            if (boton == null) return;
            ActivarPadresHastaCanvas(boton.gameObject);
            if (!boton.gameObject.activeSelf)
            {
                if (coroutinePop != null) StopCoroutine(coroutinePop);
                coroutinePop = StartCoroutine(AnimacionPopSuave(boton.gameObject, 1.15f, 0.22f));
            }
        }

        public void SeleccionarOpcionExplicarDeNuevo()
        {
            opcionSeleccionadaConfirmacion = 1;

            if (botonVerdeLoEntendi != null) botonVerdeLoEntendi.SetActive(false);

            if (botonVerdeExplicarDeNuevo != null)
            {
                if (coroutinePopCheckConf != null) StopCoroutine(coroutinePopCheckConf);
                coroutinePopCheckConf = StartCoroutine(AnimacionPopSuave(botonVerdeExplicarDeNuevo));
            }

            if (botonResponderConfirmacion != null)
            {
                MostrarBotonResponderConSuavidad(botonResponderConfirmacion, ref coroutinePopBotonConf);
            }
            else
            {
                eleccionConfirmacion = 1;
            }
        }

        public void SeleccionarOpcionLoEntendi()
        {
            opcionSeleccionadaConfirmacion = 2;

            if (botonVerdeExplicarDeNuevo != null) botonVerdeExplicarDeNuevo.SetActive(false);

            if (botonVerdeLoEntendi != null)
            {
                if (coroutinePopCheckConf != null) StopCoroutine(coroutinePopCheckConf);
                coroutinePopCheckConf = StartCoroutine(AnimacionPopSuave(botonVerdeLoEntendi));
            }

            if (botonResponderConfirmacion != null)
            {
                MostrarBotonResponderConSuavidad(botonResponderConfirmacion, ref coroutinePopBotonConf);
            }
            else
            {
                eleccionConfirmacion = 2;
            }
        }

        public void ConfirmarRespuestaBucle()
        {
            if (opcionSeleccionadaConfirmacion == 0) return;
            eleccionConfirmacion = opcionSeleccionadaConfirmacion;
        }

        public void SeleccionarOpcion4A()
        {
            opcionSeleccionadaFinal = 1;

            if (botonVerdeOpcion4B != null) botonVerdeOpcion4B.SetActive(false);

            if (botonVerdeOpcion4A != null)
            {
                if (coroutinePopCheckFinal != null) StopCoroutine(coroutinePopCheckFinal);
                coroutinePopCheckFinal = StartCoroutine(AnimacionPopSuave(botonVerdeOpcion4A));
            }

            if (botonResponderPreguntaFinal != null)
            {
                MostrarBotonResponderConSuavidad(botonResponderPreguntaFinal, ref coroutinePopBotonFinal);
            }
            else
            {
                eleccionPreguntaFinal = 1;
            }
        }

        public void SeleccionarOpcion4B()
        {
            opcionSeleccionadaFinal = 2;

            if (botonVerdeOpcion4A != null) botonVerdeOpcion4A.SetActive(false);

            if (botonVerdeOpcion4B != null)
            {
                if (coroutinePopCheckFinal != null) StopCoroutine(coroutinePopCheckFinal);
                coroutinePopCheckFinal = StartCoroutine(AnimacionPopSuave(botonVerdeOpcion4B));
            }

            if (botonResponderPreguntaFinal != null)
            {
                MostrarBotonResponderConSuavidad(botonResponderPreguntaFinal, ref coroutinePopBotonFinal);
            }
            else
            {
                eleccionPreguntaFinal = 2;
            }
        }

        public void ConfirmarRespuestaFinal()
        {
            if (opcionSeleccionadaFinal == 0) return;
            eleccionPreguntaFinal = opcionSeleccionadaFinal;
        }

        /// <summary>
        /// Escucha al evento OnZonaConfirmada del mapa. Si es la zona 4, arranca la estación.
        /// </summary>
        public void OnZonaConfirmadaDesdeMapa(int zona)
        {
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            int seleccionReal = (mapa != null && mapa.ZonaSeleccionada > 0) ? mapa.ZonaSeleccionada : PlayerPrefs.GetInt("Cap2_EstacionSeleccionada", zona);

            if (seleccionReal == 4)
            {
                IniciarEstacion();
            }
        }

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
            // PASO 1: Ocultar mapa y preparar fade
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapa != null) mapa.OcultarMapa();

            Canvas[] todosLosCanvas = FindObjectsOfType<Canvas>(true);
            foreach (Canvas c in todosLosCanvas)
            {
                Transform tMapa1 = c.transform.Find("Panel Mapa");
                if (tMapa1 != null) tMapa1.gameObject.SetActive(false);
                Transform tMapa2 = c.transform.Find("Mapa");
                if (tMapa2 != null) tMapa2.gameObject.SetActive(false);
            }

            if (panelDialogo != null) panelDialogo.SetActive(false);

            if (panelFadeNegro == null && mapa != null)
            {
                panelFadeNegro = mapa.GetComponentInChildren<CanvasGroup>(true);
            }
            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                panelFadeNegro.blocksRaycasts = true;
                panelFadeNegro.alpha = 1f;
            }

            // PASO 2: Teletransportar Guía, cámara y avatar del jugador
            ConfigurarEntornoInicial();

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

            // Bienvenida directa y rápida
            yield return StartCoroutine(MostrarDialogo("¡" + nombreJugador + ", ¡Te presento el Puente de Cristal! Revisemos dos conceptos clave."));

            // BUCLE DE EXPLICACIÓN (Pasos 3, 4 y 5)
            bool conceptosEntendidos = false;
            while (!conceptosEntendidos)
            {
                // PASO 3: Explicación DAÑO CONFIRMADO
                // Mostrar imagen ilustrativa mientras el Guía explica
                if (imagenDanoConfirmado != null)
                {
                    imagenDanoConfirmado.blocksRaycasts = false;
                    StartCoroutine(FadeCanvasGroup(imagenDanoConfirmado, 1f, duracionFadeImagen));
                }

                yield return StartCoroutine(MostrarDialogo("El primero es DAÑO CONFIRMADO, cuando existe una lesión física visible o comprobable."));
                yield return StartCoroutine(MostrarDialogo("Por ejemplo, un corte que sangra o un hueso roto."));

                if (imagenDanoConfirmado != null)
                {
                    yield return StartCoroutine(FadeCanvasGroup(imagenDanoConfirmado, 0f, duracionFadeImagen));
                }

                yield return new WaitForSeconds(0.2f);

                // PASO 4: Explicación PROTECCIÓN INTERPRETATIVA
                // Mostrar imagen ilustrativa con fade suave mientras el Guía explica
                if (imagenProteccionInterpretativa != null)
                {
                    imagenProteccionInterpretativa.blocksRaycasts = false;
                    StartCoroutine(FadeCanvasGroup(imagenProteccionInterpretativa, 1f, duracionFadeImagen));
                }

                yield return StartCoroutine(MostrarDialogo("El segundo es PROTECCIÓN INTERPRETATIVA, cuando sentimos dolor, pero no hay ninguna lesión física nueva."));
                yield return StartCoroutine(MostrarDialogo("Tu cerebro enciende la alarma de protección al interpretar una señal de amenaza aunque no sea dañina del todo."));
                yield return StartCoroutine(MostrarDialogo("Por ejemplo, haber dormido mal suele ponernos de mal humor. Los nervios nos ponen en guardia."));

                if (imagenProteccionInterpretativa != null)
                {
                    yield return StartCoroutine(FadeCanvasGroup(imagenProteccionInterpretativa, 0f, duracionFadeImagen));
                }

                yield return new WaitForSeconds(0.2f);

                // PASO 5: Pregunta de confirmación
                yield return StartCoroutine(MostrarDialogo("¿Entendiste estos dos conceptos?"));

                // Ocultar diálogo para que no se atraviese con el panel de respuesta
                yield return StartCoroutine(OcultarPanelDialogoSuave());

                eleccionConfirmacion = 0;
                opcionSeleccionadaConfirmacion = 0;

                if (botonVerdeExplicarDeNuevo != null) botonVerdeExplicarDeNuevo.SetActive(false);
                if (botonVerdeLoEntendi != null) botonVerdeLoEntendi.SetActive(false);
                if (botonResponderConfirmacion != null) botonResponderConfirmacion.gameObject.SetActive(false);

                if (panelConfirmacionConceptos != null)
                {
                    // Asignar el texto de la pregunta en el panel de confirmación
                    if (textoPreguntaConfirmacion == null)
                    {
                        TMP_Text[] todosTextos = panelConfirmacionConceptos.GetComponentsInChildren<TMP_Text>(true);
                        foreach (TMP_Text t in todosTextos)
                        {
                            string s = t.text.ToLower();
                            if (!s.Contains("entend") && !s.Contains("explic") && !s.Contains("nuevo"))
                            {
                                textoPreguntaConfirmacion = t;
                                break;
                            }
                        }
                    }
                    if (textoPreguntaConfirmacion != null)
                    {
                        textoPreguntaConfirmacion.text = textoPreguntaConfirmacionStr;
                    }

                    yield return StartCoroutine(MostrarPanelSuave(panelConfirmacionConceptos));
                }

                while (eleccionConfirmacion == 0)
                {
                    yield return null;
                }

                if (panelConfirmacionConceptos != null)
                {
                    yield return StartCoroutine(OcultarPanelSuave(panelConfirmacionConceptos));
                }

                if (eleccionConfirmacion == 2)
                {
                    conceptosEntendidos = true;
                }
                else
                {
                    yield return StartCoroutine(MostrarDialogo("¡Claro que sí! Vamos a repasarlos con calma una vez más."));
                }
            }

            yield return new WaitForSeconds(0.3f);

            // PASO 6: Introducción directa al minijuego
            yield return StartCoroutine(MostrarDialogo("¡Hora de clasificar! Arrastra cada tarjeta a su caja: ¿Daño o Protección?"));

            yield return StartCoroutine(OcultarPanelDialogoSuave());
            yield return new WaitForSeconds(0.25f);

            AutoDetectarReferenciasSiFaltan();

            // Asegurar que inicialmente todas las tarjetas estén ocultas excepto la que toque en su turno
            for (int k = 0; k < tarjetas.Length; k++)
            {
                if (tarjetas[k].objetoTarjetaPropia != null)
                {
                    tarjetas[k].objetoTarjetaPropia.SetActive(false);
                }
            }
            if (tarjetaCompartidaUI != null) tarjetaCompartidaUI.SetActive(false);

            // PASO 7: Minijuego de las 3 tarjetas
            if (panelMinijuegoCards != null)
            {
                ActivarPadresHastaCanvas(panelMinijuegoCards);
                panelMinijuegoCards.SetActive(true);
                panelMinijuegoCards.transform.SetAsLastSibling();
            }

            CanvasGroup cgMinijuego = ObtenerOAgregarCanvasGroup(panelMinijuegoCards);
            if (cgMinijuego != null)
            {
                cgMinijuego.alpha = 0f;
                cgMinijuego.blocksRaycasts = true;
                cgMinijuego.interactable = true;
            }

            // Iniciar suave latido en las cajitas de Daño y Protección para invitar a la interacción
            Coroutine coroutineLatidoCajas = StartCoroutine(AnimarLatidoCajas());

            for (int i = 0; i < tarjetas.Length; i++)
            {
                yield return StartCoroutine(ProcesarTarjetaMinijuego(tarjetas[i], i + 1));
            }

            if (coroutineLatidoCajas != null) StopCoroutine(coroutineLatidoCajas);
            ResetearEscalaCajas();

            if (panelMinijuegoCards != null)
            {
                yield return StartCoroutine(OcultarPanelSuave(panelMinijuegoCards));
            }

            yield return new WaitForSeconds(0.35f);

            // PASO 8: Pregunta de opción múltiple (4A / 4B)
            yield return StartCoroutine(MostrarDialogo("¿Qué crees que es realmente el dolor?"));

            yield return StartCoroutine(OcultarPanelDialogoSuave());

            eleccionPreguntaFinal = 0;
            opcionSeleccionadaFinal = 0;

            if (botonVerdeOpcion4A != null) botonVerdeOpcion4A.SetActive(false);
            if (botonVerdeOpcion4B != null) botonVerdeOpcion4B.SetActive(false);
            if (botonResponderPreguntaFinal != null) botonResponderPreguntaFinal.gameObject.SetActive(false);

            if (panelPreguntaFinal != null)
            {
                yield return StartCoroutine(MostrarPanelSuave(panelPreguntaFinal));
            }

            while (eleccionPreguntaFinal == 0)
            {
                yield return null;
            }

            if (panelPreguntaFinal != null)
            {
                yield return StartCoroutine(OcultarPanelSuave(panelPreguntaFinal));
            }

            if (eleccionPreguntaFinal == 1)
            {
                yield return StartCoroutine(MostrarDialogo("¡Buen intento! El dolor es real, pero no siempre significa daño activo."));
                yield return StartCoroutine(MostrarDialogo("Es solo una parte de la historia."));
            }
            else
            {
                yield return StartCoroutine(MostrarDialogo("¡Exacto! Esa es la clave del capítulo."));
                yield return StartCoroutine(MostrarDialogo("El dolor depende de interpretación y contexto."));
            }

            yield return new WaitForSeconds(0.3f);

            // PASO 9: Campo de texto libre (Reflexión)
            yield return StartCoroutine(MostrarDialogo("Para cerrar, ¿cómo le explicarías el dolor con tus propias palabras a alguien cercano?"));
            yield return StartCoroutine(MostrarDialogo("No es obligatorio responder, pero te ayudará mucho. ¡Escribe lo que sientas en el recuadro!"));

            yield return StartCoroutine(OcultarPanelDialogoSuave());

            reflexionGuardada = false;
            if (panelReflexion != null)
            {
                if (botonGuardarVerdeReflexion != null) botonGuardarVerdeReflexion.SetActive(false);
                yield return StartCoroutine(MostrarPanelSuave(panelReflexion));

                while (!reflexionGuardada)
                {
                    yield return null;
                }

                yield return StartCoroutine(OcultarPanelSuave(panelReflexion));
            }

            yield return new WaitForSeconds(0.3f);

            // PASO 10: Cierre y felicitación
            yield return StartCoroutine(MostrarDialogo("¡Excelente trabajo, " + nombreJugador + "!"));
            yield return StartCoroutine(MostrarDialogo("Ya conoces el puente: el dolor es real, pero no siempre significa daño físico."));

            yield return new WaitForSeconds(0.8f);

            // Ocultar suavemente el diálogo para que no quede encima del mapa
            yield return StartCoroutine(OcultarPanelDialogoSuave());

            // Guardar progreso en PlayerPrefs
            PlayerPrefs.SetInt(claveGuardadoEstacion, 1);
            PlayerPrefs.Save();

            // Fade a negro para volver al mapa
            if (panelFadeNegro != null)
            {
                panelFadeNegro.gameObject.SetActive(true);
                panelFadeNegro.blocksRaycasts = true;
                float t = 0f;
                while (t < duracionFadeNegro * 1.5f)
                {
                    t += Time.deltaTime;
                    panelFadeNegro.alpha = Mathf.Clamp01(t / (duracionFadeNegro * 1.5f));
                    yield return null;
                }
                panelFadeNegro.alpha = 1f;
            }

            yield return new WaitForSeconds(0.3f);

            // Con la pantalla en negro: apagar toda la UI de la estación
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelConfirmacionConceptos != null) panelConfirmacionConceptos.SetActive(false);
            if (panelMinijuegoCards != null) panelMinijuegoCards.SetActive(false);
            if (panelPreguntaFinal != null) panelPreguntaFinal.SetActive(false);
            if (panelReflexion != null) panelReflexion.SetActive(false);
            if (imagenDanoConfirmado != null) imagenDanoConfirmado.gameObject.SetActive(false);
            if (imagenProteccionInterpretativa != null) imagenProteccionInterpretativa.gameObject.SetActive(false);
            if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, false);

            // Restaurar las cámaras originales (vuelve a la vista inicial)
            RestaurarCamaras();

            // Abrir el mapa actualizado
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

            // Solo desvanecer el fade negro en el mapa si NO se han completado todas las estaciones
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

            estacionIniciada = false;
            onEstacionCompletada?.Invoke();
        }

        // Procesa la interacción y feedback de una tarjeta
        private IEnumerator ProcesarTarjetaMinijuego(TarjetaSituacion tarjeta, int indice)
        {
            CanvasGroup cgMinijuego = ObtenerOAgregarCanvasGroup(panelMinijuegoCards);
            GameObject objTarjetaActual = tarjeta.objetoTarjetaPropia != null ? tarjeta.objetoTarjetaPropia : tarjetaCompartidaUI;

            // Failsafe: Si no está asignada en el inspector, buscar por tarjeta draggable en el panel
            if (objTarjetaActual == null && panelMinijuegoCards != null)
            {
                TarjetaDraggableUI[] drags = panelMinijuegoCards.GetComponentsInChildren<TarjetaDraggableUI>(true);
                if (drags != null && drags.Length >= indice)
                {
                    objTarjetaActual = drags[indice - 1].gameObject;
                }
                else if (drags != null && drags.Length > 0)
                {
                    objTarjetaActual = drags[0].gameObject;
                }
            }

            TarjetaDraggableUI dragActual = objTarjetaActual != null ? objTarjetaActual.GetComponent<TarjetaDraggableUI>() : null;

            bool acerto = false;

            while (!acerto)
            {
                // Resetear posición de la tarjeta al centro/inicio
                if (dragActual != null) dragActual.ResetearPosicionInmediata();

                // Activar únicamente la tarjeta que corresponde a esta ronda
                if (objTarjetaActual != null)
                {
                    ActivarPadresHastaCanvas(objTarjetaActual);
                    objTarjetaActual.SetActive(true);
                    objTarjetaActual.transform.SetAsLastSibling();

                    if (objTarjetaActual.transform.localScale == Vector3.zero || objTarjetaActual.transform.localScale.x <= 0.05f)
                    {
                        objTarjetaActual.transform.localScale = Vector3.one;
                    }

                    CanvasGroup cgTarjeta = objTarjetaActual.GetComponent<CanvasGroup>();
                    if (cgTarjeta != null)
                    {
                        cgTarjeta.alpha = 1f;
                        cgTarjeta.blocksRaycasts = true;
                        cgTarjeta.interactable = true;
                    }

                    TMP_Text txt = objTarjetaActual.GetComponentInChildren<TMP_Text>(true);
                    if (txt != null && !string.IsNullOrEmpty(tarjeta.textoSituacion))
                    {
                        txt.text = tarjeta.textoSituacion;
                    }

                    StartCoroutine(AnimacionPopSuave(objTarjetaActual));
                }

                // Asegurar que el panel completo del minijuego sea visible y con interactividad
                if (panelMinijuegoCards != null)
                {
                    ActivarPadresHastaCanvas(panelMinijuegoCards);
                    panelMinijuegoCards.SetActive(true);
                }

                // Vuelven a aparecer en fade suave las cards y las cajas para que el jugador pueda interactuar
                if (cgMinijuego != null)
                {
                    cgMinijuego.blocksRaycasts = true;
                    cgMinijuego.interactable = true;
                    yield return StartCoroutine(FadeCanvasGroup(cgMinijuego, 1f, 0.25f));
                }

                eleccionCajaMinijuego = 0;

                // Esperar a que el jugador arrastre la tarjeta a una de las dos cajas
                while (eleccionCajaMinijuego == 0)
                {
                    yield return null;
                }

                bool eligioDano = (eleccionCajaMinijuego == 1);
                acerto = (eligioDano == tarjeta.esDanoConfirmado);

                if (acerto)
                {
                    // Destello verde al acertar
                    CanvasGroup flashAUsar = (flashVerde != null) ? flashVerde : flashRojo;
                    if (flashAUsar != null) StartCoroutine(EjecutarFlash(flashAUsar, new Color(0.12f, 0.88f, 0.25f, 1f)));

                    // Desaparecen las cards y las cajas con fade suave
                    if (cgMinijuego != null)
                    {
                        yield return StartCoroutine(FadeCanvasGroup(cgMinijuego, 0f, 0.25f));
                    }

                    // Ocultar tarjeta para que no quede en pantalla en la siguiente ronda
                    if (objTarjetaActual != null) objTarjetaActual.SetActive(false);

                    // Aparece el diálogo de retroalimentación
                    yield return StartCoroutine(MostrarDialogo(tarjeta.retroAcierto));
                    yield return StartCoroutine(OcultarPanelDialogoSuave());
                }
                else
                {
                    // Destello rojo al fallar
                    CanvasGroup flashAUsar = (flashRojo != null) ? flashRojo : flashVerde;
                    if (flashAUsar != null) StartCoroutine(EjecutarFlash(flashAUsar, new Color(0.95f, 0.15f, 0.15f, 1f)));

                    // Regresar suavemente la tarjeta a su lugar inicial
                    if (dragActual != null) dragActual.RegresarSuaveAPosicionInicial(0.2f);

                    // Desaparecen las cards y las cajas con fade suave
                    if (cgMinijuego != null)
                    {
                        yield return StartCoroutine(FadeCanvasGroup(cgMinijuego, 0f, 0.25f));
                    }

                    // Aparece el diálogo de retroalimentación
                    yield return StartCoroutine(MostrarDialogo(tarjeta.retroFallo));
                    yield return StartCoroutine(OcultarPanelDialogoSuave());

                    // Al continuar el bucle, las cards y cajas vuelven a aparecer en fade suave para reintentar
                }
            }

            yield return new WaitForSeconds(0.2f);
        }

        private void OnConfirmarReflexion()
        {
            if (inputReflexion != null && !string.IsNullOrEmpty(inputReflexion.text))
            {
                PlayerPrefs.SetString(claveGuardadoReflexion, inputReflexion.text);
                PlayerPrefs.Save();
            }

            StartCoroutine(SecuenciaBotonGuardarPop());
        }

        private IEnumerator SecuenciaBotonGuardarPop()
        {
            if (botonGuardarVerdeReflexion != null)
            {
                yield return StartCoroutine(AnimacionPopSuave(botonGuardarVerdeReflexion));
                yield return new WaitForSeconds(0.25f);
            }
            reflexionGuardada = true;
        }

        // Permite clasificar una tarjeta mediante arrastre o llamada directa desde eventos
        public void SeleccionarCajaDano()
        {
            eleccionCajaMinijuego = 1;
        }

        public void SeleccionarCajaProteccion()
        {
            eleccionCajaMinijuego = 2;
        }

        // Configuración inicial de posiciones del guía y cámara
        private void ConfigurarEntornoInicial()
        {
            // Guía / Avatar: teletransportar manteniendo su tamaño original sin alterarlo
            if (transformGuia != null && puntoDestinoGuia != null)
            {
                if (guiaAnimator != null) guiaAnimator.applyRootMotion = false;
                transformGuia.position = puntoDestinoGuia.position;
                transformGuia.rotation = puntoDestinoGuia.rotation;
            }

            // Asegurar que la cámara de la Estación 4 esté ubicada
            if (camaraEstacion == null)
            {
                AutoDetectarReferenciasSiFaltan();
            }

            // Cámara
            camaraExistenteMovida = false;

            if (camaraEstacion != null)
            {
                if (puntoDestinoCamara != null)
                {
                    camaraEstacion.transform.SetPositionAndRotation(puntoDestinoCamara.position, puntoDestinoCamara.rotation);
                }
                Camera camComp = CamarasCapitulo2.Activar(camaraEstacion);
                if (camComp != null) camComp.depth = 35;
            }
            else if (transformCamara != null && puntoDestinoCamara != null)
            {
                posCamaraOriginal = transformCamara.position;
                rotCamaraOriginal = transformCamara.rotation;
                camaraExistenteMovida = true;

                transformCamara.position = puntoDestinoCamara.position;
                transformCamara.rotation = puntoDestinoCamara.rotation;
                CamarasCapitulo2.Activar(transformCamara.gameObject);
            }
            else
            {
                // Respaldo de seguridad: mover la cámara principal activa si existe punto destino
                Camera camPrincipal = Camera.main;
                if (camPrincipal != null && puntoDestinoCamara != null)
                {
                    posCamaraOriginal = camPrincipal.transform.position;
                    rotCamaraOriginal = camPrincipal.transform.rotation;
                    camaraExistenteMovida = true;
                    transformCamara = camPrincipal.transform;

                    camPrincipal.transform.position = puntoDestinoCamara.position;
                    camPrincipal.transform.rotation = puntoDestinoCamara.rotation;
                    CamarasCapitulo2.Activar(camPrincipal.gameObject);
                }
            }
        }

        // Termina la vista del puente antes de entregar la cámara al mapa.
        private void RestaurarCamaras()
        {
            if (camaraEstacion != null) camaraEstacion.SetActive(false);

            if (camaraExistenteMovida && transformCamara != null)
            {
                transformCamara.position = posCamaraOriginal;
                transformCamara.rotation = rotCamaraOriginal;
            }
            camaraExistenteMovida = false;
            // AbrirMapa selecciona la cámara de retorno; no se recuperan cámaras de estaciones anteriores.
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
        {
            if (cg == null) yield break;

            cg.gameObject.SetActive(true);
            float startAlpha = cg.alpha;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                yield return null;
            }

            cg.alpha = targetAlpha;
            if (targetAlpha <= 0.01f)
            {
                cg.gameObject.SetActive(false);
            }
        }

        private IEnumerator EjecutarFlash(CanvasGroup flashCG, Color col)
        {
            if (flashCG == null) yield break;

            Canvas canvas = flashCG.GetComponentInParent<Canvas>(true);
            Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
            if (rootCanvas != null)
            {
                flashCG.transform.SetParent(rootCanvas.transform, false);
                RectTransform rect = flashCG.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.localScale = Vector3.one;
                }
            }
            else
            {
                ActivarPadresHastaCanvas(flashCG.gameObject);
            }
            flashCG.gameObject.SetActive(true);
            flashCG.transform.SetAsLastSibling();
            flashCG.blocksRaycasts = false;

            Image img = flashCG.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = null;
                img.color = new Color(col.r, col.g, col.b, 1f);
                img.raycastTarget = false;
            }

            // Duración mejorada para que el destello verde o rojo se aprecie claramente (Fade In -> Sostener -> Fade Out)
            float tiempoIn = 0.18f;
            float tiempoSostener = 0.28f;
            float tiempoOut = 0.38f;

            // 1. Fade In
            float t = 0f;
            while (t < tiempoIn)
            {
                t += Time.deltaTime;
                flashCG.alpha = Mathf.Lerp(0f, 0.72f, t / tiempoIn);
                yield return null;
            }
            flashCG.alpha = 0.72f;

            // 2. Sostener destello
            yield return new WaitForSeconds(tiempoSostener);

            // 3. Fade Out
            t = 0f;
            while (t < tiempoOut)
            {
                t += Time.deltaTime;
                flashCG.alpha = Mathf.Lerp(0.72f, 0f, t / tiempoOut);
                yield return null;
            }

            flashCG.alpha = 0f;
            flashCG.gameObject.SetActive(false);
        }

        private IEnumerator AnimarLatidoCajas()
        {
            Vector3 escalaOriginalDano = (botonCajaDano != null && botonCajaDano.transform.localScale.x > 0.05f) 
                ? botonCajaDano.transform.localScale : Vector3.one;
            Vector3 escalaOriginalProt = (botonCajaProteccion != null && botonCajaProteccion.transform.localScale.x > 0.05f) 
                ? botonCajaProteccion.transform.localScale : Vector3.one;

            float tiempo = 0f;
            while (true)
            {
                tiempo += Time.deltaTime * 3.2f;
                float pulso = 1f + Mathf.Sin(tiempo) * 0.038f;

                if (botonCajaDano != null) botonCajaDano.transform.localScale = escalaOriginalDano * pulso;
                if (botonCajaProteccion != null) botonCajaProteccion.transform.localScale = escalaOriginalProt * pulso;

                yield return null;
            }
        }

        private void ResetearEscalaCajas()
        {
            if (botonCajaDano != null) botonCajaDano.transform.localScale = Vector3.one;
            if (botonCajaProteccion != null) botonCajaProteccion.transform.localScale = Vector3.one;
        }

        private CanvasGroup ObtenerOAgregarCanvasGroup(GameObject obj)
        {
            if (obj == null) return null;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = obj.AddComponent<CanvasGroup>();
            }
            return cg;
        }

        private IEnumerator MostrarPanelSuave(GameObject obj, float duracion = 0.25f)
        {
            if (obj == null) yield break;
            CanvasGroup cg = ObtenerOAgregarCanvasGroup(obj);
            obj.SetActive(true);
            obj.transform.SetAsLastSibling();

            if (cg != null)
            {
                cg.alpha = 0f;
                cg.blocksRaycasts = true;
                cg.interactable = true;

                float t = 0f;
                while (t < duracion)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Clamp01(t / duracion);
                    yield return null;
                }
                cg.alpha = 1f;
            }
        }

        private IEnumerator OcultarPanelSuave(GameObject obj, float duracion = 0.2f)
        {
            if (obj == null) yield break;
            CanvasGroup cg = ObtenerOAgregarCanvasGroup(obj);

            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;

                float t = 0f;
                while (t < duracion)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Clamp01(1f - (t / duracion));
                    yield return null;
                }
                cg.alpha = 0f;
            }

            obj.SetActive(false);
        }

        private IEnumerator OcultarPanelDialogoSuave(float duracion = 0.2f)
        {
            if (panelDialogo == null || !panelDialogo.activeSelf) yield break;
            CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelDialogo);

            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;

                float t = 0f;
                while (t < duracion)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Clamp01(1f - (t / duracion));
                    yield return null;
                }
                cg.alpha = 0f;
            }

            panelDialogo.SetActive(false);
        }

        private IEnumerator MostrarDialogo(string texto)
        {
            if (panelDialogo == null || textoDialogo == null || botonContinuar == null)
            {
                AutoDetectarReferenciasSiFaltan();
            }

            if (panelDialogo != null)
            {
                ActivarPadresHastaCanvas(panelDialogo);
                panelDialogo.SetActive(true);
                panelDialogo.transform.SetAsLastSibling();

                if (panelDialogo.transform.localScale == Vector3.zero || panelDialogo.transform.localScale.x <= 0.05f)
                {
                    panelDialogo.transform.localScale = Vector3.one;
                }

                CanvasGroup cg = ObtenerOAgregarCanvasGroup(panelDialogo);
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                }
            }

            if (botonContinuar != null)
            {
                ActivarPadresHastaCanvas(botonContinuar.gameObject);
                botonContinuar.gameObject.SetActive(true);
            }

            if (textoDialogo == null && panelDialogo != null)
            {
                textoDialogo = panelDialogo.GetComponentInChildren<TMP_Text>(true);
            }

            textoCompletoActual = texto;
            textoTerminado = false;
            esperandoContinuar = true;

            if (textoDialogo != null) textoDialogo.text = "";
            if (guiaAnimator != null) guiaAnimator.SetBool(parametroHablando, true);

            int i = 0;
            while (i < texto.Length)
            {
                if (textoTerminado) break;

                // Si encontramos una etiqueta como <color=...> o <b>, la insertamos completa para no tipear código visible
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
                yield return new WaitForSeconds(Mathf.Max(0.01f, velocidadTypewriter * 0.8f));
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
    }
}
