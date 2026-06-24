using UnityEngine;
using System.Collections;

public class EntradaClinico : MonoBehaviour
{
    [Header("Puntos")]
    public Transform puntoInicio;
    public Transform puntoFinal;

    [Header("Animación")]
    public Animator animator;
    public float velocidad = 1.0f;
    public float rotacionVelocidad = 5f;

    // Removed optional flag; always use Fishing Idle at the end
    // Flag to allow skipping the intro for testing
    public bool saltarIntro = false;

    void Start()
    {
        if (saltarIntro)
        {
            // Directly play idle animation and exit
            animator.CrossFade("Fishing Idle", 0.2f);
            return;
        }

        transform.position = puntoInicio.position;
        StartCoroutine(SecuenciaEntrada());
    }

    IEnumerator SecuenciaEntrada()
    {
        // 
        yield return new WaitForSeconds(2f);

        // 🚶 caminar
        animator.CrossFade("Walking", 0.55f);

        // Reduce the distance threshold to keep walking animation longer
        while (Vector3.Distance(transform.position, puntoFinal.position) > 0.001f)
        {
            // movimiento
            transform.position = Vector3.MoveTowards(
                transform.position,
                puntoFinal.position,
                velocidad * Time.deltaTime
            );

            // rotación suave hacia el punto
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

        // 📍 snap final exacto (optional, keep for precision)
        // transform.position = puntoFinal.position; // commented to avoid abrupt snap

        // 🧍 idle final – always transition to Fishing Idle
        animator.CrossFade("Fishing Idle", 0.4f);
    }
}