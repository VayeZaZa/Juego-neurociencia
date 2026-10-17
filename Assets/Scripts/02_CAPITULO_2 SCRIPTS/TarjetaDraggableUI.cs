using UnityEngine;
using UnityEngine.EventSystems;

namespace Capitulo2
{
    /// <summary>
    /// Componente opcional para añadir a una tarjeta UI si deseas que se pueda arrastrar físicamente
    /// con el dedo o el ratón hacia la caja de Daño o la caja de Protección.
    /// </summary>
    [AddComponentMenu("Capitulo 2/Tarjeta Draggable UI")]
    public class TarjetaDraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Referencia a la EstacionPuenteCapitulo2")]
        [SerializeField] private EstacionPuenteCapitulo2 estacionPuente;

        [Tooltip("RectTransform de la caja de Daño Confirmado")]
        [SerializeField] private RectTransform rectCajaDano;

        [Tooltip("RectTransform de la caja de Protección Interpretativa")]
        [SerializeField] private RectTransform rectCajaProteccion;

        private RectTransform rectTransform;
        private Canvas canvasPadre;
        private Vector2 posicionInicialLocal;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasPadre = GetComponentInParent<Canvas>();
            if (rectTransform != null)
            {
                posicionInicialLocal = rectTransform.anchoredPosition;
            }

            if (estacionPuente == null)
            {
                estacionPuente = FindObjectOfType<EstacionPuenteCapitulo2>(true);
            }

            VincularCajasSiFaltan();
        }

        private void Start()
        {
            VincularCajasSiFaltan();
        }

        private void VincularCajasSiFaltan()
        {
            if (estacionPuente == null)
            {
                estacionPuente = FindObjectOfType<EstacionPuenteCapitulo2>(true);
            }
            if (estacionPuente != null)
            {
                if (rectCajaDano == null) rectCajaDano = estacionPuente.RectCajaDano;
                if (rectCajaProteccion == null) rectCajaProteccion = estacionPuente.RectCajaProteccion;
            }
        }

        private Coroutine coroutineRetorno;

        private void OnEnable()
        {
            ResetearPosicionInmediata();
        }

        public void ResetearPosicionInmediata()
        {
            if (coroutineRetorno != null) StopCoroutine(coroutineRetorno);
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = posicionInicialLocal;
            }
        }

        public void RegresarSuaveAPosicionInicial(float duracion = 0.25f)
        {
            if (coroutineRetorno != null) StopCoroutine(coroutineRetorno);
            if (gameObject.activeInHierarchy)
            {
                coroutineRetorno = StartCoroutine(AnimarRegreso(duracion));
            }
            else
            {
                ResetearPosicionInmediata();
            }
        }

        private System.Collections.IEnumerator AnimarRegreso(float duracion)
        {
            if (rectTransform == null) yield break;
            Vector2 posActual = rectTransform.anchoredPosition;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                float suave = Mathf.SmoothStep(0f, 1f, p);
                rectTransform.anchoredPosition = Vector2.Lerp(posActual, posicionInicialLocal, suave);
                yield return null;
            }
            rectTransform.anchoredPosition = posicionInicialLocal;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (coroutineRetorno != null) StopCoroutine(coroutineRetorno);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rectTransform == null || canvasPadre == null) return;
            rectTransform.anchoredPosition += eventData.delta / canvasPadre.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            VincularCajasSiFaltan();

            Camera cam = eventData.pressEventCamera;
            if (cam == null && canvasPadre != null && canvasPadre.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvasPadre.worldCamera;
            }

            bool soltadoEnDano = false;
            bool soltadoEnProteccion = false;

            if (rectCajaDano != null)
            {
                soltadoEnDano = RectTransformUtility.RectangleContainsScreenPoint(rectCajaDano, eventData.position, cam);
            }

            if (rectCajaProteccion != null)
            {
                soltadoEnProteccion = RectTransformUtility.RectangleContainsScreenPoint(rectCajaProteccion, eventData.position, cam);
            }

            if (soltadoEnDano && estacionPuente != null)
            {
                estacionPuente.SeleccionarCajaDano();
            }
            else if (soltadoEnProteccion && estacionPuente != null)
            {
                estacionPuente.SeleccionarCajaProteccion();
            }
            else
            {
                // Si no se soltó en ninguna caja válida, regresa suavemente a su lugar inicial
                RegresarSuaveAPosicionInicial(0.2f);
            }
        }
    }
}
