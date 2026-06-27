using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class NubeController : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuracion")]
    public GameObject corazonAsociado;
    public float velocidadLevitacion = 0.5f;
    public float alturaLevitacion = 8f;
    public float radioOrbita = 12f;
    public float offsetFase = 0f;

    private GameManager gameManager;
    private RectTransform rt;
    private Vector2 posicionInicial;
    private bool explotada = false;
    private int clicksNecesarios = 2;
    private int clicksActuales = 0;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        rt = GetComponent<RectTransform>();
        posicionInicial = rt.anchoredPosition;
        corazonAsociado.SetActive(false);
    }

    void Update()
{
    if (!explotada)
    {
        float offsetX = Mathf.Sin(Time.time * velocidadLevitacion + offsetFase) * radioOrbita;
        float offsetY = Mathf.Cos(Time.time * velocidadLevitacion * 0.7f + offsetFase) * alturaLevitacion;

        rt.anchoredPosition = new Vector2(
            posicionInicial.x + offsetX,
            posicionInicial.y + offsetY
        );
    }
}

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!explotada)
        {
            clicksActuales++;

            if (clicksActuales < clicksNecesarios)
            {
                StartCoroutine(AnimacionGolpe());
            }
            else
            {
                explotada = true;
                StartCoroutine(AnimacionExplosion());
            }
        }
    }

    IEnumerator AnimacionGolpe()
    {
        float duracion = 0.15f;
        float tiempo = 0f;
        Vector3 escalaActual = transform.localScale;
        Vector3 escalaReducida = escalaActual * 0.7f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaActual, escalaReducida, tiempo / duracion);
            yield return null;
        }

        transform.localScale = escalaReducida;
    }

    IEnumerator AnimacionExplosion()
    {
        float duracion = 0.3f;
        float tiempo = 0f;
        Vector3 escalaOriginal = transform.localScale;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            transform.localScale = Vector3.Lerp(escalaOriginal, Vector3.zero, t);
            yield return null;
        }

        gameManager.NubeExplotada(corazonAsociado);
        gameObject.SetActive(false);
    }
}