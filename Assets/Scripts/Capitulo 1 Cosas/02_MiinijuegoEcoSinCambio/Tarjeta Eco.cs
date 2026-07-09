using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class TarjetaEco : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Configuración")]
    public bool esLoAplico;
    public GameObject checkmark;

    [Header("Referencias")]
    public GameManagerEco gameManager;

    [Header("Palpitación")]
    public float palpitacionIntensidad = 0.02f;  // qué tanto crece (4%)
    public float palpitacionVelocidad  = 0.2f;   // latidos por segundo

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Transform parentOriginal;
    private int indiceOriginal;
    private Vector2 posicionOriginal;
    private Vector3 escalaOriginal;
    private bool colocadaCorrectamente = false;

    private Coroutine coroutinePalpitacion;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(true);

        parentOriginal = transform.parent;
        indiceOriginal = transform.GetSiblingIndex();
        posicionOriginal = rectTransform.anchoredPosition;
        escalaOriginal = rectTransform.localScale;

        if (checkmark != null)
            checkmark.SetActive(false);

        // Arranca la palpitación al iniciar
        coroutinePalpitacion = StartCoroutine(Palpitar());
    }

    IEnumerator Palpitar()
    {
        while (true)
        {
            // Seno que va de 0 a 1 para que el pulso sea suave
            float t = (Mathf.Sin(Time.time * palpitacionVelocidad * Mathf.PI * 2f) + 1f) * 0.5f;
            float factor = 1f + palpitacionIntensidad * t;
            rectTransform.localScale = escalaOriginal * factor;
            yield return null;
        }
    }

    void DetenerPalpitacion()
    {
        if (coroutinePalpitacion != null)
        {
            StopCoroutine(coroutinePalpitacion);
            coroutinePalpitacion = null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (colocadaCorrectamente) return;

        // Pausa palpitación y restaura escala al arrastrar
        DetenerPalpitacion();
        rectTransform.localScale = escalaOriginal;

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (colocadaCorrectamente) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (colocadaCorrectamente) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        bool enZonaEntiendo = RectTransformUtility.RectangleContainsScreenPoint(
            gameManager.zonaLoEntiendo,
            eventData.position,
            eventData.pressEventCamera);

        bool enZonaAplico = RectTransformUtility.RectangleContainsScreenPoint(
            gameManager.zonaLoAplico,
            eventData.position,
            eventData.pressEventCamera);

        bool correcta =
            (esLoAplico && enZonaAplico) ||
            (!esLoAplico && enZonaEntiendo);

        if (correcta)
        {
            colocadaCorrectamente = true;

            RectTransform slot = gameManager.ObtenerSlot(esLoAplico);

            if (slot != null)
            {
                transform.SetParent(slot, false);
                rectTransform.anchoredPosition = Vector2.zero;
                StartCoroutine(AnimarAMiniatura());
            }
            else
            {
                transform.SetParent(parentOriginal, false);
                transform.SetSiblingIndex(indiceOriginal);
                rectTransform.anchoredPosition = posicionOriginal;
            }

            if (checkmark != null)
                checkmark.SetActive(true);

            gameManager.TarjetaColocadaCorrectamente();
        }
        else
        {
            // Regresa a posición original y reanuda palpitación
            transform.SetParent(parentOriginal, false);
            transform.SetSiblingIndex(indiceOriginal);
            rectTransform.anchoredPosition = posicionOriginal;

            coroutinePalpitacion = StartCoroutine(Palpitar());
        }
    }

    IEnumerator AnimarAMiniatura()
    {
        float duracion = 0.3f;
        float t = 0f;
        Vector3 escalaDestino = escalaOriginal * 0.3f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(escalaOriginal, escalaDestino, t / duracion);
            yield return null;
        }

        rectTransform.localScale = escalaDestino;
    }
}