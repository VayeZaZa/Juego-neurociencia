using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogoRetirada : MonoBehaviour
{
    [Header("Animator")]
    public Animator avatarAnimator;

    [Header("UI")]
    public TMP_Text textoDialogo;
    public GameObject panelDialogo;
    public GameObject panelRespuestas;
    public GameObject check1;
    public GameObject check2;
    public GameObject check3;
    public GameObject check4;
    public GameObject panelRespuestas2;
    public GameObject check1_2;
    public GameObject check2_2;
    public GameObject check3_2;

    [Header("Introduccion Minijuego")]
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
    private bool segundaFase = false;
    private bool finalStage = false;
    private bool postMinijuego = false; // ← NUEVO
    private bool aplausoHecho = false; // ← APLAUDIR DESPUES DE MINIJUEGO
    private int respuestaSeleccionada2 = 0;

    void Start()
    {
        dialogos = new string[]
        {
            "Esta es la Puerta de la Retirada.",
            "No es debilidad llegar hasta aquí... Muchas veces es cansancio.",
            "Cansancio de buscar respuestas.",
            "De cargar con algo que no parece terminar.",
            "Pero detrás de ese cansancio...",
            "Suele haber algo más.",
            "Una emoción que ha estado presente durante el camino.",
            "¿Cuál sientes que ha sido la emoción más presente?"
        };

        if (panelDialogo != null) panelDialogo.SetActive(false);
        if (panelRespuestas != null) panelRespuestas.SetActive(false);
        if (panelRespuestas2 != null) panelRespuestas2.SetActive(false);
        if (panelIntroduccion != null) panelIntroduccion.SetActive(false);
        if (panelFade != null) panelFade.gameObject.SetActive(false);
    }

    public void IniciarDialogo()
    {
        indice = 0;
        panelDialogo.SetActive(true);
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
                avatarAnimator.SetBool("Hablando", false);

            return;
        }

        indice++;

        if (indice < dialogos.Length)
        {
            MostrarDialogoActual();
        }
        else
        {
            if (postMinijuego)
            {
                // Aquí termina el dialogo post minijuego
                panelDialogo.SetActive(false);
                PlayerPrefs.DeleteKey("EstadoRetirada");
                PlayerPrefs.Save();
                // Aquí puedes cargar la siguiente escena o lo que necesites
            }
            else if (!segundaFase && !finalStage)
            {
                panelDialogo.SetActive(false);
                if (panelRespuestas != null)
                    panelRespuestas.SetActive(true);
            }
            else if (segundaFase && !finalStage)
            {
                segundaFase = false;
                panelDialogo.SetActive(false);

                if (check1_2 != null) check1_2.SetActive(false);
                if (check2_2 != null) check2_2.SetActive(false);
                if (check3_2 != null) check3_2.SetActive(false);
                respuestaSeleccionada2 = 0;

                if (panelRespuestas2 != null)
                    panelRespuestas2.SetActive(true);
            }
            else if (finalStage)
            {
                panelDialogo.SetActive(false);
                if (panelIntroduccion != null)
                    StartCoroutine(FadeInPanel(panelIntroduccion));
            }
        }
    }

    private IEnumerator FadeInPanel(GameObject panel)
    {
        panel.SetActive(true);

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float t = 0f;
        float duracion = 1f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / duracion);
            yield return null;
        }

        cg.alpha = 1f;
    }

    IEnumerator EscribirTexto(string texto)
{
    escribiendo = true;
    textoCompletoActual = texto;

    if (avatarAnimator != null)
    {
        // 👏 Solo la primera vez después del minijuego
        if (postMinijuego && !aplausoHecho)
        {
            aplausoHecho = true;

            avatarAnimator.Play("Clapping");

            // Dejamos que aplauda un poquito
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
    public void SeleccionarRespuesta(int opcion)
    {
        respuestaSeleccionada = opcion;

        if (check1 != null) check1.SetActive(false);
        if (check2 != null) check2.SetActive(false);
        if (check3 != null) check3.SetActive(false);
        if (check4 != null) check4.SetActive(false);

        if (opcion == 1 && check1 != null) check1.SetActive(true);
        if (opcion == 2 && check2 != null) check2.SetActive(true);
        if (opcion == 3 && check3 != null) check3.SetActive(true);
        if (opcion == 4 && check4 != null) check4.SetActive(true);
    }

    public void ConfirmarRespuesta()
    {
        if (panelRespuestas != null)
            panelRespuestas.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        indice = 0;

        string nombre = PlayerPrefs.GetString("NombreJugador", "Explorador");

        dialogos = new string[]
        {
            "Lo que sentimos tiene sentido, " + nombre + "...",
            "Tomar un descanso también es válido.",
            "¿Ha habido momentos en que necesitaste tomar distancia?"
        };

        segundaFase = true;
        MostrarDialogoActual();
    }

    public void SeleccionarRespuesta2(int opcion)
    {
        respuestaSeleccionada2 = opcion;

        if (check1_2 != null) check1_2.SetActive(false);
        if (check2_2 != null) check2_2.SetActive(false);
        if (check3_2 != null) check3_2.SetActive(false);

        if (opcion == 1 && check1_2 != null) check1_2.SetActive(true);
        if (opcion == 2 && check2_2 != null) check2_2.SetActive(true);
        if (opcion == 3 && check3_2 != null) check3_2.SetActive(true);
    }

    public void ConfirmarRespuesta2()
    {
        if (panelRespuestas2 != null)
            panelRespuestas2.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        indice = 0;

        string[] respuestaPersonalizada;

        if (respuestaSeleccionada2 == 1)
        {
            respuestaPersonalizada = new string[]
            {
                "Recuerda que no estás pasando por esto sin compañía.",
                "Y a pesar de todo, sigues adelante. 😊"
            };
        }
        else if (respuestaSeleccionada2 == 2)
        {
            respuestaPersonalizada = new string[]
            {
                "No siempre es fácil responder algo así.",
                "Lo importante es que hoy decidiste dar el paso. 😊"
            };
        }
        else
        {
            respuestaPersonalizada = new string[]
            {
                "Esa fortaleza también tiene su historia detrás.",
                "Y merece ser reconocida. 😊"
            };
        }

        dialogos = new string[]
        {
            respuestaPersonalizada[0],
            respuestaPersonalizada[1],
            "Significa mucho que estés aquí.",
            "No voy a prometerte magia... Pero quiero mostrarte algo distinto.",
            "Detrás de esta puerta encontraremos pensamientos.",
            "Los que aparecen justo cuando queremos retirarnos.",
            "No vamos a luchar contra ellos.",
            "Vamos a comprenderlos.",
            "¡Adelante! 😊"
        };

        finalStage = true;
        MostrarDialogoActual();
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
            if (cg == null)
                cg = panelIntroduccion.AddComponent<CanvasGroup>();

            float t = 0f;
            float duracion = 0.8f;

            while (t < duracion)
            {
                t += Time.deltaTime;
                cg.alpha = 1f - Mathf.Clamp01(t / duracion);
                yield return null;
            }

            cg.alpha = 0f;
            panelIntroduccion.SetActive(false);
        }

        if (panelFade != null)
        {
            panelFade.gameObject.SetActive(true);

            float t = 0f;
            Color color = panelFade.color;
            color.a = 0f;
            panelFade.color = color;

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

        // Guarda que el minijuego fue completado ← IMPORTANTE
        PlayerPrefs.SetInt("EstadoRetirada", 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene("03_MinijuegoRetirada");
    }

    // ← MÉTODO NUEVO
    public void DialogoDespuesDelMinijuego()
{
    indice = 0;

    finalStage = false;
    segundaFase = false;

    postMinijuego = true;
    aplausoHecho = false;

    dialogos = new string[]
    {
        "¡Excelente trabajo!",
        "Los pensamientos siguen existiendo...",
        "Pero ahora ya no bloquean el camino.",
        "La Puerta de la Retirada está abierta.",
        "Has conseguido la Llave de la Retirada.",
        "Ahora podemos seguir adelante. 😊"
    };

    if (panelDialogo != null)
        panelDialogo.SetActive(true);

    MostrarDialogoActual();
}
}