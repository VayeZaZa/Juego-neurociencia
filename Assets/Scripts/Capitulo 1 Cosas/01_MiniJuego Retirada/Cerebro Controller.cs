using UnityEngine;

public class CerebroController : MonoBehaviour
{
    [Header("Configuracion Pulso")]
    public float velocidadPulso = 1.2f;
    public float intensidadPulso = 0.1f;

    private Vector3 escalaOriginal;

    void Start()
    {
        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        float pulso = 1f + Mathf.Sin(Time.time * velocidadPulso) * intensidadPulso;
        transform.localScale = escalaOriginal * pulso;
    }
}