using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text texto;

    [TextArea]
    public string mensajeCompleto;

    public GameObject botonContinuar;
    public TMP_InputField inputNombre;

    public float velocidad = 0.04f;

    void Start()
    {
        botonContinuar.SetActive(false);
        StartCoroutine(EscribirTexto());
    }

    IEnumerator EscribirTexto()
    {
        texto.text = "";

        foreach(char letra in mensajeCompleto)
        {
            texto.text += letra;
            yield return new WaitForSeconds(velocidad);
        }
    }

    void Update()
    {
        if (inputNombre.text.Length > 0)
            botonContinuar.SetActive(true);
        else
            botonContinuar.SetActive(false);
    }
}