using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CinematicaFinal : MonoBehaviour
{
    [Header("Personajes")]
    public GameObject hombre;
    public GameObject mujer;
    private Animator animator;

    [Header("Cámaras")]
    public GameObject camaraJuego;
    public GameObject camaraFinal;

    [Header("Fade de Cámara")]
    public Image panelFadeCamara;
    public float duracionFadeCamara = 0.4f;

    [Header("Panel Final")]
    public GameObject panelFinal;
    public float duracionFadePanelFinal = 1.5f;

    [Header("Estrellas (arrastrar las 3 en orden)")]
    public RectTransform[] estrellas;
    public float duracionPopEstrella = 0.35f;
    public float pausaEntreEstrellas = 0.25f;
    public float pausaAntesDeEstrellas = 0.4f;

    [Header("Logros (arrastrar los 4 en orden)")]
    public RectTransform[] logros;
    public float duracionPopLogro = 0.3f;
    public float pausaEntreLogros = 0.2f;
    public float pausaAntesDeLogros = 0.5f;

    [Header("Botón Reiniciar")]
    public GameObject botonReiniciar;
    public float duracionFadeBoton = 0.6f;
    public float pausaAntesDelBoton = 0.4f;

    [Header("Animación")]
    public string nombreAnimacion = "Excited";

    [Header("Tiempos")]
    public float esperaAntesDelCambioCamara = 0.3f;
    public float duracionBaile = 3f;

    [Header("Sonidos Finales")]
    public AudioSource audioSource;
    public AudioClip sonidoCinematica;
    public AudioClip sonidoPopLogro;
    public AudioClip musicaMenu;

    private CanvasGroup canvasPanel;

    void Start()
    {
        int avatar = PlayerPrefs.GetInt("Avatar", 1);
        GameObject jugadorActivo = avatar == 1 ? hombre : mujer;

        if (jugadorActivo != null)
        {
            animator = jugadorActivo.GetComponent<Animator>();
        }
        else
        {
            Animator[] todosLosAnimators = FindObjectsOfType<Animator>();
            foreach (Animator anim in todosLosAnimators)
            {
                string nombreObj = anim.gameObject.name.ToLower();
                if (!nombreObj.Contains("clinico") && !nombreObj.Contains("avatar"))
                {
                    animator = anim;
                    break;
                }
            }
        }

        if (camaraJuego == null && Camera.main != null)
            camaraJuego = Camera.main.gameObject;

        if (camaraFinal == null)
        {
            camaraFinal = GameObject.Find("CamaraFinal");
            if (camaraFinal == null)
                camaraFinal = GameObject.Find("CamaraCinematica");
        }

        if (camaraFinal != null)
            camaraFinal.SetActive(false);

        // Desactivado por defecto para no bloquear Raycasts/clics
        if (panelFadeCamara != null)
        {
            panelFadeCamara.gameObject.SetActive(false);
            Color c = panelFadeCamara.color;
            c.a = 0f;
            panelFadeCamara.color = c;
        }

        if (panelFinal != null)
        {
            panelFinal.SetActive(false);

            canvasPanel = panelFinal.GetComponent<CanvasGroup>();

            if (canvasPanel == null)
                canvasPanel = panelFinal.AddComponent<CanvasGroup>();

            canvasPanel.alpha = 0f;
            canvasPanel.interactable = false;
            canvasPanel.blocksRaycasts = false;
        }

        // Preparar estrellas ocultas (escala 0)
        foreach (RectTransform estrella in estrellas)
        {
            if (estrella != null)
            {
                estrella.gameObject.SetActive(false);
                estrella.localScale = Vector3.zero;
            }
        }

        // Preparar logros ocultos (escala 0)
        foreach (RectTransform logro in logros)
        {
            if (logro != null)
            {
                logro.gameObject.SetActive(false);
                logro.localScale = Vector3.zero;
            }
        }

        // Preparar botón oculto
        if (botonReiniciar != null)
        {
            botonReiniciar.SetActive(false);

            CanvasGroup cgBoton = botonReiniciar.GetComponent<CanvasGroup>();
            if (cgBoton == null)
                cgBoton = botonReiniciar.AddComponent<CanvasGroup>();

            cgBoton.alpha = 0f;
            cgBoton.interactable = false;
            cgBoton.blocksRaycasts = false;
        }
    }

    public void Iniciar()
    {
        if (audioSource != null && sonidoCinematica != null)
        {
            audioSource.PlayOneShot(sonidoCinematica);
        }
        StartCoroutine(SecuenciaFinal());
    }

    IEnumerator SecuenciaFinal()
    {
        yield return new WaitForSeconds(esperaAntesDelCambioCamara);

        yield return StartCoroutine(CambiarCamaraConFade());

        if (animator != null)
            animator.CrossFade(nombreAnimacion, 0.15f);

        yield return new WaitForSeconds(duracionBaile);

        yield return StartCoroutine(SecuenciaPanelFinal());
    }

    IEnumerator CambiarCamaraConFade()
    {
        if (panelFadeCamara != null)
        {
            panelFadeCamara.gameObject.SetActive(true); // Se activa para la transición

            float t = 0f;
            Color c = panelFadeCamara.color;

            while (t < duracionFadeCamara)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / duracionFadeCamara);
                panelFadeCamara.color = c;
                yield return null;
            }
            c.a = 1f;
            panelFadeCamara.color = c;
        }

        if (camaraJuego != null)
            camaraJuego.SetActive(false);

        if (camaraFinal != null)
            camaraFinal.SetActive(true);

        if (panelFadeCamara != null)
        {
            float t = 0f;
            Color c = panelFadeCamara.color;

            while (t < duracionFadeCamara)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / duracionFadeCamara);
                panelFadeCamara.color = c;
                yield return null;
            }
            c.a = 0f;
            panelFadeCamara.color = c;

            panelFadeCamara.gameObject.SetActive(false); // Se apaga al terminar la transición
        }
    }

    IEnumerator SecuenciaPanelFinal()
    {
        // 1. Fade del panel base (UI general)
        yield return StartCoroutine(MostrarPanelBase());

        // 2. Pop de las estrellas, una por una
        yield return new WaitForSeconds(pausaAntesDeEstrellas);

        foreach (RectTransform estrella in estrellas)
        {
            if (estrella != null)
                yield return StartCoroutine(PopIn(estrella, duracionPopEstrella));

            yield return new WaitForSeconds(pausaEntreEstrellas);
        }

        // 3. Pop de los logros, uno por uno
        yield return new WaitForSeconds(pausaAntesDeLogros);

        foreach (RectTransform logro in logros)
        {
            if (logro != null)
                yield return StartCoroutine(FadeInSuave(logro, duracionPopLogro));

            yield return new WaitForSeconds(pausaEntreLogros);
        }

        // 4. Fade del botón "Comenzar de nuevo"
        yield return new WaitForSeconds(pausaAntesDelBoton);
        yield return StartCoroutine(MostrarBotonReiniciar());
    }

    IEnumerator MostrarPanelBase()
    {
        if (panelFinal == null) yield break;

        panelFinal.SetActive(true);

        canvasPanel.alpha = 0f;
        canvasPanel.interactable = false;
        canvasPanel.blocksRaycasts = false;

        float t = 0f;

        while (t < duracionFadePanelFinal)
        {
            t += Time.deltaTime;
            canvasPanel.alpha = Mathf.Lerp(0f, 1f, t / duracionFadePanelFinal);
            yield return null;
        }

        canvasPanel.alpha = 1f;
        canvasPanel.interactable = true;
        canvasPanel.blocksRaycasts = true;
    }

    // Animación tipo "pop" con rebote: crece pasando de 0 a un poco más grande y se asienta en 1
    IEnumerator PopIn(RectTransform objeto, float duracion)
    {
        if (audioSource != null && sonidoPopLogro != null)
        {
            audioSource.PlayOneShot(sonidoPopLogro);
        }

        objeto.gameObject.SetActive(true);
        objeto.localScale = Vector3.zero;

        float sobrepasoEscala = 1.15f;
        float mitad = duracion * 0.6f;
        float resto = duracion - mitad;

        float t = 0f;
        while (t < mitad)
        {
            t += Time.deltaTime;
            float progreso = Mathf.Clamp01(t / mitad);
            objeto.localScale = Vector3.one * Mathf.Lerp(0f, sobrepasoEscala, progreso);
            yield return null;
        }

        t = 0f;
        while (t < resto)
        {
            t += Time.deltaTime;
            float progreso = Mathf.Clamp01(t / resto);
            objeto.localScale = Vector3.one * Mathf.Lerp(sobrepasoEscala, 1f, progreso);
            yield return null;
        }

        objeto.localScale = Vector3.one;
    }

    // Animación de fade-in suave para logros
    IEnumerator FadeInSuave(RectTransform objeto, float duracion)
    {
        if (audioSource != null && sonidoPopLogro != null)
        {
            audioSource.PlayOneShot(sonidoPopLogro);
        }

        objeto.gameObject.SetActive(true);
        objeto.localScale = Vector3.one; // Restaura la escala normal

        CanvasGroup cg = objeto.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = objeto.gameObject.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;

        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / duracion);
            yield return null;
        }

        cg.alpha = 1f;
    }

    IEnumerator MostrarBotonReiniciar()
    {
        if (botonReiniciar == null) yield break;

        botonReiniciar.SetActive(true);

        CanvasGroup cgBoton = botonReiniciar.GetComponent<CanvasGroup>();
        if (cgBoton == null)
            cgBoton = botonReiniciar.AddComponent<CanvasGroup>();

        cgBoton.alpha = 0f;
        cgBoton.interactable = false;
        cgBoton.blocksRaycasts = false;

        float t = 0f;

        while (t < duracionFadeBoton)
        {
            t += Time.deltaTime;
            cgBoton.alpha = Mathf.Lerp(0f, 1f, t / duracionFadeBoton);
            yield return null;
        }

        cgBoton.alpha = 1f;
        cgBoton.interactable = true;
        cgBoton.blocksRaycasts = true;
    }

    public void VolverAlInicio()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (MusicManager.Instance != null && musicaMenu != null)
        {
            MusicManager.Instance.CambiarMusica(musicaMenu);
        }

        SceneManager.LoadScene("00_MenuJuego");
    }
}