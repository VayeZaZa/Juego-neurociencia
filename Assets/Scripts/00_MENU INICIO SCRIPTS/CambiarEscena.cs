using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CambiarEscena : MonoBehaviour
{
    public Image panelBlanco; // arrastra el Panel blanco aquí en el Inspector
    public float duracionFade = 1f;

    public void Explorar()
    {
        StartCoroutine(FadeYCambiar("01_SeleccionAvatar"));
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    IEnumerator FadeYCambiar(string escena)
    {
        panelBlanco.gameObject.SetActive(true);
        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            float alpha = tiempo / duracionFade;
            panelBlanco.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // pausa en blanco puro
        SceneManager.LoadScene(escena);
    }
}