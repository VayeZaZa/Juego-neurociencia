using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource audioSource;

    [Header("Fade al cambiar de canción")]
    public float fadeOutDuration = 2f;
    public float fadeInDuration = 2f;

    private float volumenOriginal;
    private Coroutine transicionCoroutine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        volumenOriginal = audioSource.volume;

        // El loop lo maneja Unity de forma nativa (sin cortes)
        audioSource.loop = true;
    }

    void Start()
    {
        audioSource.volume = volumenOriginal;
        audioSource.Play();
    }

    IEnumerator Fade(float desde, float hasta, float duracion)
    {
        float t = 0;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = Mathf.SmoothStep(0f, 1f, t / duracion);
            audioSource.volume = Mathf.Lerp(desde, hasta, progreso);
            yield return null;
        }

        audioSource.volume = hasta;
    }

    // ============================================
    // Detener música suavemente (fade out y stop)
    // ============================================
    public void DetenerMusica()
    {
        if (transicionCoroutine != null)
        {
            StopCoroutine(transicionCoroutine);
            transicionCoroutine = null;
        }

        transicionCoroutine = StartCoroutine(DetenerMusicaSuave());
    }

    private IEnumerator DetenerMusicaSuave()
    {
        yield return StartCoroutine(Fade(audioSource.volume, 0, fadeOutDuration));

        audioSource.Stop();
        audioSource.time = 0;
    }

    // ============================================
    // Cambiar a una nueva canción: baja la actual, luego sube la nueva
    // ============================================
    public void CambiarMusica(AudioClip nuevoClip)
    {
        if (nuevoClip == null) return;

        if (transicionCoroutine != null)
        {
            StopCoroutine(transicionCoroutine);
            transicionCoroutine = null;
        }

        transicionCoroutine = StartCoroutine(CambiarMusicaRoutine(nuevoClip));
    }

    private IEnumerator CambiarMusicaRoutine(AudioClip nuevoClip)
    {
        // Si ya está sonando esta misma canción, no hacer nada
        if (audioSource.clip == nuevoClip && audioSource.isPlaying)
            yield break;

        // 1. Bajar el volumen de lo que está sonando ahora
        yield return StartCoroutine(Fade(audioSource.volume, 0, fadeOutDuration));

        audioSource.Stop();
        audioSource.time = 0;

        // 2. Poner el nuevo clip y reproducir en loop nativo
        audioSource.clip = nuevoClip;
        audioSource.loop = true;
        audioSource.volume = 0;
        audioSource.Play();

        // 3. Subir el volumen despacio
        yield return StartCoroutine(Fade(0, volumenOriginal, fadeInDuration));
    }
}