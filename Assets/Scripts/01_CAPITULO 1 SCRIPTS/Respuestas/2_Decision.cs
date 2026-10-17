using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ControlDecision : MonoBehaviour
{
    [Header("Fade")]
    public Image panelFade;
    public float duracionFade = 1f;

    [Header("Texto Abandono")]
    public TMP_Text textoAbandono;
    public float velocidadEscritura = 0.08f;

    [Header("Música")]
    public AudioClip musicaMenu;

    private string mensajeAbandono = "Puedes salir. Pero si vuelves, el corredor seguirá aquí. Ningún mapa se revela a quien no entra.";

    public void Seguir()
    {
        Debug.Log("El jugador decidió seguir.");
    }

    public void Abandonar()
    {
        StartCoroutine(FadeYCargar("00_MenuJuego"));
    }

    private IEnumerator FadeYCargar(string escena)
    {
        panelFade.gameObject.SetActive(true);

        if (textoAbandono != null)
        {
            textoAbandono.gameObject.SetActive(true);
            textoAbandono.text = "";
        }

        // ============================================
        // Volver a la música del menú al abandonar
        // ============================================
        if (MusicManager.Instance != null && musicaMenu != null)
        {
            MusicManager.Instance.CambiarMusica(musicaMenu);
        }

        float t = 0f;
        Color color = panelFade.color;
        int letraActual = 0;
        float tiempoUltraLetra = 0f;

        // FASE 1: Fade y typewriter juntos
        while (t < duracionFade)
        {
            t += Time.deltaTime;

            color.a = Mathf.Clamp01(t / duracionFade);
            panelFade.color = color;

            if (textoAbandono != null && letraActual < mensajeAbandono.Length)
            {
                tiempoUltraLetra += Time.deltaTime;
                while (tiempoUltraLetra >= velocidadEscritura && letraActual < mensajeAbandono.Length)
                {
                    textoAbandono.text += mensajeAbandono[letraActual];
                    letraActual++;
                    tiempoUltraLetra -= velocidadEscritura;
                }
            }

            yield return null;
        }

        // Pantalla negra
        color.a = 1f;
        panelFade.color = color;

        // FASE 2: Si el texto no terminó, seguir escribiendo en negro
        while (letraActual < mensajeAbandono.Length)
        {
            tiempoUltraLetra += Time.deltaTime;
            while (tiempoUltraLetra >= velocidadEscritura && letraActual < mensajeAbandono.Length)
            {
                textoAbandono.text += mensajeAbandono[letraActual];
                letraActual++;
                tiempoUltraLetra -= velocidadEscritura;
            }
            yield return null;
        }

        // FASE 3: Breve pausa con todo visible antes de cambiar
        yield return new WaitForSeconds(0.8f);

        SceneManager.LoadScene(escena);
    }
}