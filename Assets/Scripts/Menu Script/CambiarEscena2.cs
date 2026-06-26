using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeEntrada : MonoBehaviour
{
    public Image panelBlanco;
    public float duracion = 1f;

    void Start()
    {
        panelBlanco.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = duracion;

        while (t > 0)
        {
            t -= Time.deltaTime;
            float alpha = t / duracion;

            panelBlanco.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        panelBlanco.gameObject.SetActive(false);
    }
}