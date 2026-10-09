using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterTMP : MonoBehaviour
{
    public TMP_Text texto;

    [TextArea]
    public string mensaje;

    public float velocidad = 0.03f;

    public bool autoPlay = true;

    private Coroutine rutina;

    void OnEnable()
    {
        if (autoPlay)
            Play();
    }

    public void Play()
    {
        if (rutina != null)
            StopCoroutine(rutina);

        rutina = StartCoroutine(Type());
    }

    IEnumerator Type()
    {
        texto.text = "";

        foreach (char c in mensaje)
        {
            texto.text += c;
            yield return new WaitForSeconds(velocidad);
        }
    }

    public void SetTexto(string nuevoTexto)
    {
        mensaje = nuevoTexto;
        Play();
    }
}