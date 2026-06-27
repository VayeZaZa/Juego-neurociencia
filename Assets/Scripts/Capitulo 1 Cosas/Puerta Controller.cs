using UnityEngine;

public class PuertaController : MonoBehaviour
{
    [Header("Modelos de la puerta")]
    public GameObject puertaCerrada;
    public GameObject puertaAbierta;

    void Start()
    {
        int completado = PlayerPrefs.GetInt("EstadoRetirada", 0);

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