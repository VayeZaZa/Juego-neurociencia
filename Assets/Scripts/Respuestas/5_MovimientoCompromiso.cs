using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MovimientoCompromiso : MonoBehaviour
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
    public DialogoCompromiso dialogoCompromiso;

    [Header("Fade durante el recorrido")]
    public Image panelFade;

    [Header("Puertas")]
    public GameObject puertaCerrada;
    public GameObject puertaAbierta;

    [Header("Estado Restaurado")]
    public Transform puntoAvatarPost;
    public Transform puntoJugadorPost;
    public Transform puntoCamaraPost;

    [Header("Movimiento")]
    public float velocidad = 2f;
    public float velocidadGiro = 240f;

    private bool mover = false;
    private bool girando = false;

    void Start()
    {
        int avatar = PlayerPrefs.GetInt("Avatar", 1);
        jugadorActivo = avatar == 1 ? hombre : mujer;

        if (jugadorActivo != null)
            animJugador = jugadorActivo.GetComponent<Animator>();

        if (panelFade != null)
            panelFade.gameObject.SetActive(false);

        // Restaurar estado si ya completamos el minijuego de Compromiso
        if (PlayerPrefs.GetInt("EstadoCompromiso", 0) == 1)
        {
            RestaurarEstadoCompromiso();
            StartCoroutine(MostrarDialogoDespues());
        }
    }

    IEnumerator MostrarDialogoDespues()
    {
        // Espera 3 frames para que todos los Start() de la escena hayan terminado de apagar sus paneles
        yield return null;
        yield return null;
        yield return null;

        if (dialogoCompromiso != null)
            dialogoCompromiso.DialogoDespuesDelMinijuego();
    }

    void RestaurarEstadoCompromiso()
    {
        mover = false;
        girando = false;

        // Abrir la puerta físicamente
        if (puertaCerrada != null) puertaCerrada.SetActive(false);
        if (puertaAbierta != null) puertaAbierta.SetActive(true);

        // Reposicionar el avatar clínico en su destino post-juego
        if (avatarClinico != null && puntoAvatarPost != null)
        {
            avatarClinico.position = puntoAvatarPost.position;
            avatarClinico.rotation = puntoAvatarPost.rotation;
        }

        // Reposicionar el jugador activo en su destino post-juego
        if (jugadorActivo != null && puntoJugadorPost != null)
        {
            jugadorActivo.transform.position = puntoJugadorPost.position;
            jugadorActivo.transform.rotation = puntoJugadorPost.rotation;
            jugadorActivo.transform.Rotate(0f, 180f, 0f);
        }

        // Reposicionar la cámara en su destino post-juego
        if (camaraPrincipal != null && puntoCamaraPost != null)
        {
            camaraPrincipal.transform.position = puntoCamaraPost.position;
            camaraPrincipal.transform.rotation = puntoCamaraPost.rotation;
        }

        // Configurar animaciones de Idle
        if (animAvatar != null)
            animAvatar.Play("Fishing Idle");

        if (animJugador != null)
        {
            animJugador.SetBool("Walking", false);
            animJugador.Play("Happy Idle");
        }
    }

    public void IniciarRecorrido()
    {
        Debug.Log("Comienza el recorrido hacia Compromiso.");

        mover = true;

        if (panelFade != null)
            panelFade.gameObject.SetActive(true);

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
        // Giro final: el avatar clínico voltea suavemente hacia el jugador al terminar el recorrido
        if (girando && avatarClinico != null && jugadorActivo != null)
        {
            Vector3 dir = jugadorActivo.transform.position - avatarClinico.position;
            dir.y = 0f;

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
            else
            {
                girando = false;
            }
        }

        if (!mover) return;

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

        bool avatarLlego =
            avatarClinico != null && puntoAvatar != null &&
            Vector3.Distance(avatarClinico.position, puntoAvatar.position) < 0.05f;

        bool jugadorLlego =
            jugadorActivo != null && puntoJugador != null &&
            Vector3.Distance(jugadorActivo.transform.position, puntoJugador.position) < 0.05f;

        if (avatarLlego && jugadorLlego)
        {
            mover = false;

            if (animAvatar != null)
                animAvatar.Play("Fishing Idle");

            if (animJugador != null)
                animJugador.SetBool("Walking", false);

            if (panelFade != null)
                panelFade.gameObject.SetActive(false);

                girando = true;

                if (dialogoCompromiso != null)
                {
                    dialogoCompromiso.IniciarDialogo();
                }

                Debug.Log("Llegaron al punto de Compromiso.");
        }
    }
}