using UnityEngine;
using System.Collections;

public class IntroJugador : MonoBehaviour
{
    public Animator animator;

    // Canción que sonará en este capítulo
    public AudioClip musicaCapitulo;

    // Flag to allow skipping the intro cinematic (useful for testing)
    public bool saltarIntro = false;

    public GameObject camaraCinematica;
    public GameObject camaraJugador;

    // UI completa de diálogos
    public GameObject uiDialogos;

    public GameObject presentacionCapitulo;
    public GameObject tituloCapitulo;
    public GameObject textoCapitulo;

    // Clinical avatar and dialogue point references
    public Transform avatarClinico;
    public Transform puntoDialogo;

    // ============================================
    // Fade de pantalla blanca al entrar a la escena
    // ============================================
    [Header("Fade pantalla blanca")]
    public CanvasGroup fadeBlanco;
    public float duracionFade = 1.5f;

    IEnumerator Start()
    {
        // ============================================
        // Cambiar a la música de este capítulo
        // ============================================
        if (MusicManager.Instance != null && musicaCapitulo != null)
        {
            MusicManager.Instance.CambiarMusica(musicaCapitulo);
        }

        // ============================================
        // SI VOLVEMOS DEL MINIJUEGO
        // ============================================
        if (PlayerPrefs.GetInt("EstadoRetirada", 0) == 1)
        {
            // Ocultamos el fade blanco inmediatamente para que no aparezca
            if (fadeBlanco != null)
            {
                fadeBlanco.alpha = 0f;
                fadeBlanco.blocksRaycasts = false;
                fadeBlanco.gameObject.SetActive(false);
            }

            camaraCinematica.SetActive(false);
            camaraJugador.SetActive(true);

            animator.Play("Happy Idle");

            presentacionCapitulo.SetActive(false);
            tituloCapitulo.SetActive(false);
            textoCapitulo.SetActive(false);

            if (uiDialogos != null)
                uiDialogos.SetActive(false);

            yield break;
        }

        // ============================================
        // Fade in de pantalla blanca a transparente (Solo en la cinemática inicial)
        // ============================================
        if (fadeBlanco != null)
        {
            StartCoroutine(FadeDesdeBlanco());
        }

        // Ocultar diálogos al iniciar
        if (uiDialogos != null)
            uiDialogos.SetActive(false);

        // Modo testing: saltar intro
        if (saltarIntro)
        {
            camaraCinematica.SetActive(false);
            camaraJugador.SetActive(true);

            animator.Play("Happy Idle");

            presentacionCapitulo.SetActive(false);
            tituloCapitulo.SetActive(false);
            textoCapitulo.SetActive(false);

            if (avatarClinico != null && puntoDialogo != null)
            {
                avatarClinico.position = puntoDialogo.position;
                avatarClinico.rotation = puntoDialogo.rotation;
            }

            // Mostrar diálogos inmediatamente
            if (uiDialogos != null)
                uiDialogos.SetActive(true);

            yield break;
        }

        // Inicio cinemática
        camaraCinematica.SetActive(true);
        camaraJugador.SetActive(false);

        // 1. Despertar
        animator.Play("Waking");
        yield return new WaitForSeconds(4f);

        // 2. Levantarse
        animator.Play("Standing Up");
        yield return new WaitForSeconds(9f);

        // 3. Idle
        animator.Play("Happy Idle");

        // Ocultar UI del capítulo
        presentacionCapitulo.SetActive(false);
        tituloCapitulo.SetActive(false);
        textoCapitulo.SetActive(false);

        // Colocar avatar clínico
        if (avatarClinico != null && puntoDialogo != null)
        {
            avatarClinico.position = puntoDialogo.position;
            avatarClinico.rotation = puntoDialogo.rotation;
        }

        // Cambiar a cámara de juego
        camaraCinematica.SetActive(false);
        camaraJugador.SetActive(true);

        // Mostrar diálogos al terminar la intro
        if (uiDialogos != null)
            uiDialogos.SetActive(true);
    }

    IEnumerator FadeDesdeBlanco()
    {
        fadeBlanco.alpha = 1f;
        fadeBlanco.blocksRaycasts = true;

        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            fadeBlanco.alpha = Mathf.Lerp(1f, 0f, t / duracionFade);
            yield return null;
        }

        fadeBlanco.alpha = 0f;
        fadeBlanco.blocksRaycasts = false;
    }
}