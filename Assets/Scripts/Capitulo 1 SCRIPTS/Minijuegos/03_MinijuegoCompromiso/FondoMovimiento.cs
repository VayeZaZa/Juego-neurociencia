using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class FondoScroll : MonoBehaviour
{
    RawImage img;

    [Header("Movimiento")]
    [Tooltip("Dirección del scroll en UV. Ej: (0,-1) = hacia abajo, (0,1) = hacia arriba")]
    public Vector2 direccion = new Vector2(0, -1);

    [Tooltip("Velocidad del scroll (en 'vueltas' de UV por segundo)")]
    public float velocidad = 0.1f;

    bool activo = false;
    Vector2 offset;

    void Awake()
    {
        img = GetComponent<RawImage>();
    }

    public void IniciarScroll() => activo = true;
    public void DetenerScroll() => activo = false;

    void Update()
    {
        if (!activo) return;

        offset += direccion.normalized * velocidad * Time.deltaTime;
        img.uvRect = new Rect(offset, img.uvRect.size);
    }
}