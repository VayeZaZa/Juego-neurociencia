using UnityEngine;
using System.Collections;

public class EntradaClinico : MonoBehaviour
{
    [Header("Puntos")]
    public Transform puntoInicio;
    public Transform puntoFinal;

    // NUEVO
    [Header("Retorno desde Minijuego")]
    public Transform puntoRetorno;

    [Header("Animación")]
    public Animator animator;
    public float velocidad = 1.0f;
    public float rotacionVelocidad = 5f;

    public bool saltarIntro = false;

    void Awake()
    {
        // Al sustituir el modelo, el movimiento sigue perteneciendo a esta raiz.
        // El Animator del hijo GUIA solo controla las animaciones del modelo.
        // Awake se ejecuta antes de los Start que restauran los minijuegos.
        if (animator == null || animator.transform == transform ||
            !animator.transform.IsChildOf(transform))
            return;

        animator.applyRootMotion = false;

        // La raiz conserva el rig antiguo, pero ya no debe animarse.
        Animator animatorAnterior = GetComponent<Animator>();
        if (animatorAnterior != null && animatorAnterior != animator)
            animatorAnterior.enabled = false;

        // Incluye paneles y jugadores inactivos de ESTA escena. Asi los dialogos
        // que se activan mas tarde tambien reciben el Animator correcto.
        foreach (GameObject raizEscena in gameObject.scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour componente in raizEscena.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (componente is IntroJugador introduccion)
                {
                    introduccion.avatarClinico = transform;
                    // introduccion.animator pertenece al jugador y se conserva.
                }
                else if (componente is MovimientoSimple retirada)
                {
                    retirada.avatarClinico = transform;
                    retirada.animAvatar = animator;
                }
                else if (componente is MovimientoEcoCambio eco)
                {
                    eco.avatarClinico = transform;
                    eco.animAvatar = animator;
                }
                else if (componente is MovimientoCompromiso compromiso)
                {
                    compromiso.avatarClinico = transform;
                    compromiso.animAvatar = animator;
                }
                else if (componente is DialogoBienvenida bienvenida)
                    bienvenida.avatarAnimator = animator;
                else if (componente is DialogoRetirada dialogoRetirada)
                    dialogoRetirada.avatarAnimator = animator;
                else if (componente is DialogoEcoCambio dialogoEco)
                    dialogoEco.avatarAnimator = animator;
                else if (componente is DialogoCompromiso dialogoCompromiso)
                    dialogoCompromiso.avatarAnimator = animator;
                else if (componente is RecompensaCompromiso recompensa)
                    recompensa.avatarAnimator = animator;
            }
        }
    }

    void Start()
    {
        // SI VOLVEMOS DE CUALQUIER MINIJUEGO
        if (PlayerPrefs.GetInt("EstadoRetirada", 0) == 1)
        {
            // Cada recorrido restaura su propia posicion y giro. Para Retirada,
            // MovimientoSimple recupera la pose guardada antes del minijuego.
            // No usar puntoRetorno aqui: pisaria esa pose segun el orden de Start.
            if (animator != null)
                animator.CrossFade("Fishing Idle", 0.2f);

            return;
        }

        // Modo Testing
        if (saltarIntro)
        {
            animator.CrossFade("Fishing Idle", 0.2f);
            return;
        }

        transform.position = puntoInicio.position;
        StartCoroutine(SecuenciaEntrada());
    }

    IEnumerator SecuenciaEntrada()
    {
        yield return new WaitForSeconds(2f);

        animator.CrossFade("Walking", 0.55f);

        while (Vector3.Distance(transform.position, puntoFinal.position) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                puntoFinal.position,
                velocidad * Time.deltaTime
            );

            Vector3 dir = (puntoFinal.position - transform.position).normalized;

            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rot,
                    rotacionVelocidad * Time.deltaTime
                );
            }

            yield return null;
        }

        animator.CrossFade("Fishing Idle", 0.4f);
    }
}