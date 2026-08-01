using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CambiarCapitulo1 : MonoBehaviour
{
    public Image panelBlanco;
    public float duracionFade = 1f;

    // 👉 IR AL CAPÍTULO 1
    public void IrACapitulo1()
    {
        StartCoroutine(FadeYCambiar("02_Capitulo_1"));
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

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(escena);
    }
}