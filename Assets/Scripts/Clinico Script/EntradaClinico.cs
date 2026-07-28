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

    void Start()
    {
        // SI VOLVEMOS DE CUALQUIER MINIJUEGO
        if (PlayerPrefs.GetInt("EstadoRetirada", 0) == 1)
        {
            // Solo lo colocamos en el punto de retorno de Retirada si NO hemos completado las siguientes puertas
            if (PlayerPrefs.GetInt("EstadoEcoCambio", 0) == 0 && PlayerPrefs.GetInt("EstadoCompromiso", 0) == 0)
            {
                if (puntoRetorno != null)
                {
                    transform.position = puntoRetorno.position;
                    transform.rotation = puntoRetorno.rotation;
                }
            }

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