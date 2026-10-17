using UnityEngine;
using TMPro;
using System.Collections;

public class DialogoBienvenida : MonoBehaviour
{
[Header("Animator")]
public Animator avatarAnimator;
[Header("UI")]
public TMP_Text textoDialogo;

    public GameObject panelDialogo;
    // Added reference to decision panel to activate after dialogue ends
    public GameObject panelDecision;
public GameObject panelRespuestas;
public GameObject botonConfirmar;

[Header("Typewriter")]
public float velocidadEscritura = 0.03f;

    [Header("Mapa del recorrido (opcional)")]
    [Tooltip("CanvasGroup del panel de imagen de la nubecita. Debe estar separado del panel de dialogo y del Canvas general.")]
    public UnityEngine.CanvasGroup panelRecorrido;
    [Min(0f)] public float duracionFadeRecorrido = 0.4f;
    private Coroutine coroutineRecorrido;
    private bool recorridoVisible;

private string[] dialogos;
private int indice = 0;

private bool escribiendo = false;
private string textoCompletoActual;

    private Coroutine coroutineActual;
    private bool saludoInicialHecho = false;

    // Nuevas variables para manejar respuestas
    private string[] dialogosRespuesta;
    private int indiceRespuesta = 0;
    private bool mostrandoRespuesta = false;
    // Nuevas variables para la convergencia
    private bool mostrandoConvergencia = false;
    private string[] dialogosConvergencia;
    private int indiceConvergencia = 0;

void Start()
{
    OcultarRecorridoInmediato();

    // Si venimos del minijuego saltamos el dialogo de bienvenida
    if (PlayerPrefs.GetInt("EstadoRetirada", 0) == 1)
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);
        if (panelRespuestas != null) panelRespuestas.SetActive(false);
        if (botonConfirmar != null) botonConfirmar.SetActive(false);
        if (panelDecision != null) panelDecision.SetActive(false);
        return; // ← sale del Start sin iniciar nada
    }
    
    string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");

    dialogos = new string[]
    {
        "¡Hola, " + nombre + "! 😊 ¡Qué alegría verte por aquí!",
        "Yo soy tu Guía. ¡Estaré contigo durante todo el recorrido!",
        "Juntos descubriremos más sobre el dolor, paso a paso.",
        "Y dime... ¿Cómo llegas hoy aquí?"
    };

    if (panelRespuestas != null)
        panelRespuestas.SetActive(false);

    if (botonConfirmar != null)
        botonConfirmar.SetActive(false);

    // Inicializar texto del diálogo vacío antes de iniciar la secuencia
    textoDialogo.text = "";
    MostrarDialogoActual();
}

void MostrarDialogoActual()
{
    if (coroutineActual != null)
        StopCoroutine(coroutineActual);

    coroutineActual = StartCoroutine(EscribirTexto(dialogos[indice]));
}

public void SiguienteDialogo()
{
        if (escribiendo)
        {
            escribiendo = false;
            textoDialogo.text = textoCompletoActual;
            if (avatarAnimator != null)
            {
                avatarAnimator.SetBool("Hablando", false);
            }
            return;
        }

    // Si estamos mostrando una respuesta
    if (mostrandoRespuesta)
    {
        indiceRespuesta++;

        if (indiceRespuesta < dialogosRespuesta.Length)
        {
            MostrarDialogoRespuestaActual();
        }
        else
        {
            mostrandoRespuesta = false;

            string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");

            dialogosConvergencia = new string[]
            {
                "Hoy descubriremos tres puertas. ¡Mira el camino que nos espera!",
                "En la primera reconoceremos emociones y pensamientos que nos acompañan.",
                "En la segunda, conectaremos lo aprendido con pequeñas acciones.",
                "En la tercera, daremos un pequeño paso con la respiración.",
                "Cada paso nos dará pistas para entender mejor lo que sientes.",
                "¿Te animas a ver qué hay detrás? ¡Sígueme! 😊"
            };

            mostrandoConvergencia = true;
            indiceConvergencia = 0;

            MostrarConvergenciaActual();
        }

        return;
    }

    // Si estamos mostrando la convergencia
    if (mostrandoConvergencia)
    {
        indiceConvergencia++;

        if (indiceConvergencia < dialogosConvergencia.Length)
        {
            MostrarConvergenciaActual();
        }
        else
        {
            mostrandoConvergencia = false;

                // Cambiar mensaje final a algo más claro
                mostrandoConvergencia = false;
                panelDialogo.SetActive(false);
                if (panelDecision != null)
                {
                    panelDecision.SetActive(true);
                }
                Debug.Log("Fin de la introducción");
        }

        return;
    }

// Diálogos normales de introducción
indice++;

if (indice < dialogos.Length)
{
    MostrarDialogoActual();
}
        else
        {
            if (panelDialogo != null)
                panelDialogo.SetActive(false);

            // Activar solo el panel de respuestas aquí; el panel de decisión se muestra al final de la convergencia
            if (panelRespuestas != null)
                panelRespuestas.SetActive(true);
        }
}


    IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        textoCompletoActual = texto;

        // Primer diálogo: saludo
        if (avatarAnimator != null)
        {
            if (!saludoInicialHecho)
            {
                saludoInicialHecho = true;

                avatarAnimator.Play("Greeting");
                // Esperamos un poco antes de empezar a escribir
                yield return new WaitForSeconds(0.8f);
                avatarAnimator.SetBool("Hablando", true);
            }
            else
            {
                avatarAnimator.SetBool("Hablando", true);
            }
        }

        textoDialogo.text = "";

        foreach (char letra in texto)
        {
                if (!escribiendo)
                {
                    textoDialogo.text = textoCompletoActual;

                    if (avatarAnimator != null)
                    {
                        avatarAnimator.SetBool("Hablando", false);
                    }

                    yield break;
                }

            textoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        escribiendo = false;
        if (avatarAnimator != null)
        {
            avatarAnimator.SetBool("Hablando", false);
        }
    }

public void MostrarRespuestaElegida(int opcion)
{
    if (panelRespuestas != null)
        panelRespuestas.SetActive(false);

    if (botonConfirmar != null)
        botonConfirmar.SetActive(false);

    if (panelDialogo != null)
        panelDialogo.SetActive(true);

    mostrandoRespuesta = true;
    indiceRespuesta = 0;

    if (opcion == 1)
    {
        dialogosRespuesta = new string[]
        {
            "A veces basta con un pequeño paso. Démoslo juntos. 😊"
        };
    }
    else if (opcion == 2)
    {
        dialogosRespuesta = new string[]
        {
            "Es normal tener dudas. Podemos descubrir esto paso a paso. 😊"
        };
    }
    else if (opcion == 3)
    {
        dialogosRespuesta = new string[]
        {
            "¡Qué bueno que tengas curiosidad! Tenemos mucho por descubrir. 😊",
        };
    }

    MostrarDialogoRespuestaActual();
}

    // Método para mostrar el diálogo de respuesta actual
    void MostrarDialogoRespuestaActual()
    {
        if (coroutineActual != null)
            StopCoroutine(coroutineActual);

        coroutineActual = StartCoroutine(
            EscribirTexto(dialogosRespuesta[indiceRespuesta])
        );
    }

    // Nuevo método para mostrar los diálogos de convergencia
    void MostrarConvergenciaActual()
    {
        if (coroutineActual != null)
        StopCoroutine(coroutineActual);

        // Despues de la respuesta personalizada se explica el recorrido.
        // La ultima frase invita a caminar y ya deja ver el escenario.
        MostrarRecorrido(indiceConvergencia < dialogosConvergencia.Length - 1);

        coroutineActual = StartCoroutine(
            EscribirTexto(dialogosConvergencia[indiceConvergencia])
        );
    }

    bool PanelRecorridoValido()
    {
        // Evitar ocultar los dialogos si se asigna por error todo el Canvas.
        return panelRecorrido != null &&
            !transform.IsChildOf(panelRecorrido.transform) &&
            (textoDialogo == null || !textoDialogo.transform.IsChildOf(panelRecorrido.transform));
    }

    void MostrarRecorrido(bool visible)
    {
        if (!PanelRecorridoValido() || recorridoVisible == visible) return;

        if (coroutineRecorrido != null) StopCoroutine(coroutineRecorrido);
        recorridoVisible = visible;
        panelRecorrido.interactable = false;
        panelRecorrido.blocksRaycasts = false;
        panelRecorrido.gameObject.SetActive(true);
        coroutineRecorrido = StartCoroutine(FadeRecorrido(visible));
    }

    IEnumerator FadeRecorrido(bool visible)
    {
        float desde = panelRecorrido.alpha;
        float hasta = visible ? 1f : 0f;
        float t = 0f;
        while (t < duracionFadeRecorrido)
        {
            t += Time.deltaTime;
            panelRecorrido.alpha = Mathf.Lerp(desde, hasta, Mathf.Clamp01(t / duracionFadeRecorrido));
            yield return null;
        }

        panelRecorrido.alpha = hasta;
        if (!visible) panelRecorrido.gameObject.SetActive(false);
        coroutineRecorrido = null;
    }

    void OcultarRecorridoInmediato()
    {
        if (coroutineRecorrido != null) StopCoroutine(coroutineRecorrido);
        coroutineRecorrido = null;
        recorridoVisible = false;
        if (!PanelRecorridoValido()) return;

        panelRecorrido.alpha = 0f;
        panelRecorrido.interactable = false;
        panelRecorrido.blocksRaycasts = false;
        panelRecorrido.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        OcultarRecorridoInmediato();
    }

    // Added method to allow skipping the dialogue and showing the response panel
    public void SkipDialogo()
    {
        OcultarRecorridoInmediato();

        if (coroutineActual != null)
            StopCoroutine(coroutineActual);

        escribiendo = false;
        panelDialogo.SetActive(false);

        if (panelRespuestas != null)
            panelRespuestas.SetActive(true);
    }

    // Added method to skip everything and go directly to decision panel
  public void SkipTodo()
{
    OcultarRecorridoInmediato();

    if (coroutineActual != null)
        StopCoroutine(coroutineActual);

    escribiendo = false;
    mostrandoRespuesta = false;
    mostrandoConvergencia = false;

    if (panelDialogo != null) panelDialogo.SetActive(false);
    if (panelRespuestas != null) panelRespuestas.SetActive(false);
    if (botonConfirmar != null) botonConfirmar.SetActive(false);

    if (panelDecision != null)
    {
        panelDecision.SetActive(true);
        Debug.Log("panelDecision activado: " + panelDecision.name);
    }
    else
    {
        Debug.Log("ERROR: panelDecision es NULL");
    }
}
}
