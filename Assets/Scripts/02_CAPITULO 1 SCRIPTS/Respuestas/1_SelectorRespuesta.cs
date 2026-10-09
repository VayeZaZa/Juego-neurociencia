using UnityEngine;

public class SelectorRespuesta : MonoBehaviour
{
public GameObject botonConfirmar;

public DialogoBienvenida dialogoBienvenida;

public int respuestaSeleccionada = 0;

void Start()
{
    botonConfirmar.SetActive(false);
}

    // Nuevas referencias a los checks que se activarán según la opción seleccionada
    public GameObject check1;
    public GameObject check2;
    public GameObject check3;

    public void SeleccionarRespuesta(int opcion)
    {
        respuestaSeleccionada = opcion;

        // Desactivamos todos los checks y activamos solo el correspondiente
        if (check1 != null) check1.SetActive(false);
        if (check2 != null) check2.SetActive(false);
        if (check3 != null) check3.SetActive(false);

        if (opcion == 1 && check1 != null) check1.SetActive(true);
        if (opcion == 2 && check2 != null) check2.SetActive(true);
        if (opcion == 3 && check3 != null) check3.SetActive(true);

        botonConfirmar.SetActive(true);

        Debug.Log("Respuesta elegida: " + opcion);
    }

    // Método añadido según solicitud del usuario
    public void ConfirmarRespuesta()
    {
        if (dialogoBienvenida != null)
        {
            dialogoBienvenida.MostrarRespuestaElegida(respuestaSeleccionada);
        }
    }

    // Added method to skip response and confirm automatically
    public void SkipRespuesta()
    {
        SeleccionarRespuesta(1);
        ConfirmarRespuesta();
    }
}