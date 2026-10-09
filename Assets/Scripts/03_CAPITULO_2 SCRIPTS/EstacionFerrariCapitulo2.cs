using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

namespace Capitulo2
{
    [AddComponentMenu("Capitulo 2/Estacion Ferrari y Tobillo")]
    public class EstacionFerrariCapitulo2 : MonoBehaviour
    {
        [Header("--- Avatares y Posiciones Iniciales ---")]
        [Tooltip("Objeto del personaje Hombre en la escena")]
        [SerializeField] private GameObject jugadorHombre;
        [Tooltip("Objeto del personaje Mujer en la escena")]
        [SerializeField] private GameObject jugadorMujer;
        [Tooltip("Punto donde inicia caminando tranquilamente por el parque")]
        [SerializeField] private Transform puntoInicioJugador;
        [Tooltip("(Opcional) Punto hacia donde camina antes de tropezarse")]
        [SerializeField] private Transform puntoTropiezoJugador;
        [SerializeField] private float velocidadCaminata = 1.8f;
        [SerializeField] private float tiempoCaminata = 2.8f;

        [Header("--- Nombres de Animaciones (Animator) ---")]
        [SerializeField] private string animCaminar = "Walking";
        [SerializeField] private string animCaerse = "Tripping";
        [SerializeField] private string animCorrerHerido = "Injured Run";
        [SerializeField] private string animIdle = "Idle";

        public string AnimCorrerHerido => animCorrerHerido;
        public string AnimIdle => animIdle;
        /// <summary>Expone el modelo Hombre para que lo use el Minijuego.</summary>
        public GameObject JugadorHombre => jugadorHombre;
        /// <summary>Expone el modelo Mujer para que lo use el Minijuego.</summary>
        public GameObject JugadorMujer => jugadorMujer;
        /// <summary>Expone el panel de fade negro para que el Minijuego lo reutilice.</summary>
        public CanvasGroup PanelFadeNegro => panelFadeNegro;
        /// <summary>Expone la cámara de la cinemática.</summary>
        public GameObject CamaraEstacion => camaraEstacion;


        [Header("--- Tiempos de Animación ---")]
        [Tooltip("Tiempo en segundos tras iniciar 'Tripping' en el que el personaje toca el suelo y se congela en pose de caído")]
        [SerializeField] private float tiempoHastaPausarCaida = 0.85f;

        [Header("--- Efecto de Dolor (Destello Rojo) ---")]
        [Tooltip("CanvasGroup con una imagen roja a pantalla completa para el estallido suave")]
        [SerializeField] private CanvasGroup flashRojo;
        [SerializeField] private float alphaMaxFlashRojo = 0.45f;
        [SerializeField] private float duracionFadeInFlash = 0.15f;
        [SerializeField] private float duracionFadeOutFlash = 0.45f;

        [Header("--- El Ferrari (Carro Loco) ---")]
        [Tooltip("Transform del Ferrari")]
        [SerializeField] private Transform transformFerrari;
        [Tooltip("Punto inicial donde aparece el Ferrari")]
        [SerializeField] private Transform puntoInicioFerrari;
        [Tooltip("Punto hacia donde avanza el Ferrari")]
        [SerializeField] private Transform puntoFinFerrari;
        [SerializeField] private float velocidadFerrari = 7.0f;
        [SerializeField] private float duracionAvanceFerrari = 2.0f;
        [SerializeField] private float amplitudZigzag = 1.6f;
        [SerializeField] private float frecuenciaZigzag = 8.5f;

        [Header("--- Cámara de la Estación ---")]
        [Tooltip("Cámara dedicada para la Estación 2")]
        [SerializeField] private GameObject camaraEstacion;
        [Tooltip("(Opcional) Transform de cámara a mover directamente si no se usa objeto separado")]
        [SerializeField] private Transform transformCamara;
        [Tooltip("Punto de cámara que enfoca al jugador caminando y cayéndose")]
        [SerializeField] private Transform puntoCamaraJugador;
        [Tooltip("Punto de cámara que enfoca al Ferrari loco avanzando")]
        [SerializeField] private Transform puntoCamaraFerrari;
        [Tooltip("Punto de cámara final donde comenzará el minijuego")]
        [SerializeField] private Transform puntoCamaraInicioJuego;
        [SerializeField] private float duracionGiroCamara = 0.85f;

        [Header("--- UI de Diálogos ---")]
        [SerializeField] private GameObject panelDialogo;
        [SerializeField] private TMP_Text textoDialogo;
        [SerializeField] private Button botonContinuar;
        [SerializeField] private float velocidadTypewriter = 0.03f;

        [Header("--- Fade a Negro de Pantalla ---")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        [SerializeField] private float duracionFadeNegro = 0.5f;

        [Header("--- Eventos de Progreso ---")]
        [Tooltip("Se dispara cuando termina la cinemática y la pantalla hace fade a negro, listo para iniciar el minijuego")]
        public UnityEvent alTerminarCinematica;

        [Header("--- Pruebas / Debug ---")]
        [Tooltip("Si está activo, la estación arranca sola al dar Play sin necesidad de pasar por el mapa")]
        [SerializeField] private bool iniciarAlEmpezar = false;

        // Variables internas
        private GameObject jugadorActivo;
        private Animator animJugadorActivo;
        private bool cinematicaIniciada = false;
        private bool esperandoContinuar = false;
        private bool textoTerminado = false;
        private string textoCompletoActual = "";
        private float tiempoUltimoClick = 0f;

        private void Awake()
        {
            ConectarConMapa();

            if (flashRojo != null)
            {
                flashRojo.alpha = 0f;
                flashRojo.blocksRaycasts = false;
                flashRojo.gameObject.SetActive(false);
            }

            ActivarPanelDialogo(false);
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

            ActivarPanelDialogo(false);

            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveListener(OnClickContinuar);
                botonContinuar.onClick.AddListener(OnClickContinuar);
            }

            if (iniciarAlEmpezar)
            {
                IniciarEstacion();
            }
        }

        /// <summary>
        /// Conecta este método al evento OnZonaConfirmada del mapa (Zona 2).
        /// </summary>
        public void OnZonaConfirmadaDesdeMapa(int zona)
        {
            // Validar contra la estación real elegida en el mapa
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            int seleccionReal = (mapa != null && mapa.ZonaSeleccionada > 0) ? mapa.ZonaSeleccionada : PlayerPrefs.GetInt("Cap2_EstacionSeleccionada", zona);

            if (seleccionReal == 2)
            {
                IniciarEstacion();
            }
        }

        /// <summary>
        /// Arranca la cinemática de la Estación 2 completa.
        /// </summary>
        public void IniciarEstacion()
        {
            if (cinematicaIniciada) return;
            cinematicaIniciada = true;

            StopAllCoroutines();
            StartCoroutine(SecuenciaCinematicaFerrari());
        }

        private IEnumerator SecuenciaCinematicaFerrari()
        {
            // 0. Asegurar que cualquier mapa abierto quede oculto
            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapa != null) mapa.OcultarMapa();

            // Ocultar diálogos inicialmente
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

            // 1. Configurar avatar del jugador (Hombre o Mujer)
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
                }
            }

            // 2. Ubicar el Ferrari en su punto inicial
            if (transformFerrari != null && puntoInicioFerrari != null)
            {
                transformFerrari.position = puntoInicioFerrari.position;
                transformFerrari.rotation = puntoInicioFerrari.rotation;
            }

            // 3. Activar y posicionar la cámara en el jugador
            ConfigurarCamara(puntoCamaraJugador);

            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.2f);

            // 4. Iniciar caminata y fade desde negro
            if (animJugadorActivo != null)
            {
                AsignarParametroBoolSiExiste(animJugadorActivo, "Walking", true);
                AsignarParametroBoolSiExiste(animJugadorActivo, "Caminando", true);
                animJugadorActivo.Play(animCaminar, -1, 0f);
            }

            // Fade desde negro (tanto el del mapa como el de la estación)
            CanvasGroup fadeMapa = (mapa != null) ? mapa.PanelFadeNegro : null;
            if (panelFadeNegro != null || fadeMapa != null)
            {
                float tFade = 0f;
                while (tFade < duracionFadeNegro)
                {
                    tFade += Time.deltaTime;
                    float alpha = Mathf.Clamp01(1f - (tFade / duracionFadeNegro));
                    if (panelFadeNegro != null) panelFadeNegro.alpha = alpha;
                    if (fadeMapa != null && fadeMapa != panelFadeNegro) fadeMapa.alpha = alpha;
                    yield return null;
                }
                if (panelFadeNegro != null)
                {
                    panelFadeNegro.alpha = 0f;
                    panelFadeNegro.blocksRaycasts = false;
                    panelFadeNegro.gameObject.SetActive(false);
                }
                if (fadeMapa != null && fadeMapa != panelFadeNegro)
                {
                    fadeMapa.alpha = 0f;
                    fadeMapa.blocksRaycasts = false;
                    fadeMapa.gameObject.SetActive(false);
                }
            }

            // 5. Paseo tranquilo por el parque (movimiento suave por el camino)
            Vector3 posInicialCaminata = jugadorActivo != null ? jugadorActivo.transform.position : Vector3.zero;
            Vector3 posDestinoCaminata = puntoTropiezoJugador != null 
                ? puntoTropiezoJugador.position 
                : (posInicialCaminata + (jugadorActivo != null ? jugadorActivo.transform.forward * (velocidadCaminata * tiempoCaminata) : Vector3.zero));

            float cronometro = 0f;
            while (cronometro < tiempoCaminata)
            {
                cronometro += Time.deltaTime;
                float progreso = Mathf.Clamp01(cronometro / tiempoCaminata);

                if (jugadorActivo != null)
                {
                    jugadorActivo.transform.position = Vector3.Lerp(posInicialCaminata, posDestinoCaminata, progreso);
                }

                yield return null;
            }

            // 6. ¡El tropiezo y caída directa! (De Walking a Tripping de una, sin pasar por Idle)
            if (animJugadorActivo != null)
            {
                AsignarParametroBoolSiExiste(animJugadorActivo, "Walking", false);
                AsignarParametroBoolSiExiste(animJugadorActivo, "Caminando", false);
                ActivarTriggerSiExiste(animJugadorActivo, "Tripping");
                ActivarTriggerSiExiste(animJugadorActivo, "Caerse");
                animJugadorActivo.Play(animCaerse, -1, 0f);
            }

            StartCoroutine(AnimarFlashRojo());

            // Esperar a que toque el suelo
            yield return new WaitForSeconds(tiempoHastaPausarCaida);

            // Mantener al personaje en el suelo (no levantarse inmediatamente a correr herido)
            if (animJugadorActivo != null)
            {
                animJugadorActivo.speed = 0f;
            }

            yield return new WaitForSeconds(0.25f);

            // 7. Diálogo 1: Tobillo torcido (el panel aparece de inmediato mientras sigue en el suelo)
            string nombreJugador = PlayerPrefs.GetString("NombreJugador", "Explorador");
            yield return StartCoroutine(MostrarDialogo("¡Ups! Te torciste el tobillo, " + nombreJugador + "…"));

            // 8. Paneo rápido de cámara hacia el punto del Ferrari
            ActivarPanelDialogo(false);

            yield return StartCoroutine(MoverCamaraSuavemente(puntoCamaraFerrari, duracionGiroCamara));

            // 9. ¡Avanza el Ferrari loco zigzagueando!
            yield return StartCoroutine(SecuenciaFerrariLoco());

            // 10. Diálogo 2: ¡Viene un Ferrari!
            yield return StartCoroutine(MostrarDialogo("¡OH NO! ¡Viene un Ferrari descontrolado! ¡Trata de esquivarlo!"));

            ActivarPanelDialogo(false);

            // 11. Fade a negro para dar paso al minijuego
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

            // Reanudar animador para el minijuego
            if (animJugadorActivo != null)
            {
                animJugadorActivo.speed = 1f;
            }

            // Acomodar cámara en la posición inicial del gameplay
            if (puntoCamaraInicioJuego != null)
            {
                ConfigurarCamara(puntoCamaraInicioJuego);
            }

            cinematicaIniciada = false;

            // Disparar evento para que arranque el minijuego que construiremos
            alTerminarCinematica?.Invoke();
        }

        // Simula el avance y zigzag veloz del Ferrari por unos breves segundos
        private IEnumerator SecuenciaFerrariLoco()
        {
            if (transformFerrari == null) yield break;

            Vector3 origen = transformFerrari.position;
            Vector3 destino = puntoFinFerrari != null ? puntoFinFerrari.position : (origen + transformFerrari.forward * (velocidadFerrari * duracionAvanceFerrari));

            float t = 0f;
            while (t < duracionAvanceFerrari)
            {
                t += Time.deltaTime;
                float progreso = Mathf.Clamp01(t / duracionAvanceFerrari);

                // Avance hacia adelante
                Vector3 posBase = Vector3.Lerp(origen, destino, progreso);

                // Desvío lateral sinusoidal (zigzag)
                float desvioLateral = Mathf.Sin(t * frecuenciaZigzag) * amplitudZigzag;
                Vector3 derecha = transformFerrari.right;
                transformFerrari.position = posBase + (derecha * desvioLateral);

                yield return null;
            }
        }

        // Destello rojo suave que aparece y desaparece sin ser agresivo
        private IEnumerator AnimarFlashRojo()
        {
            if (flashRojo == null) yield break;

            flashRojo.gameObject.SetActive(true);
            flashRojo.alpha = 0f;

            // Fade In rápido
            float t = 0f;
            while (t < duracionFadeInFlash)
            {
                t += Time.deltaTime;
                flashRojo.alpha = Mathf.Lerp(0f, alphaMaxFlashRojo, t / duracionFadeInFlash);
                yield return null;
            }
            flashRojo.alpha = alphaMaxFlashRojo;

            // Fade Out suave
            t = 0f;
            while (t < duracionFadeOutFlash)
            {
                t += Time.deltaTime;
                flashRojo.alpha = Mathf.Lerp(alphaMaxFlashRojo, 0f, t / duracionFadeOutFlash);
                yield return null;
            }
            flashRojo.alpha = 0f;
            flashRojo.gameObject.SetActive(false);
        }

        // Paneo suave y veloz de cámara entre dos puntos
        private IEnumerator MoverCamaraSuavemente(Transform destino, float duracion)
        {
            Transform camTransform = ObtenerTransformCamara();
            if (camTransform == null || destino == null) yield break;

            Vector3 posInicial = camTransform.position;
            Quaternion rotInicial = camTransform.rotation;

            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                float suave = Mathf.SmoothStep(0f, 1f, p);

                camTransform.position = Vector3.Lerp(posInicial, destino.position, suave);
                camTransform.rotation = Quaternion.Slerp(rotInicial, destino.rotation, suave);
                yield return null;
            }

            camTransform.position = destino.position;
            camTransform.rotation = destino.rotation;
        }

        private void ConfigurarCamara(Transform punto)
        {
            Transform camTransform = ObtenerTransformCamara();

            if (camaraEstacion != null)
            {
                Camera camEstacionComp = camaraEstacion.GetComponent<Camera>();
                if (camEstacionComp == null) camEstacionComp = camaraEstacion.GetComponentInChildren<Camera>();

                camaraEstacion.SetActive(true);
                if (camEstacionComp != null)
                {
                    camEstacionComp.enabled = true;
                    camEstacionComp.depth = 30;
                    camEstacionComp.cullingMask |= (1 << LayerMask.NameToLayer("UI"));
                }
            }

            if (camTransform != null && punto != null)
            {
                camTransform.position = punto.position;
                camTransform.rotation = punto.rotation;
            }
        }

        private Transform ObtenerTransformCamara()
        {
            if (camaraEstacion != null) return camaraEstacion.transform;
            if (transformCamara != null) return transformCamara;
            if (Camera.main != null) return Camera.main.transform;
            return null;
        }

        private void ActivarPanelDialogo(bool activar)
        {
            if (panelDialogo == null)
            {
                Canvas[] todosCanvas = FindObjectsOfType<Canvas>(true);
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
                if (botonContinuar == null)
                {
                    botonContinuar = panelDialogo.GetComponentInChildren<Button>(true);
                    if (botonContinuar != null)
                    {
                        botonContinuar.onClick.RemoveListener(OnClickContinuar);
                        botonContinuar.onClick.AddListener(OnClickContinuar);
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

                    if (botonContinuar != null)
                    {
                        botonContinuar.gameObject.SetActive(true);
                        if (botonContinuar.transform.localScale.x <= 0.05f)
                            botonContinuar.transform.localScale = Vector3.one;
                    }
                }
            }
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

        public void OnClickContinuar()
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

        /// <summary>
        /// Guarda el progreso de la estación 2 como completada en PlayerPrefs.
        /// </summary>
        public void GuardarEstacionCompletada()
        {
            PlayerPrefs.SetInt("Cap2_Estacion2_Completada", 1);
            PlayerPrefs.Save();

            MapaSeleccionCapitulo2 mapa = FindObjectOfType<MapaSeleccionCapitulo2>(true);
            if (mapa != null)
            {
                mapa.ActualizarEstadoEstaciones();
            }
        }

        private void AsignarParametroBoolSiExiste(Animator anim, string nombreParam, bool valor)
        {
            if (anim == null) return;
            foreach (AnimatorControllerParameter p in anim.parameters)
            {
                if (p.type == AnimatorControllerParameterType.Bool)
                {
                    if (p.name.Equals(nombreParam, System.StringComparison.OrdinalIgnoreCase))
                    {
                        anim.SetBool(p.name, valor);
                    }
                }
            }
        }

        private void ActivarTriggerSiExiste(Animator anim, string nombreTrigger)
        {
            if (anim == null) return;
            foreach (AnimatorControllerParameter p in anim.parameters)
            {
                if (p.type == AnimatorControllerParameterType.Trigger)
                {
                    if (p.name.Equals(nombreTrigger, System.StringComparison.OrdinalIgnoreCase))
                    {
                        anim.SetTrigger(p.name);
                    }
                }
            }
        }
    }
}
