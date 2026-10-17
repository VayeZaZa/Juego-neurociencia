using UnityEngine;

public class EfectosUI : MonoBehaviour
{
    [Header("Pulso")]
    public bool activarPulso = false;
    public float velocidadPulso = 1.2f;
    public float escalaMin = 0.97f;
    public float escalaMax = 1.03f;

    [Header("Flotar")]
    public bool activarFlotar = false;
    public float velocidadFlotar = 1f;
    public float altura = 8f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time) + 1f) / 2f;

        if (activarPulso)
        {
            float escala = Mathf.Lerp(escalaMin, escalaMax,
                           (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f);
            transform.localScale = new Vector3(escala, escala, 1f);
        }

        if (activarFlotar)
        {
            float y = Mathf.Sin(Time.time * velocidadFlotar) * altura;
            transform.localPosition = posicionInicial + new Vector3(0f, y, 0f);
        }
    }
}