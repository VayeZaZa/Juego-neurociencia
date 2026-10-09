using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Capitulo2
{
    [AddComponentMenu("Capitulo 2/Estacion Junta Directiva")]
    public class EstacionJuntaCapitulo2 : MonoBehaviour
    {
        [System.Serializable]
        public class NodoEmocion
        {
            public string nombreNodo = "Sensibilidad";

            [Header("Modelo y botón")]
            [Tooltip("Modelo 3D de la emoción. Aparece solo cuando se carga la estación y levita suavemente")]
            public GameObject modelo3D;
            [Tooltip("Botón 'Activar...' de esta emoción (puede ser hijo del modelo)")]
            public Button botonNodo;
            [Tooltip("El mismo botón pero en verde, oculto al inicio. Al hacer click hace pop y queda visible")]
            public GameObject botonVerde;
            [Tooltip("CanvasGroup que envuelve el botón normal Y el verde juntos. Se usa para hacerlos desaparecer/aparecer con fade durante el close-up de OTRA emoción")]
            public CanvasGroup grupoBoton;

            [Header("Cámara close-up")]
            [Tooltip("Punto (Empty) frente a esta emoción hacia donde se acerca la cámara al seleccionarla")]
            public Transform puntoCamaraCloseUp;

            [Header("Diálogo de la emoción")]
            [Tooltip("Etiqueta de nombre YA HECHA (con su color/estilo propio). El script solo la activa/desactiva")]
            public GameObject etiquetaNombre;
            [Tooltip("Lo que dice esta emoción en el panel único de diálogo")]
            [TextArea(2, 4)]
            public string dialogoEmocion = "";

            [Header("Al terminar de leer")]
            [Tooltip("Luz que se enciende en esta emoción (una Light, o un objeto que contenga una Light / partículas)")]
            public GameObject efectoLuz;
            [Tooltip("Pulgar/check que aparece con pop SOLO cuando termina de leerse el diálogo")]
            public GameObject checkPulgar;

            [System.NonSerialized] public bool activado = false;
            [System.NonSerialized] public Vector3 posicionBase;
            [System.NonSerialized] public Vector3 escalaBase;
        }

        [Header("--- Cámara de la Estación ---")]
        [Tooltip("(Recomendado) Cámara dedicada para esta estación. Se enciende al entrar y se apaga al salir")]
        [SerializeField] private GameObject camaraEstacion;
        [Tooltip("(Opcional) Si no usas cámara dedicada, se mueve esta cámara (o la Main Camera) y se restaura al salir")]
        [SerializeField] private Transform transformCamara;
        [Tooltip("Punto (Empty) de la vista frontal general: se ve la sala de juntas con las 6 sillas")]
        [SerializeField] private Transform puntoCamaraGeneral;
        [SerializeField] private float duracionCloseUp = 1.0f;
        [SerializeField] private float duracionVolverGeneral = 0.9f;

        [Header("--- Las 6 Emociones ---")]
        [SerializeField]
        private NodoEmocion[] nodos = new NodoEmocion[]
        {
            new NodoEmocion { nombreNodo = "Sensibilidad",
                dialogoEmocion = "Soy Sensibilidad. Yo detecto la señal: qué tan fuerte es el estímulo. Pero ojo, solo traigo la información en bruto a la mesa. Yo no decido si eso duele." },
            new NodoEmocion { nombreNodo = "Atención",
                dialogoEmocion = "Soy Atención. Yo decido cuánto foco le das a la señal. Si estás distraído, un golpe casi ni se nota; si me concentro en tu cuerpo, ese mismo golpe se siente enorme." },
            new NodoEmocion { nombreNodo = "Miedo",
                dialogoEmocion = "Soy Miedo. Si creo que hay una amenaza real, le subo el volumen a todo. Hago que el dolor se sienta más grande de lo que el daño justificaría... pero es porque quiero protegerte." },
            new NodoEmocion { nombreNodo = "Memoria",
                dialogoEmocion = "Soy Memoria. Comparo lo que sientes ahora con lo que ya viviste. Si algo parecido terminó mal antes, doy la alarma más fuerte, aunque esta vez el daño sea menor." },
            new NodoEmocion { nombreNodo = "Motivación",
                dialogoEmocion = "Soy Motivación. Cuando hay algo más importante que hacer, como escapar de un peligro o terminar algo que te importa, puedo bajarle el volumen al dolor." },
            new NodoEmocion { nombreNodo = "Estrés",
                dialogoEmocion = "Soy Estrés. Cuando llevas mucho tiempo bajo presión, bajo el umbral de todas las demás voces. Entonces todo empieza a doler un poco más fácil." },
        };

        [Header("--- Levitación de los Modelos ---")]
        [SerializeField] private float velocidadLevitacion = 1.2f;
        [SerializeField] private float amplitudLevitacion = 0.15f;

        [Header("--- Efecto de Luz ---")]
        [SerializeField] private float duracionEncendidoLuz = 0.8f;

        [Header("--- Efecto Pop (botón verde y pulgar) ---")]
        [SerializeField] private float escalaMaxPop = 1.2f;
        [SerializeField] private float duracionPop = 0.22f;

        [Header("--- Fade de Botones y Modelos durante el Close-up ---")]
        [Tooltip("Cuánto tardan en desvanecerse los botones y encogerse los otros modelos al acercarse a una emoción, y en reaparecer al volver")]
        [SerializeField] private float duracionFadeUI = 0.3f;

        [Header("--- Contenedor General de UI ---")]
        [Tooltip("GameObject padre que agrupa TODO lo de esta estación (los 6 botones y el panel de diálogo). " +
                 "Se oculta apenas arranca el juego y solo se muestra mientras esta estación está activa, " +
                 "para que no aparezca encima de las otras estaciones en la misma escena.")]
        [SerializeField] private GameObject contenedorUIGeneral;

        [Header("--- UI de Diálogo (panel único para todos) ---")]
        [SerializeField] private GameObject panelDialogo;
        [Tooltip("Texto donde va el mensaje")]
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuar;
        [SerializeField] private float velocidadTypewriter = 0.03f;
        [Tooltip("Etiqueta de nombre YA HECHA para cuando habla el Guía (misma lógica que las de cada emoción)")]
        [SerializeField] private GameObject etiquetaGuia;

        [Header("--- Fade a Negro de Pantalla ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        [SerializeField] private float duracionFadeNegro = 0.5f;

        [Header("--- Guardado de Progreso y Eventos ---")]
        [SerializeField] private string claveGuardadoEstacion = "Cap2_Estacion3_Completada";
        public UnityEvent onEstacionCompletada;

        [Header("--- Panel de Sliders (Clasificación Final) ---")]
        [Tooltip("Panel PNG / UI que contiene los 3 sliders y el botón Aceptar")]
        [SerializeField] private GameObject panelSliders;
        [Tooltip("Slider 1: Motivación (izquierda, valor 0) ↔ Estrés (derecha, valor 100)")]
        [SerializeField] private Slider slider1MotivacionEstres;
        [Tooltip("Slider 2: Sensibilidad (izquierda, valor 0) ↔ Miedo (derecha, valor 100)")]
        [SerializeField] private Slider slider2SensibilidadMiedo;
        [Tooltip("Slider 3: Atención (izquierda, valor 0) ↔ Memoria (derecha, valor 100)")]
        [SerializeField] private Slider slider3AtencionMemoria;
        [Tooltip("Botón 'Aceptar' (normal/negro) dentro del panel de sliders")]
        [SerializeField] private Button botonAceptarSliders;
        [Tooltip("El mismo botón pero en verde, oculto al inicio. Al hacer click hace Pop como confirmación")]
        [SerializeField] private GameObject botonAceptarVerde;

        [Header("--- Handles de los Sliders (Icono del Avatar) ---")]
        [Tooltip("Componente Image del Handle del Slider 1 (el circulito o botón que se arrastra)")]
        [SerializeField] private Image handleImageSlider1;
        [Tooltip("Componente Image del Handle del Slider 2")]
        [SerializeField] private Image handleImageSlider2;
        [Tooltip("Componente Image del Handle del Slider 3")]
        [SerializeField] private Image handleImageSlider3;
        [Tooltip("Sprite del avatar Hombre para los handles (PlayerPrefs Avatar = 1)")]
        [SerializeField] private Sprite spriteHandleHombre;
        [Tooltip("Sprite del avatar Mujer para los handles (PlayerPrefs Avatar = 2)")]
        [SerializeField] private Sprite spriteHandleMujer;

        [Header("--- Luz de Ambiente (resultado de los sliders) ---")]
        [Tooltip("Light grande de la sala que cambia de color según la combinación dominante")]
        [SerializeField] private Light luzAmbiente;
        [Tooltip("Color cuando Miedo Y Memoria dominan (valor > 60 en sus sliders)")]
        [SerializeField] private Color colorMiedoMemoria = new Color(1f, 0.25f, 0.1f);    // Rojo/naranja
        [Tooltip("Color cuando Motivación Y Atención dominan (valor < 40 en sus sliders)")]
        [SerializeField] private Color colorMotivacionAtencion = new Color(0.2f, 1f, 0.45f); // Verde cálido
        [Tooltip("Color para cualquier otra combinación")]
        [SerializeField] private Color colorNeutral = new Color(1f, 0.92f, 0.25f);           // Amarillo
        [SerializeField] private float duracionCambioLuz = 0.8f;

        [Header("--- Debug / Pruebas ---")]
        [SerializeField] private bool iniciarAlEmpezar = false;

        // Variables internas
        private bool estacionIniciada = false;
        private bool esperandoContinuar = false;
        private bool textoTerminado = false;
        private string textoCompletoActual = "";
        private float tiempoUltimoClick = 0f;
        private bool procesandoNodo = false;
        private int nodosActivados = 0;
        private List<GameObject> todasLasEtiquetas = new List<GameObject>();

        private List<GameObject> camarasDesactivadas = new List<GameObject>();
        private bool camaraExistenteMovida = false;
        private Vector3 posCamaraOriginal;
        private Quaternion rotCamaraOriginal;

        private void Awake()
        {
            ConectarConMapa();

            // Ocultar TODO de entrada: como es una sola escena con varias estaciones,
            // sin esto la UI de la Junta se vería encima de las demás desde el inicio.
            if (contenedorUIGeneral != null) contenedorUIGeneral.SetActive(false);
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelSliders != null) panelSliders.SetActive(false);
            if (botonAceptarVerde != null) botonAceptarVerde.SetActive(false);

            // Armar la lista de TODAS las etiquetas de nombre (Guía + 6 emociones) para poder apagarlas todas de un tirón
            todasLasEtiquetas.Clear();
            if (etiquetaGuia != null) todasLasEtiquetas.Add(etiquetaGuia);

            for (int i = 0; i < nodos.Length; i++)
            {
                NodoEmocion nodo = nodos[i];
                nodo.activado = false;

                if (nodo.modelo3D != null)
                {
                    nodo.posicionBase = nodo.modelo3D.transform.localPosition;
                    nodo.escalaBase = nodo.modelo3D.transform.localScale;
                    nodo.modelo3D.SetActive(false);
                }
                if (nodo.botonVerde != null) nodo.botonVerde.SetActive(false);
                if (nodo.checkPulgar != null) nodo.checkPulgar.SetActive(false);
                if (nodo.efectoLuz != null) nodo.efectoLuz.SetActive(false);
                if (nodo.grupoBoton != null) nodo.grupoBoton.alpha = 1f;
                if (nodo.etiquetaNombre != null)
                {
                    nodo.etiquetaNombre.SetActive(false);
                    todasLasEtiquetas.Add(nodo.etiquetaNombre);
                }
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

            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveListener(OnClickContinuar);
                botonContinuar.onClick.AddListener(OnClickContinuar);
            }

            for (int i = 0; i < nodos.Length; i++)
            {
                int indice = i; // copia local para que cada botón use su propio índice
                if (nodos[i].botonNodo != null)
                {
                    nodos[i].botonNodo.onClick.RemoveAllListeners();
                    nodos[i].botonNodo.onClick.AddListener(() => SeleccionarNodo(indice));
                }
            }

            if (iniciarAlEmpezar)
            {
                IniciarEstacion();
            }
        }

        private void Update()
        {
            // Levitación suave de los modelos (cada uno con su propio desfase)
            if (!estacionIniciada) return;

            for (int i = 0; i < nodos.Length; i++)
            {
                GameObject modelo = nodos[i].modelo3D;
                if (modelo == null || !modelo.activeSelf) continue;

                Vector3 pos = nodos[i].posicionBase;
                pos.y += Mathf.Sin(Time.time * velocidadLevitacion + i * 0.9f) * amplitudLevitacion;
                modelo.transform.localPosition = pos;
            }
        }

        /// <summary>
        /// Se conecta solo al evento del mapa. Si la zona confirmada es 3, inicia la estación.
        /// </summary>
        public void OnZonaConfirmadaDesdeMapa(int zona)
        {
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            int seleccionReal = (mapa != null && mapa.ZonaSeleccionada > 0) ? mapa.ZonaSeleccionada : PlayerPrefs.GetInt("Cap2_EstacionSeleccionada", zona);

            if (seleccionReal == 3)
            {
                IniciarEstacion();
            }
        }

        public void IniciarEstacion()
        {
            if (estacionIniciada) return;
            estacionIniciada = true;

            nodosActivados = 0;
            procesandoNodo = false;
            for (int i = 0; i < nodos.Length; i++) nodos[i].activado = false;

            StopAllCoroutines();
            StartCoroutine(SecuenciaEstacionCompleta());
        }

        private IEnumerator SecuenciaEstacionCompleta()
        {
            // Ocultar mapa si estuviera abierto
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapa != null) mapa.OcultarMapa();

            if (panelDialogo != null) panelDialogo.SetActive(false);

            // Buscar panel de fade a negro si no está asignado
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

            // Con la pantalla en negro: cámara frontal a la sala y aparecen los 6 modelos
            PrepararCamara();
            ActivarEmociones();

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

            // INTRODUCCIÓN DEL GUÍA (los botones siguen bloqueados mientras habla)
            yield return StartCoroutine(MostrarDialogo("Estas voces no son enemigas. Son sistemas que negocian qué respuesta conviene."));
            yield return StartCoroutine(MostrarDialogo("¿Qué te han dicho sobre tu dolor que nunca olvidaste? Eso también se sienta en esta mesa."));
            yield return StartCoroutine(MostrarDialogo("Elige cada voz y déjala hablar. Conócelas antes de seguir."));

            if (panelDialogo != null) panelDialogo.SetActive(false);

            // Ahora sí: el jugador puede empezar a elegir emociones
            HabilitarBotones(true);

            // El flujo continúa en SeleccionarNodo() -> ProcesarNodo().
            // Cuando las 6 estén escuchadas, ProcesarNodo llama a SecuenciaCierre().
        }

        // Hace aparecer los 6 modelos y bloquea sus botones hasta que termine la intro
        private void ActivarEmociones()
        {
            if (contenedorUIGeneral != null) contenedorUIGeneral.SetActive(true);

            for (int i = 0; i < nodos.Length; i++)
            {
                if (nodos[i].modelo3D != null)
                {
                    nodos[i].modelo3D.transform.localPosition = nodos[i].posicionBase;
                    nodos[i].modelo3D.SetActive(true);
                }
            }
            HabilitarBotones(false);
        }

        private void OcultarEmociones()
        {
            for (int i = 0; i < nodos.Length; i++)
            {
                NodoEmocion n = nodos[i];

                if (n.modelo3D != null)
                {
                    n.modelo3D.SetActive(false);
                    n.modelo3D.transform.localScale = n.escalaBase; // dejarlo listo para la próxima vez
                }
                if (n.efectoLuz != null) n.efectoLuz.SetActive(false);
                if (n.checkPulgar != null) n.checkPulgar.SetActive(false);
                if (n.botonVerde != null) n.botonVerde.SetActive(false);
                if (n.grupoBoton != null) n.grupoBoton.alpha = 1f; // dejarlo listo para la próxima vez
            }

            if (contenedorUIGeneral != null) contenedorUIGeneral.SetActive(false);
        }

        // Los botones ya usados nunca se vuelven a habilitar
        private void HabilitarBotones(bool habilitar)
        {
            for (int i = 0; i < nodos.Length; i++)
            {
                if (nodos[i].botonNodo != null)
                {
                    nodos[i].botonNodo.interactable = habilitar && !nodos[i].activado;
                }
            }
        }

        // Apaga TODAS las etiquetas de nombre y prende solo la que corresponde
        private void ActivarEtiqueta(GameObject etiqueta)
        {
            for (int i = 0; i < todasLasEtiquetas.Count; i++)
            {
                if (todasLasEtiquetas[i] != null) todasLasEtiquetas[i].SetActive(false);
            }
            if (etiqueta != null) etiqueta.SetActive(true);
        }

        /// <summary>
        /// Lo llama el botón "Activar" de cada emoción (ya conectado en Start).
        /// </summary>
        public void SeleccionarNodo(int indice)
        {
            if (procesandoNodo) return;
            if (indice < 0 || indice >= nodos.Length) return;
            if (nodos[indice].activado) return;

            StartCoroutine(ProcesarNodo(indice));
        }

        private IEnumerator ProcesarNodo(int indice)
        {
            procesandoNodo = true;

            NodoEmocion nodo = nodos[indice];
            nodo.activado = true;
            nodosActivados++;

            // Bloquear todos los botones mientras esta emoción está en curso
            HabilitarBotones(false);

            // 1. MIENTRAS la cámara se acerca: todos los botones se desvanecen y los otros 5
            //    modelos se encogen, para que no estorben a los lados de la pantalla.
            StartCoroutine(OcultarOtrosParaCloseUp(indice));
            yield return StartCoroutine(MoverCamara(nodo.puntoCamaraCloseUp, duracionCloseUp));

            // 2. La emoción habla, en el mismo panel (se activa su etiqueta de nombre)
            ActivarEtiqueta(nodo.etiquetaNombre);
            yield return StartCoroutine(MostrarDialogoTexto(nodo.dialogoEmocion));

            if (panelDialogo != null) panelDialogo.SetActive(false);

            // 3. Recién AHORA que terminó de leerse: aparece el botón elegido (pop, no fade),
            //    se ilumina y sale el pulgar con pop.
            if (nodo.grupoBoton != null) nodo.grupoBoton.alpha = 1f;
            if (nodo.botonVerde != null) yield return StartCoroutine(AnimacionPop(nodo.botonVerde));

            StartCoroutine(EncenderEfectoLuz(nodo.efectoLuz));
            if (nodo.checkPulgar != null) StartCoroutine(AnimacionPop(nodo.checkPulgar));

            yield return new WaitForSeconds(duracionEncendidoLuz + 0.3f);

            // 4. La cámara vuelve a la vista general de la sala, y ahí reaparecen
            //    (con fade) los demás botones y modelos que se habían ocultado.
            yield return StartCoroutine(MoverCamara(puntoCamaraGeneral, duracionVolverGeneral));
            yield return StartCoroutine(MostrarOtrosDespuesDeCloseUp(indice));

            procesandoNodo = false;

            if (nodosActivados >= nodos.Length)
            {
                yield return StartCoroutine(SecuenciaCierre());
            }
            else
            {
                HabilitarBotones(true);
            }
        }

        private IEnumerator SecuenciaCierre()
        {
            // 1. El guía explica que ahora toca clasificar con los sliders
            yield return StartCoroutine(MostrarDialogo("Ya las conociste a todas. Ahora mueve cada barra hacia la voz que más reconoces en ti."));
            yield return StartCoroutine(MostrarDialogo("No hay respuesta correcta. Solo hay pesos, y el resultado que producen."));

            if (panelDialogo != null) panelDialogo.SetActive(false);

            // 2. Fade a negro → mostrar panel de sliders
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

            // Mostrar panel y resetear sliders al centro (50)
            if (panelSliders != null)
            {
                panelSliders.SetActive(true);
                panelSliders.transform.SetAsLastSibling();

                CanvasGroup cgSliders = panelSliders.GetComponent<CanvasGroup>();
                if (cgSliders != null)
                {
                    cgSliders.alpha = 1f;
                    cgSliders.interactable = true;
                    cgSliders.blocksRaycasts = true;
                }
            }

            // Asignar el sprite del avatar a los 3 handles
            Sprite spriteHandle = (PlayerPrefs.GetInt("Avatar", 1) == 1) ? spriteHandleHombre : spriteHandleMujer;
            if (spriteHandle != null)
            {
                if (handleImageSlider1 != null) handleImageSlider1.sprite = spriteHandle;
                if (handleImageSlider2 != null) handleImageSlider2.sprite = spriteHandle;
                if (handleImageSlider3 != null) handleImageSlider3.sprite = spriteHandle;
            }

            if (slider1MotivacionEstres != null) { slider1MotivacionEstres.minValue = 0; slider1MotivacionEstres.maxValue = 100; slider1MotivacionEstres.value = 50; }
            if (slider2SensibilidadMiedo != null) { slider2SensibilidadMiedo.minValue = 0; slider2SensibilidadMiedo.maxValue = 100; slider2SensibilidadMiedo.value = 50; }
            if (slider3AtencionMemoria != null)   { slider3AtencionMemoria.minValue = 0;   slider3AtencionMemoria.maxValue = 100;   slider3AtencionMemoria.value = 50; }

            if (botonAceptarVerde != null) botonAceptarVerde.SetActive(false);
            if (botonAceptarSliders != null) botonAceptarSliders.gameObject.SetActive(true);

            // Fade desde negro (el panel PNG cubre la escena 3D)
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

            // 3. Esperar a que el jugador presione Aceptar
            bool acepto = false;
            if (botonAceptarSliders != null)
            {
                botonAceptarSliders.onClick.RemoveAllListeners();
                botonAceptarSliders.onClick.AddListener(() => acepto = true);
            }
            else
            {
                // Sin botón asignado: aceptar automático (solo para debug)
                acepto = true;
            }

            while (!acepto) yield return null;

            // Efecto POP del botón verde (confirmación visual)
            if (botonAceptarVerde != null)
            {
                yield return StartCoroutine(AnimacionPop(botonAceptarVerde));
                yield return new WaitForSeconds(0.25f);
            }

            // 4. Leer valores de los sliders
            float v1 = slider1MotivacionEstres  != null ? slider1MotivacionEstres.value  : 50f; // Motivación ↔ Estrés
            float v2 = slider2SensibilidadMiedo != null ? slider2SensibilidadMiedo.value : 50f; // Sensibilidad ↔ Miedo
            float v3 = slider3AtencionMemoria   != null ? slider3AtencionMemoria.value   : 50f; // Atención ↔ Memoria

            // Dominancia simple: > 60 = lado derecho domina, < 40 = lado izquierdo domina
            bool miedoDomina       = v2 > 60f;   // Slider 2 derecha
            bool memoriaDomina     = v3 > 60f;   // Slider 3 derecha
            bool motivacionDomina  = v1 < 40f;   // Slider 1 izquierda
            bool atencionDomina    = v3 < 40f;   // Slider 3 izquierda
            bool estresDomina      = v1 > 60f;   // Slider 1 derecha
            bool sensibilidadDomina = v2 < 40f;  // Slider 2 izquierda

            // 5. Fade a negro → ocultar panel → encender luces en la escena 3D
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

            if (panelSliders != null) panelSliders.SetActive(false);

            // Encender mini-luces de las emociones dominantes
            // Orden en nodos[]: 0=Sensibilidad, 1=Atención, 2=Miedo, 3=Memoria, 4=Motivación, 5=Estrés
            EncenderLucesSegunDominantes(sensibilidadDomina, atencionDomina, miedoDomina, memoriaDomina, motivacionDomina, estresDomina);

            // Determinar color de la luz de ambiente
            Color colorDestino;
            if (miedoDomina && memoriaDomina)
                colorDestino = colorMiedoMemoria;
            else if (motivacionDomina && atencionDomina)
                colorDestino = colorMotivacionAtencion;
            else
                colorDestino = colorNeutral;

            StartCoroutine(CambiarColorLuzAmbiente(colorDestino));

            // Fade desde negro (ahora se ve la escena 3D con las luces encendidas)
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

            // 6. Diálogo del guía según el resultado (reconoce stats y aplica líneas del Excel)
            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");

            if (miedoDomina && memoriaDomina)
            {
                yield return StartCoroutine(MostrarDialogo("Parece que hoy el Miedo y la Memoria tuvieron más peso en tu balanza."));
                yield return StartCoroutine(MostrarDialogo("Lo que hemos vivido puede acompañarnos incluso cuando estamos frente a algo nuevo."));
                yield return StartCoroutine(MostrarDialogo("Puedes escuchar lo que esas experiencias tienen que decirte..."));
                yield return StartCoroutine(MostrarDialogo("Pero no olvides que este momento también merece ser vivido por sí mismo. 😊 😊"));
                
    
            }
            else if (motivacionDomina && atencionDomina)
            {
                yield return StartCoroutine(MostrarDialogo("Hoy, la Motivación y la Atención al presente tuvieron más peso en tu balanza."));
                yield return StartCoroutine(MostrarDialogo("Mismo estímulo, otro mundo. Ninguna voz sobra, lo que cambia es el peso."));
                yield return StartCoroutine(MostrarDialogo("Esta vez, tu mirada encontró espacio para lo que está sucediendo ahora..."));
                yield return StartCoroutine(MostrarDialogo("Y para aquello que todavía deseas alcanzar. 😊"));
            }
            else
            {
                yield return StartCoroutine(MostrarDialogo("Hoy cada voz encontro su lugar repartido en la mesa."));
                yield return StartCoroutine(MostrarDialogo("Ninguna voz sobra en esta mesa. Lo que cambia es el peso que le das a cada una."));
                yield return StartCoroutine(MostrarDialogo("Todas tienen algo que decir. Al escucharlas juntas, puedes descubrir nuevas formas de sentir y comprender una misma señal. 😊"));
            }

            // Cierre orgánico final de la junta antes de salir al mapa
            yield return StartCoroutine(MostrarDialogo("Has comprendido el mensaje, " + nombreJugador + ". El dolor no es un daño fijo: es el resultado de cómo negocia esta mesa."));

            if (panelDialogo != null) panelDialogo.SetActive(false);
            yield return new WaitForSeconds(0.35f);

            // Guardar progreso de esta estación
            PlayerPrefs.SetInt(claveGuardadoEstacion, 1);
            PlayerPrefs.Save();

            // Fade a negro para volver al mapa
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

            // Con la pantalla en negro: ocultar modelos/efectos y devolver las cámaras
            OcultarEmociones();
            RestaurarCamaras();

            // Abrir el mapa con el progreso actualizado (la zona 3 queda marcada como completada)
            MapaSeleccionCapitulo2 mapaSeleccion = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapaSeleccion != null)
            {
                mapaSeleccion.gameObject.SetActive(true);
                mapaSeleccion.AbrirMapa();
            }

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

            estacionIniciada = false;
            onEstacionCompletada?.Invoke();
        }

        /// <summary>
        /// Enciende las mini-luces de cada emoción según cuáles dominan en los sliders.
        /// Orden de nodos[]: 0=Sensibilidad, 1=Atención, 2=Miedo, 3=Memoria, 4=Motivación, 5=Estrés
        /// </summary>
        private void EncenderLucesSegunDominantes(bool sensibilidad, bool atencion, bool miedo, bool memoria, bool motivacion, bool estres)
        {
            bool[] dominan = { sensibilidad, atencion, miedo, memoria, motivacion, estres };
            for (int i = 0; i < nodos.Length && i < dominan.Length; i++)
            {
                if (dominan[i] && nodos[i].efectoLuz != null)
                {
                    StartCoroutine(EncenderEfectoLuz(nodos[i].efectoLuz));
                }
            }
        }

        /// <summary>
        /// Cambia suavemente el color de la luz de ambiente al color del resultado.
        /// </summary>
        private IEnumerator CambiarColorLuzAmbiente(Color colorDestino)
        {
            if (luzAmbiente == null) yield break;

            Color colorInicial = luzAmbiente.color;
            float t = 0f;
            while (t < duracionCambioLuz)
            {
                t += Time.deltaTime;
                luzAmbiente.color = Color.Lerp(colorInicial, colorDestino, t / duracionCambioLuz);
                yield return null;
            }
            luzAmbiente.color = colorDestino;
        }



        private Transform ObtenerTransformCamara()
        {
            if (camaraEstacion != null) return camaraEstacion.transform;
            if (transformCamara != null) return transformCamara;
            if (Camera.main != null) return Camera.main.transform;
            return null;
        }

        // Deja la cámara en la vista frontal general de la sala (sin mover al jugador ni al Guía)
        private void PrepararCamara()
        {
            camarasDesactivadas.Clear();
            camaraExistenteMovida = false;

            if (camaraEstacion != null)
            {
                // Apagar las demás cámaras (guardándolas para devolverlas al final)
                Camera[] todasLasCamaras = FindObjectsOfType<Camera>();
                foreach (Camera c in todasLasCamaras)
                {
                    if (c.gameObject != camaraEstacion && !c.transform.IsChildOf(camaraEstacion.transform))
                    {
                        camarasDesactivadas.Add(c.gameObject);
                        c.gameObject.SetActive(false);
                    }
                }

                camaraEstacion.SetActive(true);
            }
            else
            {
                // Sin cámara dedicada: se mueve una cámara existente y se restaura al salir
                Transform cam = ObtenerTransformCamara();
                if (cam != null)
                {
                    posCamaraOriginal = cam.position;
                    rotCamaraOriginal = cam.rotation;
                    camaraExistenteMovida = true;
                }
            }

            Transform camaraActual = ObtenerTransformCamara();
            if (camaraActual != null && puntoCamaraGeneral != null)
            {
                camaraActual.position = puntoCamaraGeneral.position;
                camaraActual.rotation = puntoCamaraGeneral.rotation;
            }
        }

        private void RestaurarCamaras()
        {
            if (camaraEstacion != null) camaraEstacion.SetActive(false);

            for (int i = 0; i < camarasDesactivadas.Count; i++)
            {
                if (camarasDesactivadas[i] != null) camarasDesactivadas[i].SetActive(true);
            }
            camarasDesactivadas.Clear();

            if (camaraExistenteMovida)
            {
                Transform cam = ObtenerTransformCamara();
                if (cam != null)
                {
                    cam.position = posCamaraOriginal;
                    cam.rotation = rotCamaraOriginal;
                }
                camaraExistenteMovida = false;
            }
        }

        // Movimiento suave de la cámara hacia un punto (close-up o vuelta a la vista general)
        private IEnumerator MoverCamara(Transform destino, float duracion)
        {
            Transform cam = ObtenerTransformCamara();
            if (cam == null || destino == null) yield break;

            Vector3 posInicial = cam.position;
            Quaternion rotInicial = cam.rotation;

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float suave = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracion));

                cam.position = Vector3.Lerp(posInicial, destino.position, suave);
                cam.rotation = Quaternion.Slerp(rotInicial, destino.rotation, suave);
                yield return null;
            }

            cam.position = destino.position;
            cam.rotation = destino.rotation;
        }

        // ---------------- FADE DE BOTONES Y MODELOS (close-up) ----------------

        // Al hacer close-up a "indiceSeleccionado": desvanece TODOS los botones (incluido el
        // suyo, que reaparece después con pop) y encoge los OTROS 5 modelos para que no
        // estorben a los lados de la pantalla mientras la cámara se acerca.
        private IEnumerator OcultarOtrosParaCloseUp(int indiceSeleccionado)
        {
            for (int i = 0; i < nodos.Length; i++)
            {
                NodoEmocion n = nodos[i];

                if (n.grupoBoton != null)
                {
                    StartCoroutine(FadeCanvasGroup(n.grupoBoton, 0f, duracionFadeUI));
                }

                if (i != indiceSeleccionado && n.modelo3D != null)
                {
                    StartCoroutine(FadeEscalaModelo(n.modelo3D.transform, n.escalaBase, 0f, duracionFadeUI));
                }
            }

            yield return new WaitForSeconds(duracionFadeUI);
        }

        // Al volver a la vista general: reaparecen (con fade) los botones y modelos de las
        // demás emociones. La emoción seleccionada ya reapareció antes, con su propio pop.
        private IEnumerator MostrarOtrosDespuesDeCloseUp(int indiceSeleccionado)
        {
            for (int i = 0; i < nodos.Length; i++)
            {
                if (i == indiceSeleccionado) continue;

                NodoEmocion n = nodos[i];

                if (n.grupoBoton != null)
                {
                    StartCoroutine(FadeCanvasGroup(n.grupoBoton, 1f, duracionFadeUI));
                }

                if (n.modelo3D != null)
                {
                    StartCoroutine(FadeEscalaModelo(n.modelo3D.transform, n.escalaBase, 1f, duracionFadeUI));
                }
            }

            yield return new WaitForSeconds(duracionFadeUI);
        }

        // Fade de alpha genérico para un CanvasGroup (usado por los botones)
        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float alphaDestino, float duracion)
        {
            if (cg == null) yield break;

            float alphaInicial = cg.alpha;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(alphaInicial, alphaDestino, t / duracion);
                yield return null;
            }
            cg.alpha = alphaDestino;
        }

        // "Fade" por escala para los modelos 3D: encoge hacia 0 o vuelve a su tamaño base.
        // (Los modelos no tienen un canal de transparencia garantizado, así que encoger
        // suavemente da el mismo efecto visual de "desaparecer" sin depender del material.)
        private IEnumerator FadeEscalaModelo(Transform modelo, Vector3 escalaBase, float factorDestino, float duracion)
        {
            if (modelo == null) yield break;

            Vector3 escalaInicial = modelo.localScale;
            Vector3 escalaDestino = escalaBase * factorDestino;

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                modelo.localScale = Vector3.Lerp(escalaInicial, escalaDestino, t / duracion);
                yield return null;
            }
            modelo.localScale = escalaDestino;
        }

        // ---------------- EFECTOS ----------------

        // Enciende suavemente la luz de la emoción (si el objeto tiene una Light, sube su intensidad)
        private IEnumerator EncenderEfectoLuz(GameObject efecto)
        {
            if (efecto == null) yield break;

            Light luz = efecto.GetComponent<Light>();
            if (luz == null) luz = efecto.GetComponentInChildren<Light>(true);

            float intensidadFinal = (luz != null) ? luz.intensity : 0f;
            if (luz != null) luz.intensity = 0f;

            efecto.SetActive(true);
            if (luz == null) yield break;

            float t = 0f;
            while (t < duracionEncendidoLuz)
            {
                t += Time.deltaTime;
                luz.intensity = Mathf.Lerp(0f, intensidadFinal, t / duracionEncendidoLuz);
                yield return null;
            }
            luz.intensity = intensidadFinal;
        }

        // Pop suave: escala 0 -> escalaMaxPop -> escala original (sirve para el botón verde y el pulgar)
        private IEnumerator AnimacionPop(GameObject objetivo)
        {
            if (objetivo == null) yield break;

            Vector3 escalaFinal = objetivo.transform.localScale;
            if (escalaFinal.x <= 0.05f) escalaFinal = Vector3.one;

            objetivo.SetActive(true);
            Transform tr = objetivo.transform;
            tr.localScale = Vector3.zero;

            float tiempo = 0f;
            while (tiempo < duracionPop)
            {
                tiempo += Time.deltaTime;
                float progreso = Mathf.Clamp01(tiempo / duracionPop);

                float multiplicador;
                if (progreso < 0.65f)
                {
                    multiplicador = Mathf.Lerp(0f, escalaMaxPop, progreso / 0.65f);
                }
                else
                {
                    multiplicador = Mathf.Lerp(escalaMaxPop, 1f, (progreso - 0.65f) / 0.35f);
                }

                tr.localScale = escalaFinal * multiplicador;
                yield return null;
            }

            tr.localScale = escalaFinal;
        }

        // ---------------- DIÁLOGOS ----------------

        // Diálogo del Guía: activa su etiqueta y usa el panel único
        private IEnumerator MostrarDialogo(string mensaje)
        {
            ActivarEtiqueta(etiquetaGuia);
            yield return StartCoroutine(MostrarDialogoTexto(mensaje));
        }

        // Diálogo con typewriter en el panel único; espera a que el jugador pulse Continuar.
        // Asume que la etiqueta de nombre correcta ya fue activada antes de llamar esto.
        private IEnumerator MostrarDialogoTexto(string mensaje)
        {
            if (panelDialogo != null) panelDialogo.SetActive(true);
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);

            textoCompletoActual = mensaje;
            textoTerminado = false;
            esperandoContinuar = true;

            if (textoDialogo != null) textoDialogo.text = "";

            foreach (char letra in mensaje)
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

        // El botón "Continuar" del panel único llama a este método
        public void OnClickContinuar()
        {
            if (Time.time - tiempoUltimoClick < 0.12f) return;
            tiempoUltimoClick = Time.time;

            // Si aún está escribiendo, mostrar el texto completo de golpe
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
    }
}