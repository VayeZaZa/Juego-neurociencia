using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

namespace Capitulo2
{
    public class MapaSeleccionCapitulo2 : MonoBehaviour
    {
        [System.Serializable]
        public class EstacionZona
        {
            public string nombreEstacion = "Estación 1";
            public Button botonZona;             // Botón clickeable transparente en el mapa
            public Transform puntoDestino;       // (Opcional) Si está vacío, van directo a la posición del botón
            public GameObject checkZona;         // Check / tick visual que hace POP
            public GameObject marcaAvance;       // (Opcional) Icono o sello que muestra que esta estación ya fue superada
            [System.NonSerialized] public Vector3 escalaOriginalCheck = Vector3.one;
            [System.NonSerialized] public Vector3 escalaOriginalMarca = Vector3.one;
        }

        [Header("Panel Principal del Mapa")]
        [SerializeField] private GameObject panelMapa;
        [SerializeField] private Transform contenedorVisualMapa; // Objeto que palpita suavecito
        [SerializeField] private bool activarPalpitar = true;
        [SerializeField] private float velocidadPalpitar = 2.5f;
        [SerializeField] private float amplitudPalpitar = 0.02f; // Palpitar muy suave (1.0 +/- 0.02)

        [Header("Fichas / Iconos de los Personajes")]
        [SerializeField] private Transform contenedorFichas;     // El objeto padre que contiene los muñequitos
        [SerializeField] private GameObject iconoGuia;           // Icono del Guía
        [SerializeField] private GameObject iconoHombre;         // Icono si es Hombre
        [SerializeField] private GameObject iconoMujer;          // Icono si es Mujer
        [SerializeField] private float duracionViajeFichas = 0.45f; // Tiempo que tardan en deslizarse

        [Header("Zonas del Mapa (4 Estaciones)")]
        [SerializeField] private EstacionZona[] estaciones = new EstacionZona[4];

        [Header("Botón Confirmar")]
        [SerializeField] private Button botonConfirmar;
        [SerializeField] private GameObject checkConfirmar;      // Check que hace POP al pulsar Confirmar

        [Header("Fade a Negro")]
        [SerializeField] private CanvasGroup panelFadeNegro;
        public CanvasGroup PanelFadeNegro => panelFadeNegro;
        [SerializeField] private float duracionFadeNegro = 0.5f;

        [Header("Eventos")]
        [Tooltip("Se dispara tras hacer fade a negro confirmando la estación elegida (1, 2, 3 o 4)")]
        public UnityEvent<int> onZonaConfirmada;

        private Vector3 escalaBaseMapa = Vector3.one;
        private Vector3 escalaBaseBotonConfirmar = Vector3.one;
        private Vector3 escalaBaseCheckConfirmar = Vector3.one;
        private int zonaSeleccionada = -1;
        public int ZonaSeleccionada => zonaSeleccionada;
        private Coroutine coroutineMoverFichas;
        private Coroutine coroutinePopCheck;
        private Coroutine coroutinePopBoton;
        private bool confirmando = false;

        private void OnEnable()
        {
            ActualizarEstadoEstaciones();
        }

        private void Start()
        {
            if (contenedorVisualMapa != null)
            {
                escalaBaseMapa = contenedorVisualMapa.localScale;
            }

            // Ocultar mapa inicialmente
            if (panelMapa != null) panelMapa.SetActive(false);

            // Guardar escala base y ocultar botón de confirmar y su check
            if (botonConfirmar != null)
            {
                if (botonConfirmar.transform.localScale != Vector3.zero)
                    escalaBaseBotonConfirmar = botonConfirmar.transform.localScale;
                botonConfirmar.gameObject.SetActive(false);
            }

            if (checkConfirmar != null)
            {
                if (checkConfirmar.transform.localScale != Vector3.zero)
                    escalaBaseCheckConfirmar = checkConfirmar.transform.localScale;
                checkConfirmar.SetActive(false);
            }

            if (panelFadeNegro != null)
            {
                panelFadeNegro.alpha = 0f;
                panelFadeNegro.blocksRaycasts = false;
                panelFadeNegro.gameObject.SetActive(false);
            }

            // Guardar escalas base de los checks y auto-vincular botones
            for (int i = 0; i < estaciones.Length; i++)
            {
                if (estaciones[i] == null) continue;

                if (estaciones[i].checkZona != null)
                {
                    if (estaciones[i].checkZona.transform.localScale.x > 0.05f)
                        estaciones[i].escalaOriginalCheck = estaciones[i].checkZona.transform.localScale;
                    else
                        estaciones[i].escalaOriginalCheck = Vector3.one;
                }

                int index = i;
                if (estaciones[i].botonZona != null)
                {
                    estaciones[i].botonZona.onClick.RemoveAllListeners();
                    estaciones[i].botonZona.onClick.AddListener(() => SeleccionarZona(index));
                }
            }

            ActualizarEstadoEstaciones();

            // Auto-vincular botón de confirmar
            if (botonConfirmar != null)
            {
                botonConfirmar.onClick.RemoveListener(ConfirmarSeleccion);
                botonConfirmar.onClick.AddListener(ConfirmarSeleccion);
            }
        }

        private void Update()
        {
            // Efecto de palpitar suavecito y sutil del mapa
            if (activarPalpitar && contenedorVisualMapa != null && panelMapa != null && panelMapa.activeSelf)
            {
                float factor = 1f + Mathf.Sin(Time.time * velocidadPalpitar) * amplitudPalpitar;
                contenedorVisualMapa.localScale = escalaBaseMapa * factor;
            }
        }

        /// <summary>
        /// Oculta completamente el panel del mapa y su interfaz.
        /// </summary>
        public void OcultarMapa()
        {
            if (panelMapa != null)
            {
                panelMapa.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Abre el mapa con animación y configura los iconos según el género elegido.
        /// Conéctalo al evento OnFinIntroduccion de DialogosCapitulo2.
        /// </summary>
        public void AbrirMapa()
        {
            confirmando = false;
            zonaSeleccionada = -1;

            if (panelMapa != null) panelMapa.SetActive(true);

            // Reiniciar botón confirmar y su check
            if (botonConfirmar != null)
            {
                botonConfirmar.gameObject.SetActive(false);
                botonConfirmar.transform.localScale = escalaBaseBotonConfirmar;
            }
            if (checkConfirmar != null)
            {
                checkConfirmar.SetActive(false);
                checkConfirmar.transform.localScale = escalaBaseCheckConfirmar;
            }

            // 1. Configurar icono del jugador según su selección previa
            int avatarElegido = PlayerPrefs.GetInt("Avatar", 1); // 1 = Hombre, 2 = Mujer
            if (iconoHombre != null) iconoHombre.SetActive(avatarElegido == 1);
            if (iconoMujer != null) iconoMujer.SetActive(avatarElegido == 2);
            if (iconoGuia != null) iconoGuia.SetActive(true);

            // 2. Actualizar estado de las estaciones (completadas con check y bloqueadas)
            ActualizarEstadoEstaciones();
        }

        public bool EsEstacionCompletada(int index)
        {
            int num = index + 1;
            return PlayerPrefs.GetInt("Cap2_Estacion" + num + "_Completada", 0) == 1
                || PlayerPrefs.GetInt("Estacion" + num + "_Completada", 0) == 1;
        }

        public void ActualizarEstadoEstaciones()
        {
            for (int i = 0; i < estaciones.Length; i++)
            {
                if (estaciones[i] == null) continue;

                bool completada = EsEstacionCompletada(i);

                if (completada)
                {
                    // 1. Mostrar texto PNG de completado
                    if (estaciones[i].marcaAvance != null)
                    {
                        estaciones[i].marcaAvance.SetActive(true);
                    }

                    // 2. Mostrar check permanente de completado
                    if (estaciones[i].checkZona != null)
                    {
                        estaciones[i].checkZona.SetActive(true);
                        estaciones[i].checkZona.transform.localScale = estaciones[i].escalaOriginalCheck;
                    }

                    // 3. Bloquear botón para que no se pueda volver a entrar
                    if (estaciones[i].botonZona != null)
                    {
                        estaciones[i].botonZona.interactable = false;
                    }
                }
                else
                {
                    // No completada:
                    // 1. Ocultar texto PNG de completado
                    if (estaciones[i].marcaAvance != null)
                    {
                        estaciones[i].marcaAvance.SetActive(false);
                    }

                    // 2. El check de selección SOLO se muestra si esta zona es la actualmente seleccionada
                    bool estaSeleccionada = (zonaSeleccionada == i + 1);
                    if (estaciones[i].checkZona != null)
                    {
                        estaciones[i].checkZona.SetActive(estaSeleccionada);
                        estaciones[i].checkZona.transform.localScale = estaciones[i].escalaOriginalCheck;
                    }

                    // 3. Botón disponible para poder seleccionarla
                    if (estaciones[i].botonZona != null)
                    {
                        estaciones[i].botonZona.interactable = true;
                    }
                }
            }
        }

        /// <summary>
        /// Se llama al hacer clic en cualquiera de las 4 zonas del mapa.
        /// </summary>
        public void SeleccionarZona(int indexZona)
        {
            if (confirmando || indexZona < 0 || indexZona >= estaciones.Length) return;

            // Si ya está completada, no se puede volver a entrar
            if (EsEstacionCompletada(indexZona)) return;

            zonaSeleccionada = indexZona + 1; // Estación 1, 2, 3 o 4

            // 1. Checks con efecto POP (las completadas siempre conservan su check visible)
            if (coroutinePopCheck != null) StopCoroutine(coroutinePopCheck);

            for (int i = 0; i < estaciones.Length; i++)
            {
                if (estaciones[i] == null) continue;

                if (EsEstacionCompletada(i))
                {
                    if (estaciones[i].marcaAvance != null)
                    {
                        estaciones[i].marcaAvance.SetActive(true);
                    }
                    if (estaciones[i].checkZona != null)
                    {
                        estaciones[i].checkZona.SetActive(true);
                        estaciones[i].checkZona.transform.localScale = estaciones[i].escalaOriginalCheck;
                    }
                    continue;
                }

                // Estación no completada: su marca de completado siempre oculta
                if (estaciones[i].marcaAvance != null)
                {
                    estaciones[i].marcaAvance.SetActive(false);
                }

                if (estaciones[i].checkZona != null)
                {
                    if (i == indexZona)
                    {
                        coroutinePopCheck = StartCoroutine(AnimacionPopSuave(estaciones[i].checkZona, estaciones[i].escalaOriginalCheck));
                    }
                    else
                    {
                        estaciones[i].checkZona.SetActive(false);
                        estaciones[i].checkZona.transform.localScale = estaciones[i].escalaOriginalCheck;
                    }
                }
            }

            // 2. Mover los muñequitos suavemente hasta el punto de la estación
            Vector3 destino = Vector3.zero;
            bool destinoValido = false;

            if (estaciones[indexZona].puntoDestino != null)
            {
                destino = estaciones[indexZona].puntoDestino.position;
                destinoValido = true;
            }
            else if (estaciones[indexZona].botonZona != null)
            {
                destino = estaciones[indexZona].botonZona.transform.position;
                destinoValido = true;
            }

            if (destinoValido && contenedorFichas != null)
            {
                contenedorFichas.gameObject.SetActive(true);
                if (coroutineMoverFichas != null) StopCoroutine(coroutineMoverFichas);
                coroutineMoverFichas = StartCoroutine(MoverFichasHacia(destino));
            }

            // 3. Mostrar el botón Confirmar con efecto POP al aparecer
            if (botonConfirmar != null)
            {
                if (!botonConfirmar.gameObject.activeSelf)
                {
                    if (coroutinePopBoton != null) StopCoroutine(coroutinePopBoton);
                    coroutinePopBoton = StartCoroutine(AnimacionPopSuave(botonConfirmar.gameObject, escalaBaseBotonConfirmar));
                }
            }

            // Asegurar que el check de confirmación permanezca oculto hasta hacer clic en Confirmar
            if (checkConfirmar != null)
            {
                checkConfirmar.SetActive(false);
                checkConfirmar.transform.localScale = escalaBaseCheckConfirmar;
            }
        }

        /// <summary>
        /// Se llama al pulsar el botón Confirmar.
        /// </summary>
        public void ConfirmarSeleccion()
        {
            if (confirmando || zonaSeleccionada == -1) return;
            confirmando = true;

            // Guardar la estación elegida en memoria
            PlayerPrefs.SetInt("Cap2_EstacionSeleccionada", zonaSeleccionada);
            PlayerPrefs.Save();

            StartCoroutine(SecuenciaConfirmarConFade());
        }

        private IEnumerator SecuenciaConfirmarConFade()
        {
            // 1. POP del check en el botón Confirmar
            if (checkConfirmar != null)
            {
                yield return StartCoroutine(AnimacionPopSuave(checkConfirmar, escalaBaseCheckConfirmar, 1.15f, 0.22f));
            }
            else if (botonConfirmar != null)
            {
                // Respaldo por si aún no se asignó checkConfirmar en el Inspector
                yield return StartCoroutine(AnimacionPopSuave(botonConfirmar.gameObject, escalaBaseBotonConfirmar, 1.15f, 0.2f));
            }

            // Pausa breve para apreciar el check
            yield return new WaitForSeconds(0.25f);

            // 2. Fade a negro
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

            yield return new WaitForSeconds(0.2f);

            // Ocultar el mapa en pantalla negra
            OcultarMapa();

            // Disparar evento para coordinar la siguiente acción
            onZonaConfirmada?.Invoke(zonaSeleccionada);

#if UNITY_2023_1_OR_NEWER
            if (zonaSeleccionada == 1)
            {
                EstacionAdelsonCapitulo2 estacion1 = Object.FindFirstObjectByType<EstacionAdelsonCapitulo2>();
                if (estacion1 != null)
                {
                    estacion1.IniciarEstacion();
                }
            }
            else if (zonaSeleccionada == 2)
            {
                EstacionFerrariCapitulo2 estacion2 = Object.FindFirstObjectByType<EstacionFerrariCapitulo2>();
                if (estacion2 != null)
                {
                    estacion2.IniciarEstacion();
                }
            }
#else
            if (zonaSeleccionada == 1)
            {
                EstacionAdelsonCapitulo2 estacion1 = FindObjectOfType<EstacionAdelsonCapitulo2>();
                if (estacion1 != null)
                {
                    estacion1.IniciarEstacion();
                }
            }
            else if (zonaSeleccionada == 2)
            {
                EstacionFerrariCapitulo2 estacion2 = FindObjectOfType<EstacionFerrariCapitulo2>();
                if (estacion2 != null)
                {
                    estacion2.IniciarEstacion();
                }
            }
#endif
        }

        // Desplazamiento fluido y suave en pantalla hacia el destino (usando posición global de UI)
        private IEnumerator MoverFichasHacia(Vector3 posDestino)
        {
            Vector3 posInicial = contenedorFichas.position;
            float tiempo = 0f;

            while (tiempo < duracionViajeFichas)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracionViajeFichas);
                float tSuave = Mathf.SmoothStep(0f, 1f, t);
                contenedorFichas.position = Vector3.Lerp(posInicial, posDestino, tSuave);
                yield return null;
            }

            contenedorFichas.position = posDestino;
        }

        // Efecto POP suave, ágil y sutil (escala 0 -> escalaMax -> 1.0)
        private IEnumerator AnimacionPopSuave(GameObject targetObj, float escalaMax = 1.15f, float duracion = 0.22f)
        {
            Vector3 escalaDestino = Vector3.one;
            if (targetObj != null && targetObj.transform.localScale != Vector3.zero)
            {
                escalaDestino = targetObj.transform.localScale;
            }
            return AnimacionPopSuave(targetObj, escalaDestino, escalaMax, duracion);
        }

        private IEnumerator AnimacionPopSuave(GameObject targetObj, Vector3 escalaFinal, float escalaMax = 1.15f, float duracion = 0.22f)
        {
            if (targetObj == null) yield break;

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

#if UNITY_EDITOR
        [ContextMenu("Debug: Borrar Todo el Progreso Guardado")]
        public void DebugBorrarProgreso()
        {
            for (int i = 1; i <= 4; i++)
            {
                PlayerPrefs.DeleteKey("Cap2_Estacion" + i + "_Completada");
                PlayerPrefs.DeleteKey("Estacion" + i + "_Completada");
            }
            PlayerPrefs.DeleteKey("Cap2_EstacionSeleccionada");
            PlayerPrefs.Save();
            zonaSeleccionada = -1;
            ActualizarEstadoEstaciones();
            Debug.Log("MapaSeleccionCapitulo2: Todo el progreso ha sido borrado.");
        }

        [ContextMenu("Debug: Simular Estación 1 Completada")]
        public void DebugCompletarEstacion1()
        {
            PlayerPrefs.SetInt("Cap2_Estacion1_Completada", 1);
            PlayerPrefs.Save();
            ActualizarEstadoEstaciones();
            Debug.Log("MapaSeleccionCapitulo2: Estación 1 marcada como completada.");
        }

        [ContextMenu("Debug: Simular Estación 2 Completada")]
        public void DebugCompletarEstacion2()
        {
            PlayerPrefs.SetInt("Cap2_Estacion2_Completada", 1);
            PlayerPrefs.Save();
            ActualizarEstadoEstaciones();
            Debug.Log("MapaSeleccionCapitulo2: Estación 2 marcada como completada.");
        }
#endif
    }
}
