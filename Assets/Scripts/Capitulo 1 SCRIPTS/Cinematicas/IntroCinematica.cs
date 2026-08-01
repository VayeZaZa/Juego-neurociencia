using UnityEngine;

public class MoverCamaraCinematica : MonoBehaviour
{
    public Transform puntoFinal;
    public float velocidad = 1f;
    public float velocidadRotacion = 2f;

    private bool llegue = false;

    void Update()
    {
        if (!llegue)
        {
            // Primero se aleja
            transform.position = Vector3.MoveTowards(
                transform.position,
                puntoFinal.position,
                velocidad * Time.deltaTime
            );

            // Cuando llega al punto final, activa la rotación
            if (Vector3.Distance(transform.position, puntoFinal.position) < 0.05f)
            {
                llegue = true;
            }
        }
        else
        {
            // Después rota suavemente
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                puntoFinal.rotation,
                velocidadRotacion * Time.deltaTime
            );
        }
    }
}