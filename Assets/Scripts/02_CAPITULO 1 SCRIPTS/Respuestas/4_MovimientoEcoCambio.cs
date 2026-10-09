using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MovimientoEcoCambio : MonoBehaviour
{
    [Header("Personajes")]
    public Transform avatarClinico;
    public GameObject hombre;
    public GameObject mujer;
    private GameObject jugadorActivo;

    [Header("Destino")]
    public Transform puntoAvatar;
    public Transform puntoJugador;

    [Header("Cámara")]
    public Camera camaraPrincipal;
    public Transform puntoCamara;
    public float velocidadCamara = 2f;

    [Header("Animadores")]
    public Animator animAvatar;
    public Animator animJugador;

    [Header("Diálogo")]
    public DialogoEcoCambio dialogoEcoCambio;

    [Header("Retirada")]
    public GameObject panelDecision2;

    [Header("Presentación")]
    public PresentacionCapitulo tituloCap;

    [Header("Puertas")]
    public GameObject puertaCerrada;
    public GameObject puertaAbierta;

    [Header("Estado Restaurado")]
    public Transform puntoAvatarPost;
    public Transform puntoJugadorPost;
    public Transform puntoCamaraPost;

    [Header("Fade Entrada")]
    public Image fadeInicial;
    public float duracionFade = 1f;

    [Header("Música")]
    public AudioClip musicaEcoCambio;

    [Header("Movimiento")]
    public float velocidad = 2f;
    public float velocidadGiro = 120f;

    private bool mover = false;
    private bool girando = false;
    private bool estadoRestaurado = false;
    private bool vieneDelMinijuego;

    void Start()
    {
        Debug.Log("EstadoEcoCambio = " + PlayerPrefs.GetInt("EstadoEcoCambio", 0));
        vieneDelMinijuego = PlayerPrefs.GetInt("EstadoEcoCambio", 0) == 1;

        int avatar = PlayerPrefs.GetInt("Avatar", 1);
        jugadorActivo = avatar == 1 ? hombre : mujer;

        if (jugadorActivo != null)
            animJugador = jugadorActivo.GetComponent<Animator>();

        if (PlayerPrefs.GetInt("EstadoEcoCambio", 0) == 1 &&
            PlayerPrefs.GetInt("EstadoCompromiso", 0) == 0)
        {
            RestaurarEstadoEcoCambio();
            StartCoroutine(MostrarDialogoDespues());
        }
    }

    IEnumerator MostrarDialogoDespues()
    {
        // Fade negro al inicio
        if (fadeInicial != null)
        {
            fadeInicial.gameObject.SetActive(true);
            Color c = fadeInicial.color;
            c.a = 1f;
            fadeInicial.color = c;
        }

        // Espera 3 frames para que todos los Start() terminen
        yield return null;
        yield return null;
        yield return null;

        // Reposicionar después de que todo esté inicializado
        if (avatarClinico != null && puntoAvatarPost != null)
        {
            avatarClinico.position = puntoAvatarPost.position;
            avatarClinico.rotation = puntoAvatarPost.rotation;
        }

        if (jugadorActivo != null && puntoJugadorPost != null)
        {
            jugadorActivo.transform.position = puntoJugadorPost.position;
            jugadorActivo.transform.rotation = puntoJugadorPost.rotation;
            jugadorActivo.transform.Rotate(0f, 180f, 0f);
        }

        if (camaraPrincipal != null && puntoCamaraPost != null)
        {
            camaraPrincipal.transform.position = puntoCamaraPost.position;
            camaraPrincipal.transform.rotation = puntoCamaraPost.rotation;
        }

        if (animAvatar != null)
            animAvatar.CrossFade("Fishing Idle", 0.2f);

        if (animJugador != null)
        {
            animJugador.SetBool("Walking", false);
            animJugador.CrossFade("Happy Idle", 0.2f);
        }

        // Desvanecer el fade negro
        if (fadeInicial != null)
        {
            Color c = fadeInicial.color;
            float t = 0f;

            while (t < duracionFade)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / duracionFade);
                fadeInicial.color = c;
                yield return null;
            }

            c.a = 0f;
            fadeInicial.color = c;
            fadeInicial.gameObject.SetActive(false);
        }

        if (dialogoEcoCambio != null)
            dialogoEcoCambio.DialogoDespuesDelMinijuego();
    }

    void RestaurarEstadoEcoCambio()
    {
        estadoRestaurado = true;
        mover   = false;
        girando = false;

        if (puertaCerrada != null) puertaCerrada.SetActive(false);
        if (puertaAbierta != null) puertaAbierta.SetActive(true);
    }

    public void Seguir()
    {
        if (panelDecision2 != null)
            panelDecision2.SetActive(false);

        mover = true;
        girando = false;

        // ============================================
        // Cambiar música al iniciar la caminata hacia Eco del Cambio
        // ============================================
        if (MusicManager.Instance != null && musicaEcoCambio != null)
        {
            MusicManager.Instance.CambiarMusica(musicaEcoCambio);
        }

        if (tituloCap != null)
        {
            tituloCap.gameObject.SetActive(false);
            tituloCap.gameObject.SetActive(true);
        }

        if (animAvatar != null)
            animAvatar.Play("Walking");

        if (animJugador != null)
            animJugador.SetBool("Walking", true);
    }

    // Gira suavemente un transform hacia un punto de destino (solo en el eje Y)
    private void GirarHaciaDestino(Transform objeto, Vector3 destino)
    {
        Vector3 dir = destino - objeto.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(dir);
            objeto.rotation = Quaternion.RotateTowards(
                objeto.rotation,
                rotObjetivo,
                velocidadGiro * Time.deltaTime
            );
        }
    }

    void Update()
    {
        if (girando && avatarClinico != null && jugadorActivo != null)
        {
            Vector3 dir = jugadorActivo.transform.position - avatarClinico.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion rotObjetivo = Quaternion.LookRotation(dir);
                avatarClinico.rotation = Quaternion.RotateTowards(
                    avatarClinico.rotation,
                    rotObjetivo,
                    velocidadGiro * Time.deltaTime
                );

                if (Quaternion.Angle(avatarClinico.rotation, rotObjetivo) < 1f)
                {
                    avatarClinico.rotation = rotObjetivo;
                    girando = false;
                }
            }
        }

        if (!mover) return;

        if (panelDecision2 != null && panelDecision2.activeSelf)
            panelDecision2.SetActive(false);

        if (avatarClinico != null && puntoAvatar != null)
        {
            GirarHaciaDestino(avatarClinico, puntoAvatar.position);

            avatarClinico.position = Vector3.MoveTowards(
                avatarClinico.position,
                puntoAvatar.position,
                velocidad * Time.deltaTime
            );
        }

        if (jugadorActivo != null && puntoJugador != null)
        {
            GirarHaciaDestino(jugadorActivo.transform, puntoJugador.position);

            jugadorActivo.transform.position = Vector3.MoveTowards(
                jugadorActivo.transform.position,
                puntoJugador.position,
                velocidad * Time.deltaTime
            );
        }

        if (camaraPrincipal != null && puntoCamara != null)
        {
            camaraPrincipal.transform.position = Vector3.MoveTowards(
                camaraPrincipal.transform.position,
                puntoCamara.position,
                velocidadCamara * Time.deltaTime
            );

            camaraPrincipal.transform.rotation = Quaternion.Slerp(
                camaraPrincipal.transform.rotation,
                puntoCamara.rotation,
                velocidadCamara * Time.deltaTime
            );
        }

        bool avatarLlego  = avatarClinico != null && puntoAvatar != null && Vector3.Distance(avatarClinico.position, puntoAvatar.position) < 0.05f;
        bool jugadorLlego = jugadorActivo != null && puntoJugador != null && Vector3.Distance(jugadorActivo.transform.position, puntoJugador.position) < 0.05f;

        if (avatarLlego && jugadorLlego)
        {
            mover = false;

            if (animAvatar != null)
            {
                animAvatar.SetBool("Hablando", false);
                animAvatar.Play("Fishing Idle");
            }

            if (animJugador != null)
                animJugador.SetBool("Walking", false);

            girando = true;

            if (!vieneDelMinijuego && dialogoEcoCambio != null)
            {
                dialogoEcoCambio.IniciarDialogo();
            }

            Debug.Log("Llegaron a Eco del Cambio");
        }
    }
}