using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogoCompromiso : MonoBehaviour
{
    [Header("Cinemática Final")]
    public CinematicaFinal cinematicaFinal;

    [Header("Animator")]
    public Animator avatarAnimator;

    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;

    [Header("Archivo de preguntas")]
    public float velocidadTextoArchivo = 0.055f;
    public float duracionFadeArchivo = 0.5f;
    public GameObject panelArchivoPreguntas;
    public GameObject botonContinuarArchivo;
    public GameObject botonEntendido;
    public TMP_Text tituloArchivo;
    public TMP_Text textoArchivo;

    public TMP_Text pregunta1;
    public TMP_Text pregunta2;
    public TMP_Text pregunta3;

    public Button botonPregunta1;
    public Button botonPregunta2;
    public Button botonPregunta3;

    public GameObject checkPregunta1;
    public GameObject checkPregunta2;
    public GameObject checkPregunta3;

    public Button botonConfirmar;

    [Header("Recompensa Brújula")]
    public RecompensaCompromiso scriptRecompensa;

    [Header("Respuestas")]
    public GameObject panelRespuestas;
    public GameObject botonConfirmarRespuesta;   // 👈 NUEVO (distinto del botonConfirmar del archivo)
    public GameObject check3_1;
    public GameObject check3_2;

    [Header("Introducción Minijuego")]
    public GameObject panelIntroduccion;
    public Image panelFade;
    public float duracionFade = 1.5f;

    [Header("Typewriter")]
    public float velocidadEscritura = 0.03f;

    [Header("Fade Preguntas")]
    public float duracionFadePregunta = 0.4f;

    private string[] dialogos;
    private int indice = 0;

    private string textoIntroduccionArchivo =
    "Las tres puertas han sido superadas.\n\n" +
    "El Archivo de Preguntas ha quedado disponible.\n\n" +
    "Aquí encontrarás respuestas a las dudas más importantes antes de finalizar tu recorrido.\n\n";

    private bool escribiendo = false;
    private string textoCompletoActual;
    private Coroutine coroutineActual;

    private int respuestaSeleccionada = 0;
    private int preguntaSeleccionada = 0;
    private bool botonConfirmarMostrado = false;
    private bool mostrandoRespuesta = false;
    private bool mostrandoConvergencia = false;
    private bool dialogoPostMinijuego = false;
    private bool aplausoHecho = false;
    private bool dialogoDespedida = false;

    private string textoInstruccion =
    "Escoge únicamente una pregunta para responder.\n\n";

    void Start()
    {
        if (PlayerPrefs.GetInt("EstadoCompromiso", 0) == 1) return;

        panelDialogo.SetActive(false);
        panelRespuestas.SetActive(false);
        if (botonConfirmarRespuesta != null) botonConfirmarRespuesta.SetActive(false);   // 👈 NUEVO
        check3_1.SetActive(false);
        check3_2.SetActive(false);

        if (panelIntroduccion != null)
            panelIntroduccion.SetActive(false);

        if (panelFade != null)
            panelFade.gameObject.SetActive(false);

        if (panelArchivoPreguntas != null)
        {
            panelArchivoPreguntas.SetActive(false);
        }

        if (botonContinuarArchivo != null)
            botonContinuarArchivo.SetActive(false);

        if (botonEntendido != null)
            botonEntendido.SetActive(false);

        if (checkPregunta1 != null) checkPregunta1.SetActive(false);
        if (checkPregunta2 != null) checkPregunta2.SetActive(false);
        if (checkPregunta3 != null) checkPregunta3.SetActive(false);

        if (botonConfirmar != null) botonConfirmar.gameObject.SetActive(false);
        botonConfirmarMostrado = false;
    }

    public void IniciarDialogo()
    {
        indice = 0;
        mostrandoRespuesta = false;
        mostrandoConvergencia = false;
        dialogoPostMinijuego = false;
        dialogoDespedida = false;

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

    public void DialogoDespuesDelMinijuego()
    {
        indice = 0;
        mostrandoRespuesta = true;
        mostrandoConvergencia = true;
        dialogoPostMinijuego = true;
        aplausoHecho = false;
        dialogoDespedida = false;

        dialogos = new string[]
        {
            "¡¡¡Muy bien!!! ¡Has desbloqueado todas las puertas! 😊",
            "Tu compromiso ha desbloqueado el Archivo de Preguntas.",
            "Revísalo y descubrirás la recompensa que cierra este capítulo. 😊"
        };

        panelDialogo.SetActive(true);
        MostrarDialogoActual();
    }

    public void DialogoDespedidaFinal()
    {
        indice = 0;
        dialogoDespedida = true;

        string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");

        dialogos = new string[]
        {
            "¡Felicidades por completar el capítulo \"El Umbral de la Decisión\"!",

            "Esta brújula simboliza el compromiso que has construido. Guárdala como un recordatorio de tu fortaleza. 😊",

            "Siempre recuerda seguir practicando los ejercicios indicados por los profesionales.",

            "¡Gracias por recorrer este capítulo conmigo! ¡Ha sido un gusto, " + nombre + " 😊 !",
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

        if (dialogoDespedida)
        {
            if (indice < dialogos.Length)
            {
                MostrarDialogoActual();
            }
            else
            {
                panelDialogo.SetActive(false);
                if (cinematicaFinal != null)
                {
                    Debug.Log("DialogoCompromiso: Iniciando la cinematica final...");
                    cinematicaFinal.Iniciar();
                }
                else
                {
                    Debug.LogWarning("DialogoCompromiso: ¡No se encontró el script CinematicaFinal en la escena!");
                }
            }
            return;
        }

        if (dialogoPostMinijuego)
        {
            if (indice < dialogos.Length)
            {
                MostrarDialogoActual();
            }
            else
            {
                panelDialogo.SetActive(false);
                if (panelArchivoPreguntas != null)
                    StartCoroutine(MostrarArchivoPreguntas());
            }
            return;
        }

        if (indice < dialogos.Length)
        {
            MostrarDialogoActual();
            return;
        }

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

        if (mostrandoConvergencia)
        {
            panelDialogo.SetActive(false);

            if (panelIntroduccion != null)
                StartCoroutine(FadeInPanel(panelIntroduccion));

            return;
        }

        panelDialogo.SetActive(false);
        panelRespuestas.SetActive(true);
        if (botonConfirmarRespuesta != null)
            botonConfirmarRespuesta.SetActive(false);   // 👈 NUEVO
    }

    public void SeleccionarRespuesta(int opcion)
    {
        respuestaSeleccionada = opcion;

        check3_1.SetActive(opcion == 3);
        check3_2.SetActive(opcion == 4);

        if (botonConfirmarRespuesta != null)
            botonConfirmarRespuesta.SetActive(true);   // 👈 NUEVO
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

    IEnumerator MostrarArchivoPreguntas()
    {
        panelArchivoPreguntas.SetActive(true);

        CanvasGroup cg = panelArchivoPreguntas.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelArchivoPreguntas.AddComponent<CanvasGroup>();

        cg.alpha = 0;

        string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");
        tituloArchivo.text = "Archivo de preguntas de " + nombre;

        textoArchivo.text = "";

        float t = 0;
        while (t < duracionFadeArchivo)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / duracionFadeArchivo);
            yield return null;
        }

        cg.alpha = 1;

        yield return new WaitForSeconds(0.5f);

        foreach (char letra in textoIntroduccionArchivo)
        {
            textoArchivo.text += letra;
            yield return new WaitForSeconds(velocidadTextoArchivo);
        }

        yield return new WaitForSeconds(0.3f);

        botonContinuarArchivo.SetActive(true);

        CanvasGroup cgBoton = botonContinuarArchivo.GetComponent<CanvasGroup>();
        if (cgBoton == null) cgBoton = botonContinuarArchivo.AddComponent<CanvasGroup>();

        cgBoton.alpha = 0;

        float tiempo = 0;
        while (tiempo < 0.45f)
        {
            tiempo += Time.deltaTime;
            cgBoton.alpha = Mathf.Lerp(0, 1, tiempo / 0.45f);
            yield return null;
        }

        cgBoton.alpha = 1;
    }

    public void ContinuarArchivo()
    {
        botonContinuarArchivo.SetActive(false);
        StartCoroutine(MostrarInstruccionYPreguntas());
    }

    IEnumerator MostrarInstruccionYPreguntas()
    {
        textoArchivo.text = "";

        foreach (char letra in textoInstruccion)
        {
            textoArchivo.text += letra;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(MostrarPreguntas());
    }

    IEnumerator MostrarPreguntas()
    {
        pregunta1.text = "¿Qué me está ocurriendo?";
        pregunta2.text = "¿Cómo puede ayudarme esto?";
        pregunta3.text = "¿Qué hago ahora?";

        botonPregunta1.gameObject.SetActive(false);
        botonPregunta2.gameObject.SetActive(false);
        botonPregunta3.gameObject.SetActive(false);

        checkPregunta1.SetActive(false);
        checkPregunta2.SetActive(false);
        checkPregunta3.SetActive(false);

        botonConfirmar.gameObject.SetActive(false);
        botonConfirmarMostrado = false;

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(FadeInObjeto(botonPregunta1.gameObject));
        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(FadeInObjeto(botonPregunta2.gameObject));
        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(FadeInObjeto(botonPregunta3.gameObject));
    }

    IEnumerator FadeInObjeto(GameObject objeto)
    {
        objeto.SetActive(true);

        CanvasGroup cg = objeto.GetComponent<CanvasGroup>();
        if (cg == null) cg = objeto.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float t = 0f;

        while (t < duracionFadePregunta)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / duracionFadePregunta);
            yield return null;
        }

        cg.alpha = 1f;
    }

    public void SeleccionarPregunta(int numero)
    {
        preguntaSeleccionada = numero;

        checkPregunta1.SetActive(numero == 1);
        checkPregunta2.SetActive(numero == 2);
        checkPregunta3.SetActive(numero == 3);

        if (!botonConfirmarMostrado)
        {
            botonConfirmarMostrado = true;
            StartCoroutine(MostrarBotonConfirmar());
        }
    }

    IEnumerator MostrarBotonConfirmar()
    {
        CanvasGroup cg = botonConfirmar.GetComponent<CanvasGroup>();
        if (cg == null) cg = botonConfirmar.gameObject.AddComponent<CanvasGroup>();

        botonConfirmar.gameObject.SetActive(true);
        cg.alpha = 0;

        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            cg.alpha = t / 0.4f;
            yield return null;
        }

        cg.alpha = 1;
    }

    // --- RESPUESTA CON NEUROCIENCIA DEL DOLOR ---
    public void ConfirmarPreguntaElegida()
    {
        StartCoroutine(MostrarRespuestaEnArchivo());
    }

    IEnumerator MostrarRespuestaEnArchivo()
    {
        botonPregunta1.gameObject.SetActive(false);
        botonPregunta2.gameObject.SetActive(false);
        botonPregunta3.gameObject.SetActive(false);

        checkPregunta1.SetActive(false);
        checkPregunta2.SetActive(false);
        checkPregunta3.SetActive(false);

        botonConfirmar.gameObject.SetActive(false);

        string textoRespuesta = "";

        switch (preguntaSeleccionada)
        {
            case 1:
                textoRespuesta =
                    "¿Qué me está ocurriendo?\n\n" +
                    "El dolor es una señal de protección.\n\n" +
                    "No siempre significa que exista un daño nuevo.\n\n" +
                    "• Antes de preocuparte, pregúntate qué pudo activar esa alarma.";
                break;

            case 2:
                textoRespuesta =
                    "¿Cómo puede ayudarme esto?\n\n" +
                    "Comprender el dolor disminuye el miedo.\n\n" +
                    "Con menos miedo, el cerebro deja de mantenerse tan alerta.\n\n" +
                    "• Evita pensar que todo dolor significa peligro.";
                break;

            case 3:
                textoRespuesta =
                    "¿Qué hago ahora?\n\n" +
                    "Haz ejercicios simples poco a poco. \n\n" +
                    "Cada experiencia segura ayuda al cerebro a confiar otra vez.\n\n" +
                    "• Avanza gradualmente, sin exigir perfección.";
                break;
        }

        textoArchivo.text = "";

        foreach (char letra in textoRespuesta)
        {
            textoArchivo.text += letra;
            yield return new WaitForSeconds(velocidadTextoArchivo);
        }

        if (botonEntendido != null)
        {
            yield return StartCoroutine(FadeInObjeto(botonEntendido));
        }
    }

    public void EntendidoRespuesta()
    {
        if (panelArchivoPreguntas != null)
        {
            StartCoroutine(FadeOutPanelArchivo());
        }
    }

    private IEnumerator FadeOutPanelArchivo()
    {
        CanvasGroup cg = panelArchivoPreguntas.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelArchivoPreguntas.AddComponent<CanvasGroup>();

        float t = 0f;
        while (t < duracionFadeArchivo)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / duracionFadeArchivo);
            yield return null;
        }

        cg.alpha = 0f;
        panelArchivoPreguntas.SetActive(false);

        // Dispara la animación de la brújula justo al ocultar el panel
        if (scriptRecompensa != null)
        {
            scriptRecompensa.MostrarRecompensa();
        }
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