using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class MovimientoRespiracion : MonoBehaviour
{
    RectTransform rect;
    CanvasGroup canvasGroup;

    [Header("Entrada en escena")]
    [Tooltip("Distancia en píxeles desde donde aparece (a la derecha) hasta su posición final. Ej: 400 = empieza 400px a la derecha de donde lo pusiste en el editor")]
    public float distanciaEntrada = 400f;
    public float tiempoEntrada = 1.5f;

    [Header("Movimiento vertical (respiración)")]
    [Tooltip("Cuánto sube en píxeles. Ajusta según el tamaño de tu Canvas (ej: 150-300 para un Canvas 1920x1080)")]
    public float alturaMaxima = 200f;
    [Tooltip("Desvío lateral en píxeles al SUBIR. Déjalo en 0 si se ve como que retrocede")]
    public float curvaLateralSubida = 0f;
    [Tooltip("Desvío lateral en píxeles al BAJAR (esta es la que ya te gustó)")]
    public float curvaLateralBajada = 40f;

    Vector2 posBase;
    Vector2 posArriba;

    [Header("Movimiento Idle")]
    public float amplitudIdleX = 2f;
    public float amplitudIdleY = 4f;
    public float velocidadIdle = 1.5f;

    Coroutine rutinaIdle;
    bool idleActivo = false;
    Vector2 centroIdle;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        posBase = rect.anchoredPosition;
        posArriba = posBase + new Vector2(0, alturaMaxima);

        // Oculto al iniciar
        if (canvasGroup != null)
            canvasGroup.alpha = 0;
    }

    /// <summary>
    /// El personaje "aparece" invisible a la derecha (fuera de vista) y se desliza
    /// hacia la izquierda con un pop de escala, hasta llegar a la posición donde
    /// lo dejaste ubicado en el editor (posBase, calculada en Awake).
    /// </summary>
    public IEnumerator EntrarEscena()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1;

        Vector2 destino = posBase; // la posición donde lo pusiste en el Editor
        Vector2 inicio = destino + new Vector2(distanciaEntrada, 0f); // aparece a la derecha

        rect.anchoredPosition = inicio;

        Vector3 escalaFinal = rect.localScale;
        rect.localScale = Vector3.zero;

        float t = 0f;
        while (t < tiempoEntrada)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / tiempoEntrada);

            rect.anchoredPosition = Vector2.Lerp(inicio, destino, p);
            rect.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, p);

            yield return null;
        }

        rect.anchoredPosition = destino;
        rect.localScale = escalaFinal;
    }

    /// <summary>
    /// Sube en forma de parábola durante "duracion" segundos.
    /// Solo avanza el tiempo mientras "sigaPresionado" devuelva true (se pausa si sueltas).
    /// </summary>
    public IEnumerator Subir(float duracion, System.Func<bool> sigaPresionado, System.Action<float> onProgreso = null)
    {
        float acumulado = 0f;

        while (acumulado < duracion)
        {
            if (sigaPresionado())
            {
                acumulado += Time.deltaTime;
                AplicarPosicionArco(posBase, posArriba, acumulado / duracion, curvaLateralSubida);
                onProgreso?.Invoke(Mathf.Clamp01(acumulado / duracion));
            }
            yield return null;
        }

        rect.anchoredPosition = posArriba;
        onProgreso?.Invoke(1f);
    }

    public IEnumerator Bajar(float duracion, System.Action<float> onProgreso = null)
    {
        Vector2 inicio = rect.anchoredPosition;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            AplicarPosicionArco(inicio, posBase, t / duracion, curvaLateralBajada);
            onProgreso?.Invoke(Mathf.Clamp01(t / duracion));
            yield return null;
        }

        rect.anchoredPosition = posBase;
        onProgreso?.Invoke(1f);
    }

    void AplicarPosicionArco(Vector2 desde, Vector2 hacia, float p, float curvaLateral)
    {
        p = Mathf.Clamp01(p);

        // Suavizado tipo parábola de proyectil: rápido al inicio, lento al final
        float pSuave = 2f * p - p * p;

        Vector2 pos = Vector2.Lerp(desde, hacia, pSuave);

        // Arco lateral para que la trayectoria sea una parábola real, no una línea recta
        pos.x += Mathf.Sin(p * Mathf.PI) * curvaLateral;

        rect.anchoredPosition = pos;
    }

    public Vector2 PosicionBase => posBase;

    public void EmpezarIdle()
    {
        if (rutinaIdle != null)
            StopCoroutine(rutinaIdle);

        // Guarda la posición exacta donde terminó la subida
        centroIdle = rect.anchoredPosition;

        rutinaIdle = StartCoroutine(IdleRespirando());
    }

    public void DetenerIdle()
    {
        idleActivo = false;

        if (rutinaIdle != null)
            StopCoroutine(rutinaIdle);

        // NO mover la posición
    }

    IEnumerator IdleRespirando()
    {
        idleActivo = true;

        float tiempo = 0;

        while (idleActivo)
        {
            tiempo += Time.deltaTime;

            Vector2 pos = centroIdle;

            pos.y += Mathf.Sin(tiempo * velocidadIdle) * amplitudIdleY;
            pos.x += Mathf.Sin(tiempo * velocidadIdle * 0.4f) * amplitudIdleX;

            rect.anchoredPosition = pos;

            yield return null;
        }
    }
}
