using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

namespace Capitulo2
{
    public class CinematicaCapitulo2 : MonoBehaviour
    {
        [Header("Personajes (Avatares)")]
        [SerializeField] private GameObject hombre;
        [SerializeField] private GameObject mujer;
        [SerializeField] private GameObject avatarClinico;

        [Header("Puntos de Movimiento (Personajes)")]
        [SerializeField] private Transform inicioJugador;
        [SerializeField] private Transform inicioClinico;
        [SerializeField] private Transform finJugador;
        [SerializeField] private Transform finClinico;
        [SerializeField] private float velocidadCaminata = 1.5f;

        [Header("Cámaras")]
        [SerializeField] private Camera camaraPrincipal;       // Cámara cinematográfica que desciende
        [SerializeField] private GameObject camaraGameplay;     // Cámara fija de gameplay a activar al final
        [SerializeField] private Transform camaraStartPoint;
        [SerializeField] private Transform camaraEndPoint;
        [SerializeField] private float velocidadCamara = 2f;
        [SerializeField] private float velocidadRotacionCamara = 2f;

        [Header("UI Título y Texto (Presentación de Capítulo)")]
        [SerializeField] private CanvasGroup titulo;      // CanvasGroup de la imagen "CAPITULO 2"
        [SerializeField] private CanvasGroup texto;       // CanvasGroup del texto "● La Sala de la Interpretación"
        [SerializeField] private TMP_Text tituloTexto;    // Opcional: si quieres que el script escriba el mensaje
        [SerializeField] private string mensajeTitulo = "La Sala de la Interpretación";
        [SerializeField] private float delayAntesDeAparecer = 1.5f;    // Sale pronto mientras caminan
        [SerializeField] private float tiempoEntreTituloYTexto = 0.4f; // Intervalo ágil entre título y subtítulo
        [SerializeField] private float duracionFade = 0.8f;            // Velocidad de aparición (Fade In)
        [SerializeField] private float duracionFadeDesaparecer = 0.4f; // Velocidad para desaparecer (Fade Out) - Ajustable en Inspector

        [Header("UI de Diálogos del Juego")]
        [SerializeField] private GameObject uiDialogosJuego;

        [Header("Eventos de Finalización")]
        [SerializeField] private UnityEvent alCompletarCinematica;

        [Header("Animators (Opcional)")]
        [SerializeField] private Animator animJugadorManual;
        [SerializeField] private Animator animClinicoManual;

        private GameObject jugadorActivo;
        private Animator animJugador;
        private Animator animClinico;

        private void Start()
        {
            int avatarElegido = PlayerPrefs.GetInt("Avatar", 1);
            if (avatarElegido == 1)
            {
                if (hombre != null) hombre.SetActive(true);
                if (mujer != null) mujer.SetActive(false);
                jugadorActivo = hombre;
            }
            else
            {
                if (mujer != null) mujer.SetActive(true);
                if (hombre != null) hombre.SetActive(false);
                jugadorActivo = mujer;
            }

            if (animJugadorManual != null)
            {
                animJugador = animJugadorManual;
            }
            else if (jugadorActivo != null)
            {
                animJugador = jugadorActivo.GetComponent<Animator>();
                if (animJugador == null) animJugador = jugadorActivo.GetComponentInChildren<Animator>();
            }

            if (animJugador != null)
            {
                animJugador.Play("Happy Idle");
            }

            if (animClinicoManual != null)
            {
                animClinico = animClinicoManual;
            }
            else if (avatarClinico != null)
            {
                animClinico = avatarClinico.GetComponent<Animator>();
                if (animClinico == null) animClinico = avatarClinico.GetComponentInChildren<Animator>();
            }

            if (uiDialogosJuego != null) uiDialogosJuego.SetActive(false);

            // Asegurar estado de las cámaras al comenzar la cinemática
            if (camaraPrincipal != null) camaraPrincipal.gameObject.SetActive(true);
            if (camaraGameplay != null) camaraGameplay.SetActive(false);

            // IMPORTANTE: dejamos el GameObject ACTIVO siempre, solo controlamos alpha.
            // Si el objeto está SetActive(false), el fade nunca se ve porque el objeto
            // no se está renderizando ni sus coroutines corriendo dentro de él.
            if (titulo != null)
            {
                titulo.alpha = 0f;
                titulo.gameObject.SetActive(true);
            }
            if (texto != null)
            {
                texto.alpha = 0f;
                texto.gameObject.SetActive(true);
            }

            if (tituloTexto != null)
            {
                tituloTexto.text = mensajeTitulo;
            }

            // Un fundido blanco temporal suaviza la carga de la escena.
            StartCoroutine(FadeBlancoInicio());

            // Por ahora la cinemática siempre corre al entrar a la escena.
            // Cuando agregues los minijuegos, aquí es donde reintroducimos
            // la lógica de "si vengo de un minijuego, no repitas la cinemática".
            StartCoroutine(SecuenciaCinematica());
        }

        private IEnumerator FadeBlancoInicio()
        {
            Canvas canvasBase = null;
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas candidato in canvases)
            {
                if (candidato != null && candidato.isRootCanvas)
                {
                    canvasBase = candidato;
                    if (candidato.renderMode == RenderMode.ScreenSpaceOverlay) break;
                }
            }

            if (canvasBase == null) yield break;

            GameObject overlay = new GameObject("Fade Blanco Inicial", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(Image));
            overlay.transform.SetParent(canvasBase.transform, false);
            Canvas canvasOverlay = overlay.GetComponent<Canvas>();
            canvasOverlay.overrideSorting = true;
            canvasOverlay.sortingLayerID = canvasBase.sortingLayerID;
            canvasOverlay.sortingOrder = 32760;

            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Texture2D textura = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            textura.SetPixel(0, 0, Color.white);
            textura.Apply();
            Sprite sprite = Sprite.Create(textura, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            Image imagen = overlay.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = Color.white;
            imagen.raycastTarget = false;

            CanvasGroup grupo = overlay.GetComponent<CanvasGroup>();
            grupo.alpha = 1f;
            grupo.interactable = false;
            grupo.blocksRaycasts = false;

            const float duracion = 1.1f;
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime;
                grupo.alpha = 1f - Mathf.Clamp01(tiempo / duracion);
                yield return null;
            }

            Destroy(sprite);
            Destroy(textura);
            Destroy(overlay);
        }

        private IEnumerator SecuenciaCinematica()
        {
            if (animJugador != null) animJugador.Play("Happy Idle");

            // A. Posicionamiento inicial
            if (jugadorActivo != null && inicioJugador != null)
            {
                jugadorActivo.transform.position = inicioJugador.position;
                if (finJugador != null)
                {
                    Vector3 dirJugador = (finJugador.position - inicioJugador.position);
                    dirJugador.y = 0f;
                    jugadorActivo.transform.rotation = dirJugador.sqrMagnitude > 0.001f
                        ? Quaternion.LookRotation(dirJugador)
                        : inicioJugador.rotation;
                }
                else
                {
                    jugadorActivo.transform.rotation = inicioJugador.rotation;
                }
            }

            if (avatarClinico != null && inicioClinico != null)
            {
                avatarClinico.transform.position = inicioClinico.position;
                if (finClinico != null)
                {
                    Vector3 dirClinico = (finClinico.position - inicioClinico.position);
                    dirClinico.y = 0f;
                    avatarClinico.transform.rotation = dirClinico.sqrMagnitude > 0.001f
                        ? Quaternion.LookRotation(dirClinico)
                        : inicioClinico.rotation;
                }
                else
                {
                    avatarClinico.transform.rotation = inicioClinico.rotation;
                }
            }

            if (camaraPrincipal != null && camaraStartPoint != null)
            {
                camaraPrincipal.transform.position = camaraStartPoint.position;
                camaraPrincipal.transform.rotation = camaraStartPoint.rotation;
            }

            yield return new WaitForSeconds(0.4f);

            // B. Caminar
            EstablecerAnimacionCaminando(true);

            // Iniciar la presentación del título en paralelo (saldrá a los 3 segundos de caminata)
            StartCoroutine(SecuenciaTituloYTexto());

            bool camaraLlego = false;
            bool personajesLlegaron = false;

            while (!camaraLlego || !personajesLlegaron)
            {
                float deltaTime = Time.deltaTime;

                if (camaraPrincipal != null && camaraEndPoint != null)
                {
                    camaraPrincipal.transform.position = Vector3.MoveTowards(
                        camaraPrincipal.transform.position, camaraEndPoint.position, velocidadCamara * deltaTime);
                    camaraPrincipal.transform.rotation = Quaternion.Slerp(
                        camaraPrincipal.transform.rotation, camaraEndPoint.rotation, velocidadRotacionCamara * deltaTime);

                    if (Vector3.Distance(camaraPrincipal.transform.position, camaraEndPoint.position) < 0.05f)
                    {
                        camaraPrincipal.transform.position = camaraEndPoint.position;
                        camaraPrincipal.transform.rotation = camaraEndPoint.rotation;
                        camaraLlego = true;
                    }
                }
                else camaraLlego = true;

                bool jugadorLlego = false;
                bool clinicoLlego = false;

                if (jugadorActivo != null && finJugador != null)
                {
                    jugadorActivo.transform.position = Vector3.MoveTowards(
                        jugadorActivo.transform.position, finJugador.position, velocidadCaminata * deltaTime);

                    Vector3 dirJugador = (finJugador.position - jugadorActivo.transform.position);
                    dirJugador.y = 0f;
                    if (dirJugador.sqrMagnitude > 0.001f)
                    {
                        Quaternion rotObjetivo = Quaternion.LookRotation(dirJugador);
                        jugadorActivo.transform.rotation = Quaternion.Slerp(jugadorActivo.transform.rotation, rotObjetivo, 5f * deltaTime);
                    }

                    if (Vector3.Distance(jugadorActivo.transform.position, finJugador.position) < 0.05f)
                    {
                        jugadorActivo.transform.position = finJugador.position;
                        jugadorLlego = true;
                    }
                }
                else jugadorLlego = true;

                if (avatarClinico != null && finClinico != null)
                {
                    avatarClinico.transform.position = Vector3.MoveTowards(
                        avatarClinico.transform.position, finClinico.position, velocidadCaminata * deltaTime);

                    Vector3 dirClinico = (finClinico.position - avatarClinico.transform.position);
                    dirClinico.y = 0f;
                    if (dirClinico.sqrMagnitude > 0.001f)
                    {
                        Quaternion rotObjetivo = Quaternion.LookRotation(dirClinico);
                        avatarClinico.transform.rotation = Quaternion.Slerp(avatarClinico.transform.rotation, rotObjetivo, 5f * deltaTime);
                    }

                    if (Vector3.Distance(avatarClinico.transform.position, finClinico.position) < 0.05f)
                    {
                        avatarClinico.transform.position = finClinico.position;
                        clinicoLlego = true;
                    }
                }
                else clinicoLlego = true;

                if (jugadorLlego && clinicoLlego) personajesLlegaron = true;

                yield return null;
            }

            // Los personajes llegaron a su destino
            EstablecerAnimacionCaminando(false);

            if (jugadorActivo != null && finJugador != null) jugadorActivo.transform.rotation = finJugador.rotation;
            if (avatarClinico != null && finClinico != null) avatarClinico.transform.rotation = finClinico.rotation;

            // Desvanecer rápidamente el título y texto para limpiar la pantalla
            if (duracionFadeDesaparecer > 0.02f)
            {
                if (titulo != null) StartCoroutine(Fade(titulo, titulo.alpha, 0f, duracionFadeDesaparecer));
                if (texto != null) StartCoroutine(Fade(texto, texto.alpha, 0f, duracionFadeDesaparecer));
                yield return new WaitForSeconds(duracionFadeDesaparecer);
            }
            else
            {
                if (titulo != null) titulo.alpha = 0f;
                if (texto != null) texto.alpha = 0f;
            }

            if (titulo != null) titulo.gameObject.SetActive(false);
            if (texto != null) texto.gameObject.SetActive(false);

            if (camaraPrincipal != null) camaraPrincipal.gameObject.SetActive(false);
            if (camaraGameplay != null) camaraGameplay.SetActive(true);

            FinalizarSecuencia();
        }

        private IEnumerator SecuenciaTituloYTexto()
        {
            // 1. Espera 3 segundos mientras los personajes van caminando
            yield return new WaitForSeconds(delayAntesDeAparecer);

            // 2. Aparece el título con fade
            if (titulo != null) yield return StartCoroutine(Fade(titulo, 0f, 1f, duracionFade));

            // 3. Pausa breve para que el subtítulo salga más seguido
            yield return new WaitForSeconds(tiempoEntreTituloYTexto);

            // 4. Aparece el texto de abajo con fade
            if (texto != null) yield return StartCoroutine(Fade(texto, 0f, 1f, duracionFade));
        }

        private IEnumerator Fade(CanvasGroup cg, float start, float end, float duracion)
        {
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, end, t / duracion);
                yield return null;
            }
            cg.alpha = end;
        }

        private void EstablecerAnimacionCaminando(bool caminando)
        {
            if (animJugador != null)
            {
                animJugador.SetBool("Walking", caminando);
            }

            if (animClinico != null)
            {
                string clinicoAnimName = caminando ? "Walking" : "Fishing Idle";
                animClinico.CrossFade(clinicoAnimName, 0.15f);
            }
        }

        private void FinalizarSecuencia()
        {
            if (uiDialogosJuego != null) uiDialogosJuego.SetActive(true);
            alCompletarCinematica?.Invoke();
        }
    }
}