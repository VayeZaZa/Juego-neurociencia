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
    string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");

    dialogos = new string[]
    {
        "¡Hola, " + nombre + "! 😊",
        "Me alegra mucho que estés aquí.",
        "Soy tu Avatar Clínico.",
        "Y durante este recorrido estaré contigo para guiarte paso a paso.",
        "Este es un espacio para comprender mejor lo que ocurre cuando vivimos con dolor.",
        "Estás aquí para explorar y comprender a tu propio ritmo.",
        "Antes de comenzar...",
        "¡Quiero hacerte una pregunta! Prometo que será rápida 😊",
        "¿Cómo llegas hoy aquí?"
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
                "Gracias por compartirlo conmigo, " + nombre + ".",
                "Cada persona vive el dolor de una manera diferente.",
                "Ahora demos el primer paso juntos.",
                "Cuando quieras... ¡Sígueme! 😊"
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
            "Gracias por tu honestidad.",
            "A veces basta con estar dispuestos a dar un pequeño paso. 😄"
        };
    }
    else if (opcion == 2)
    {
        dialogosRespuesta = new string[]
        {
            "Es normal tener dudas.",
            "Muchas personas llegan aquí sintiéndose igual.",
            "Solo te voy a pedir que me acompañes un momento y exploremos juntos. 😄"
        };
    }
    else if (opcion == 3)
    {
        dialogosRespuesta = new string[]
        {
            "Esa curiosidad puede convertirse en una herramienta muy valiosa.",
            "Entender el dolor no lo hace desaparecer de inmediato.",
            "Pero SÍ puede cambiar la forma en que lo vives. 😄"
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

        coroutineActual = StartCoroutine(
            EscribirTexto(dialogosConvergencia[indiceConvergencia])
        );
    }
}
