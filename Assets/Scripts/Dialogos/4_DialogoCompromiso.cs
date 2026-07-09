using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogoCompromiso : MonoBehaviour
{
    [Header("Animator")]
    public Animator avatarAnimator;

    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;

    [Header("Respuestas")]
    public GameObject panelRespuestas;
    public GameObject check3_1;
    public GameObject check3_2;

    [Header("Introducción Minijuego")]
    public GameObject panelIntroduccion;
    public Image panelFade;
    public float duracionFade = 1.5f;

    [Header("Typewriter")]
    public float velocidadEscritura = 0.03f;

    private string[] dialogos;
    private int indice = 0;

    private bool escribiendo = false;
    private string textoCompletoActual;
    private Coroutine coroutineActual;

    private int respuestaSeleccionada = 0;
    private bool mostrandoRespuesta = false;
    private bool mostrandoConvergencia = false; // ← NUEVO

    void Start()
    {
        panelDialogo.SetActive(false);
        panelRespuestas.SetActive(false);
        check3_1.SetActive(false);
        check3_2.SetActive(false);

        if (panelIntroduccion != null)
            panelIntroduccion.SetActive(false);

        if (panelFade != null)
            panelFade.gameObject.SetActive(false);
    }

    public void IniciarDialogo()
    {
        indice = 0;
        mostrandoRespuesta = false;
        mostrandoConvergencia = false;

        dialogos = new string[]
        {
            "Has llegado a la última puerta.",
            "Este lugar se llama Compromiso.",
            "Ahora ya no vienes como alguien que busca escapar...",
            "Vienes como quien decide recuperar el control de su camino.",
            "¿Qué quieres hacer ahora?"
        };

        panelDialogo.SetActive(true);
        MostrarDialogoActual();
    }

    void MostrarDialogoActual()
    {
        if (coroutineActual != null)
            StopCoroutine(coroutineActual);

        coroutineActual = StartCoroutine(EscribirTexto(dialogos[indice]));
    }

    IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        textoCompletoActual = texto;

        if (avatarAnimator != null)
            avatarAnimator.SetBool("Hablando", true);

        textoDialogo.text = "";

        foreach (char letra in texto)
        {
            if (!escribiendo)
            {
                textoDialogo.text = textoCompletoActual;

                if (avatarAnimator != null)
                    avatarAnimator.SetBool("Hablando", false);

                yield break;
            }

            textoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        escribiendo = false;

        if (avatarAnimator != null)
            avatarAnimator.SetBool("Hablando", false);
    }

    public void SiguienteDialogo()
    {
        if (escribiendo)
        {
            escribiendo = false;
            textoDialogo.text = textoCompletoActual;

            if (avatarAnimator != null)
                avatarAnimator.SetBool("Hablando", false);

            return;
        }

        indice++;

        if (indice < dialogos.Length)
        {
            MostrarDialogoActual();
            return;
        }

        // ← Terminó el diálogo de respuesta (opción 3 o 4): mostrar convergencia
        if (mostrandoRespuesta && !mostrandoConvergencia)
        {
            mostrandoConvergencia = true;
            indice = 0;

            dialogos = new string[]
            {
                "Mira todo el camino que has recorrido.",
                "Ahora es momento de poner en práctica lo aprendido.",
                "Haremos un último ejercicio juntos. 😊"
            };

            MostrarDialogoActual();
            return;
        }

        // ← Terminó el diálogo de convergencia: mostrar panel de introducción del minijuego
        if (mostrandoConvergencia)
        {
            panelDialogo.SetActive(false);

            if (panelIntroduccion != null)
                StartCoroutine(FadeInPanel(panelIntroduccion));

            return;
        }

        // ← Fin del diálogo inicial: mostrar panel de respuestas
        panelDialogo.SetActive(false);
        panelRespuestas.SetActive(true);
    }

    public void SeleccionarRespuesta(int opcion)
    {
        respuestaSeleccionada = opcion;

        check3_1.SetActive(opcion == 3);
        check3_2.SetActive(opcion == 4);
    }

    public void ConfirmarRespuesta()
    {
        panelRespuestas.SetActive(false);
        panelDialogo.SetActive(true);

        indice = 0;
        mostrandoRespuesta = true;
        mostrandoConvergencia = false;

        if (respuestaSeleccionada == 3)
        {
            dialogos = new string[]
            {
                "Has dado el primer paso del compromiso.",
                "En cada capítulo aprenderás una nueva herramienta. 😊",
            };
        }
        else
        {
            dialogos = new string[]
            {
                "El miedo puede acompañarte...",
                "Pero no tiene que decidir por ti. 😊"
            };
        }

        MostrarDialogoActual();
    }

    private IEnumerator FadeInPanel(GameObject panel)
    {
        panel.SetActive(true);

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        cg.alpha = 1f;
    }

    public void QuieroIntentarlo()
    {
        StartCoroutine(FadeYCargarMinijuego());
    }

    private IEnumerator FadeYCargarMinijuego()
    {
        if (panelIntroduccion != null)
        {
            CanvasGroup cg = panelIntroduccion.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelIntroduccion.AddComponent<CanvasGroup>();

            float t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                cg.alpha = 1f - Mathf.Clamp01(t / 0.8f);
                yield return null;
            }

            cg.alpha = 0f;
            panelIntroduccion.SetActive(false);
        }

        if (panelFade != null)
        {
            panelFade.gameObject.SetActive(true);
            Color color = panelFade.color;
            color.a = 0f;
            panelFade.color = color;

            float t = 0f;
            while (t < duracionFade)
            {
                t += Time.deltaTime;
                color.a = Mathf.Clamp01(t / duracionFade);
                panelFade.color = color;
                yield return null;
            }

            color.a = 1f;
            panelFade.color = color;
        }

        yield return new WaitForSeconds(0.2f);

        SceneManager.LoadScene("05_MinijuegoCompromiso");
    }
}