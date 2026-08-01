using UnityEngine;

public class PuertaController : MonoBehaviour
{
    [Header("Modelos de la puerta")]
    public GameObject puertaCerrada;
    public GameObject puertaAbierta;

    [Header("¿Qué puerta es esta?")]
    public string claveEstado = "EstadoRetirada"; // cambia en el Inspector

    void Awake()
    {
        int completado = PlayerPrefs.GetInt(claveEstado, 0);

        if (completado == 1)
        {
            puertaCerrada.SetActive(false);
            puertaAbierta.SetActive(true);
        }
        else
        {
            puertaCerrada.SetActive(true);
            puertaAbierta.SetActive(false);
        }
    }
}