using UnityEngine;

public class SeleccionarJugador : MonoBehaviour
{
    public GameObject jugadorHombre;
    public GameObject jugadorMujer;

    void Start()
    {
        int avatar = PlayerPrefs.GetInt("Avatar", 1);

        if (avatar == 1)
        {
            jugadorHombre.SetActive(true);
            jugadorMujer.SetActive(false);

            Debug.Log("Se cargó el avatar Hombre");
        }
        else
        {
            jugadorHombre.SetActive(false);
            jugadorMujer.SetActive(true);

            Debug.Log("Se cargó el avatar Mujer");
        }
    }
}