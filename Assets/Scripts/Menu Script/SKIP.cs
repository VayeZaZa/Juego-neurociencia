using UnityEngine;

public class BotonSkip : MonoBehaviour
{
    [Header("Referencias")]
    public DialogoBienvenida dialogoBienvenida;
    public MovimientoSimple movimientoSimple;

    [Header("Panels")]
    public GameObject panelDialogo;
    public GameObject panelRespuestas;

    public void Skip()
    {
        // Si están caminando
        if (movimientoSimple != null && movimientoSimple.EstaMoviendose())
        {
            Debug.Log("Skip: caminata");
            movimientoSimple.SkipMovimiento();
            return;
        }

        // Si está abierto el diálogo principal
        if (panelDialogo != null && panelDialogo.activeSelf)
        {
            Debug.Log("Skip: diálogo");
            dialogoBienvenida.SkipTodo();
            return;
        }

        // Si están las respuestas abiertas
        if (panelRespuestas != null && panelRespuestas.activeSelf)
        {
            Debug.Log("Skip: respuestas");
            dialogoBienvenida.SkipTodo();
            return;
        }
    }
}