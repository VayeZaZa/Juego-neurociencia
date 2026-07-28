using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogoEcoCambio : MonoBehaviour
{
    [Header("Animator")]
    public Animator avatarAnimator;

    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;
    public GameObject panelRespuestas;
    public GameObject botonConfirmar;   // 👈 NUEVO
    public GameObject check2_1;
    public GameObject check2_2;

    [Header("Introducción Minijuego")]
    public GameObject panelIntroduccion;
    public Image panelFade;
    public float duracionFade = 1.5f;

    [Header("Panel Decisión Post-Minijuego")]
    public GameObject panelDecision2;
    [Header("Movimiento")]
    public MovimientoCompromiso movimientoCompromiso;
    [Header("Typewriter")]
    public float velocidadEscritura = 0.03f;

    private string[] dialogos;
    private int indice = 0;
    private bool escribiendo = false;
    private string textoCompletoActual;
    private Coroutine coroutineActual;
    private int respuestaSeleccionada = 0;
    private bool mostrandoRespuesta = false;
    private bool introduccionMinijuego = false;
    private bool dialogoPostMinijuego = false;
    private bool aplausoHecho;

    void Start()
    {
        dialogos = new string[]
        {
            "Este lugar se llama Eco sin Cambio.",
            "Aquí muchas personas asienten con la cabeza.",
            "Entienden muchas cosas... pero no dan el siguiente paso.",
            "Una idea solo cobra vida cuando cambia algo en ti.",
            "¿Qué piensas cuando escuchas eso?"
        };

        panelDialogo.SetActive(false);
        panelRespuestas.SetActive(false);
        if (botonConfirmar != null) botonConfirmar.SetActive(false);   // 👈 NUEVO
        check2_1.SetActive(false);
        check2_2.SetActive(false);

        if (panelIntroduccion != null)
            panelIntroduccion.SetActive(false);

        if (panelFade != null)
            panelFade.gameObject.SetActive(false);

        if (panelDecision2 != null)
            panelDecision2.SetActive(false);
    }

    public void IniciarDialogo()
    {
        // ← Si ya estamos en modo post-minijuego, ignorar esta llamada
        if (dialogoPostMinijuego) return;

        indice = 0;
        mostrandoRespuesta = false;
        introduccionMinijuego = false;

        dialogos = new string[]
        {
            "Este lugar se llama Eco sin Cambio.",
            "Aquí muchas personas asienten con la cabeza.",
            "Entienden muchas cosas... pero no dan el siguiente paso.",
            "Una idea solo cobra vida cuando cambia algo en ti.",
            "¿Qué piensas cuando escuchas eso?"
        };

        panelDialogo.SetActive(true);
        MostrarDialogoActual();
    }

    public void DialogoDespuesDelMinijuego()
    {
        indice = 0;
        mostrandoRespuesta = true;
        introduccionMinijuego = true;
        dialogoPostMinijuego = true;
        aplausoHecho = false;

        string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");

        dialogos = new string[]
        {
            "¡Lo conseguiste, " + nombre + "! 😊",
            "Ahora estas ideas ya no son solamente palabras.",
            "Las llevaste a la práctica.",
            "Y eso hace que este lugar deje de ser un eco...",
            "Para convertirse en un verdadero cambio.",
            "¡Aún queda una puerta más por descubrir! 😊"
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
        {
            if (dialogoPostMinijuego && !aplausoHecho)
            {
                aplausoHecho = true;
                avatarAnimator.CrossFade("Clapping", 0.08f);
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

        // ← Fin del diálogo post-minijuego: muestra panelDecision2
        if (dialogoPostMinijuego)
        {
            panelDialogo.SetActive(false);

            if (panelDecision2 != null)
                panelDecision2.SetActive(true);

            // Reset the flag so that future dialogs are not considered post‑minijuego
            dialogoPostMinijuego = false;

            return;
        }

        if (!mostrandoRespuesta)
        {
            panelDialogo.SetActive(false);
            panelRespuestas.SetActive(true);
            if (botonConfirmar != null)
                botonConfirmar.SetActive(false);   // 👈 NUEVO
        }
        else if (!introduccionMinijuego)
        {
            introduccionMinijuego = true;
            indice = 0;

            dialogos = new string[]
            {
                "A veces la diferencia entre comprender y aplicar es muy pequeña.",
                "Vamos a descubrirla juntos. 😊",
                "Esta puerta te presentará diferentes situaciones.",
                "En cada una, solo tendrás que decidir si representan 'Lo entiendo'... o 'Lo aplico'."
            };

            MostrarDialogoActual();
        }
        else
        {
            panelDialogo.SetActive(false);

            if (panelIntroduccion != null)
                StartCoroutine(FadeInPanel(panelIntroduccion));
        }
    }

    public void ContinuarRecorrido()
    {
        // Ensure the post‑minijuego flag is cleared when the player continues
        dialogoPostMinijuego = false;

        if (panelDecision2 != null)
            panelDecision2.SetActive(false);

        if (movimientoCompromiso != null)
            movimientoCompromiso.IniciarRecorrido();
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

        PlayerPrefs.SetInt("EstadoEcoCambio", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("04_MinijuegoEcoCambio");
    }

    //==================================
    // RESPUESTAS
    //==================================

    public void SeleccionarRespuesta(int opcion)
    {
        respuestaSeleccionada = opcion;
        check2_1.SetActive(opcion == 1);
        check2_2.SetActive(opcion == 2);

        if (botonConfirmar != null)
            botonConfirmar.SetActive(true);   // 👈 NUEVO
    }

    public void ConfirmarRespuesta()
    {
        panelRespuestas.SetActive(false);
        panelDialogo.SetActive(true);

        indice = 0;
        mostrandoRespuesta = true;

        if (respuestaSeleccionada == 1)
        {
            dialogos = new string[]
            {
                "Tiene sentido, ¿verdad? 😊",
                "Comprender algo ya es un paso importante.",
                "Pero cuando nos quedamos únicamente con la idea...",
                "El lugar permanece exactamente igual.",
            };
        }
        else
        {
            dialogos = new string[]
            {
                "Esa decisión cambia muchas cosas. 😊",
                "Cada vez que intentamos aplicar lo que aprendemos...",
                "El conocimiento nos acompaña en el camino.",
            };
        }

        MostrarDialogoActual();
    }
}