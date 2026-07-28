using UnityEngine;

public class MovimientoSimple : MonoBehaviour
{
    public Transform avatarClinico;
    public Transform puntoCamara;
    public Camera camaraPrincipal;
    public float velocidadCamara = 2f;

    public GameObject hombre;
    public GameObject mujer;

    private GameObject jugadorActivo;

    public Transform puntoAvatar;
    public Transform puntoJugador;

    public Animator animAvatar;
    public Animator animJugador;

    public GameObject panelDecision;
    public PresentacionCapitulo tituloCap;
    public DialogoRetirada dialogoRetirada;

    public float velocidad = 2f;
    public float velocidadGiro = 120f;

    private bool mover = false;
    private bool girando = false;

    void Start()
    {
        int avatar = PlayerPrefs.GetInt("Avatar", 1);
        jugadorActivo = avatar == 1 ? hombre : mujer;

        if (jugadorActivo != null)
        {
            animJugador = jugadorActivo.GetComponent<Animator>();
            Debug.Log("Jugador activo: " + jugadorActivo.name);
        }

               // Comprobamos si el minijuego de Retirada fue completado
        if (PlayerPrefs.GetInt("EstadoRetirada", 0) == 1 &&
            PlayerPrefs.GetInt("EstadoEcoCambio", 0) == 0 &&
            PlayerPrefs.GetInt("EstadoCompromiso", 0) == 0)
        {
            // Siempre restauramos físicamente la puerta/posición del jugador para que el escenario esté correcto
            RestaurarEstadoPuerta();

            if (dialogoRetirada != null)
                dialogoRetirada.DialogoDespuesDelMinijuego();
        }
        
    }

    public void Seguir()
    {
        if (panelDecision != null)
            panelDecision.SetActive(false);

        if (avatarClinico != null)
            avatarClinico.LookAt(puntoAvatar);

        if (jugadorActivo != null)
            jugadorActivo.transform.LookAt(puntoJugador);

        mover = true;
        girando = false;

        if (tituloCap != null)
        {
            tituloCap.gameObject.SetActive(false);
            tituloCap.gameObject.SetActive(true);
        }

        if (animAvatar != null)
            animAvatar.Play("Walking");

        if (animJugador != null)
        {
            animJugador.SetBool("Walking", true);
            Debug.Log("Jugador Siguiendo :D");
        }
    }

    public bool EstaMoviendose()
    {
        return mover;
    }

    void Update()
    {
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

        if (!mover)
            return;

        if (avatarClinico != null)
        {
            avatarClinico.position = Vector3.MoveTowards(
                avatarClinico.position,
                puntoAvatar.position,
                velocidad * Time.deltaTime
            );
        }

        if (jugadorActivo != null)
        {
            jugadorActivo.transform.position = Vector3.MoveTowards(
                jugadorActivo.transform.position,
                puntoJugador.position,
                velocidad * Time.deltaTime
            );
        }

        bool clinicoLlego =
            avatarClinico != null &&
            Vector3.Distance(avatarClinico.position, puntoAvatar.position) < 0.05f;

        bool jugadorLlego =
            jugadorActivo != null &&
            Vector3.Distance(jugadorActivo.transform.position, puntoJugador.position) < 0.05f;

        if (clinicoLlego && jugadorLlego)
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

            if (dialogoRetirada != null)
                dialogoRetirada.IniciarDialogo();

            Debug.Log("Llegaron a la puerta de la Retirada");
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
    }

    void RestaurarEstadoPuerta()
    {
        mover = false;
        girando = false;

        if (avatarClinico != null)
            avatarClinico.LookAt(jugadorActivo.transform);

        if (jugadorActivo != null)
            jugadorActivo.transform.position = puntoJugador.position;

        if (animAvatar != null)
            animAvatar.Play("Fishing Idle");

        if (animJugador != null)
            animJugador.SetBool("Walking", false);

        if (camaraPrincipal != null && puntoCamara != null)
        {
            camaraPrincipal.transform.position = puntoCamara.position;
            camaraPrincipal.transform.rotation = puntoCamara.rotation;
        }
    }

    public void SkipMovimiento()
    {
        mover = false;

        if (avatarClinico != null)
            avatarClinico.position = puntoAvatar.position;

        if (jugadorActivo != null)
            jugadorActivo.transform.position = puntoJugador.position;

        if (camaraPrincipal != null && puntoCamara != null)
        {
            camaraPrincipal.transform.position = puntoCamara.position;
            camaraPrincipal.transform.rotation = puntoCamara.rotation;
        }

        if (animJugador != null)
            animJugador.SetBool("Walking", false);

        if (avatarClinico != null && jugadorActivo != null)
        {
            Vector3 dir = jugadorActivo.transform.position - avatarClinico.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
                avatarClinico.rotation = Quaternion.LookRotation(dir);
        }

        girando = false;

        if (animAvatar != null)
            animAvatar.Play("Fishing Idle");
    }
}