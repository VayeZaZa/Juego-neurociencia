using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlDecision : MonoBehaviour
{
    public void Seguir()
    {
        Debug.Log("El jugador decidió seguir.");
    }

    public void Abandonar()
    {
        SceneManager.LoadScene("00_MenuJuego");
    }
}