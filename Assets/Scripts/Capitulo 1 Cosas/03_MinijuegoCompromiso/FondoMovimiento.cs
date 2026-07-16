using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FondoScroll : MonoBehaviour
{
    RectTransform rect;

    [Header("Movimiento")]
    [Tooltip("Dirección del scroll. Ej: (0,-1) = hacia abajo, (0,1) = hacia arriba, (-1,0) = hacia la izquierda")]
    public Vector2 direccion = new Vector2(0, -1);

    [Tooltip("Velocidad en píxeles por segundo")]
    public float velocidad = 30f;

    bool activo = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    /// <summary>Empieza a mover el fondo. Lo llama el RespiracionManager cuando arranca el minijuego.</summary>
    public void IniciarScroll()
    {
        activo = true;
    }

    /// <summary>Detiene el fondo. Lo llama el RespiracionManager cuando termina el minijuego.</summary>
    public void DetenerScroll()
    {
        activo = false;
    }

    void Update()
    {
        if (!activo) return;

        rect.anchoredPosition += direccion.normalized * velocidad * Time.deltaTime;
    }
}