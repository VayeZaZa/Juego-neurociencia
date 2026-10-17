using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SeleccionJugador : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelNombre;
    public GameObject panelAvatar;

    [Header("Input Nombre")]
    public TMP_InputField inputNombre;
    public GameObject botonContinuar;

    [Header("Avatares")]
    public Button btnHombre;
    public Button btnMujer;
    public GameObject btnComenzar;
    public CambiarCapitulo1 transicion;

    // Checkmarks for selected avatar
    public GameObject checkHombre;
    public GameObject checkMujer;

    void Start()
    {
        panelAvatar.SetActive(false);
        btnComenzar.SetActive(false);
    }

    public void Continuar()
    {
        string nombre = inputNombre.text.Trim();
        PlayerPrefs.SetString("NombreJugador", nombre);
        Debug.Log("Nombre guardado: " + nombre);
        panelNombre.SetActive(false);
        panelAvatar.SetActive(true);
    }

    // FUNCIÓN HOMBRE
    public void ElegirHombre()
    {
        PlayerPrefs.SetInt("Avatar", 1);
        // Activate/deactivate checkmarks
        if (checkHombre != null) checkHombre.SetActive(true);
        if (checkMujer != null) checkMujer.SetActive(false);
        Debug.Log("Avatar: Hombre");
        btnComenzar.SetActive(true);
    }

    // FUNCIÓN MUJER
    public void ElegirMujer()
    {
        PlayerPrefs.SetInt("Avatar", 2);
        // Activate/deactivate checkmarks
        if (checkMujer != null) checkMujer.SetActive(true);
        if (checkHombre != null) checkHombre.SetActive(false);
        Debug.Log("Avatar: Mujer");
        btnComenzar.SetActive(true);
    }

    public void VolverANombre()
    {
        panelAvatar.SetActive(false);
        panelNombre.SetActive(true);
        btnComenzar.SetActive(false);
        PlayerPrefs.DeleteKey("Avatar");
        Debug.Log("Volviendo a nombre");
    }

    public void Comenzar()
    {
        string nombre = PlayerPrefs.GetString("NombreJugador");
        int avatar = PlayerPrefs.GetInt("Avatar");
        string tipo = (avatar == 1) ? "Hombre" : "Mujer";

        Debug.Log("=== DATOS GUARDADOS ===");
        Debug.Log("Nombre: " + nombre);
        Debug.Log("Avatar: " + tipo);

        transicion.IrACapitulo1();
    }

    // FUNCIÓN BOTÓN MENU
    public void VolverAlMenu()
    {
        SceneManager.LoadScene("00_MenuJuego");
    }
}