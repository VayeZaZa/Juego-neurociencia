using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Se conecta al cargar el capitulo: no requiere arrastrar referencias en cada boton.
[DisallowMultipleComponent]
public sealed class PopCheckCapitulo1 : MonoBehaviour
{
    private const string Escena = "02_Capitulo_1";
    private const float Duracion = 0.22f;
    private static Sprite spriteAccion;
    private static Texture2D texturaAccion;
    private UnityEngine.UI.Button boton;
    private UnityEngine.UI.Button.ButtonClickedEvent accionOriginal;
    private GameObject check;
    private Vector3 escalaOriginal;
    private bool esperarAntesDeContinuar;
    private bool ocupado;
    private bool configurado;
    private Coroutine animacion;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Registrar()
    {
        if (spriteAccion != null) Destroy(spriteAccion);
        if (texturaAccion != null) Destroy(texturaAccion);
        spriteAccion = null;
        texturaAccion = null;
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name != Escena && escena.name != "03_MinijuegoRetirada" &&
            escena.name != "04_MinijuegoEcoCambio" && escena.name != "05_MinijuegoCompromiso") return;
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (UnityEngine.UI.Button candidato in raiz.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                bool continuar;
                if (!EsBotonDelRecorrido(candidato, out continuar)) continue;
                // Los checks copiados tambien pueden tener Button y el mismo OnClick.
                bool anidado = false;
                for (Transform padre = candidato.transform.parent; padre != null; padre = padre.parent)
                {
                    var botonPadre = padre.GetComponent<UnityEngine.UI.Button>();
                    bool ignorado;
                    if (padre.GetComponent<PopCheckCapitulo1>() != null ||
                        (botonPadre != null && EsBotonDelRecorrido(botonPadre, out ignorado)))
                    {
                        anidado = true;
                        break;
                    }
                }
                if (anidado || candidato.GetComponent<PopCheckCapitulo1>() != null) continue;
                GameObject visual = continuar ? CrearCheckAccion(candidato) : BuscarCheck(candidato.transform);
                if (visual == null)
                {
                    Debug.LogWarning("Falta el check visual en " + candidato.name, candidato);
                    continue;
                }
                candidato.gameObject.AddComponent<PopCheckCapitulo1>().Configurar(candidato, visual, continuar);
            }
        }
    }

    private static bool EsBotonDelRecorrido(UnityEngine.UI.Button candidato, out bool continuar)
    {
        continuar = false;
        bool seleccion = false;
        for (int i = 0; i < candidato.onClick.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object destino = candidato.onClick.GetPersistentTarget(i);
            string metodo = candidato.onClick.GetPersistentMethodName(i);
            if (destino is SelectorRespuesta || destino is DialogoRetirada ||
                destino is DialogoEcoCambio || destino is DialogoCompromiso)
            {
                seleccion |= metodo == "SeleccionarRespuesta" || metodo == "SeleccionarRespuesta2" ||
                             metodo == "SeleccionarPregunta";
                continuar |= metodo == "ContinuarRecorrido" || metodo == "QuieroIntentarlo";
            }
            if ((destino is MovimientoSimple || destino is MovimientoEcoCambio) && metodo == "Seguir")
                continuar = true;
            if (destino is GameManager && metodo == "Regresar") continuar = true;
            if ((destino is GameManagerEco || destino is RespiracionManager) && metodo == "DesbloquearPuerta")
                continuar = true;
            if (destino is RecompensaCompromiso && metodo == "RecibirRecompensa")
                continuar = true;
        }
        return seleccion || continuar;
    }

    private static GameObject BuscarCheck(Transform padre)
    {
        // Primero el hijo directo: algunos checks son el fondo verde completo del boton.
        foreach (Transform hijo in padre)
            if (hijo.name.StartsWith("check", StringComparison.OrdinalIgnoreCase)) return hijo.gameObject;
        return null;
    }

    private static GameObject CrearCheckAccion(UnityEngine.UI.Button origen)
    {
        if (spriteAccion == null)
        {
            // Copia del PNG como recurso binario: funciona tambien en la build.
            TextAsset png = Resources.Load<TextAsset>("Capitulo1/CheckRespuesta");
            if (png == null)
            {
                Debug.LogWarning("No se encontro el recurso Capitulo1/CheckRespuesta.", origen);
                return null;
            }
            texturaAccion = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texturaAccion, png.bytes, true))
            {
                Destroy(texturaAccion);
                texturaAccion = null;
                return null;
            }
            texturaAccion.wrapMode = TextureWrapMode.Clamp;
            texturaAccion.filterMode = FilterMode.Bilinear;
            spriteAccion = Sprite.Create(texturaAccion,
                new Rect(0, 0, texturaAccion.width, texturaAccion.height), new Vector2(0.5f, 0.5f), 100f);
        }
        // Los checks viejos permanecen guardados en la escena, ocultos durante Play.
        foreach (Transform hijo in origen.transform)
        {
            var imagen = hijo.GetComponent<UnityEngine.UI.Image>();
            if (hijo.name.StartsWith("check", StringComparison.OrdinalIgnoreCase) ||
                (imagen != null && imagen.sprite != null &&
                 imagen.sprite.name.Equals("CHECK RESPUESTA", StringComparison.OrdinalIgnoreCase)))
                hijo.gameObject.SetActive(false);
        }
        GameObject visual = new GameObject("CheckVerdePopAutomatico", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform rect = (RectTransform)visual.transform;
        rect.SetParent(origen.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        // Encima del fondo del boton, debajo de sus textos.
        rect.SetAsFirstSibling();
        var grafico = visual.GetComponent<UnityEngine.UI.Image>();
        grafico.sprite = spriteAccion;
        grafico.raycastTarget = false;
        grafico.color = Color.white;
        visual.SetActive(false);
        return visual;
    }

    private void Configurar(UnityEngine.UI.Button origen, GameObject visual, bool continuar)
    {
        boton = origen;
        check = visual;
        escalaOriginal = check.transform.localScale;
        esperarAntesDeContinuar = continuar;
        accionOriginal = boton.onClick;
        boton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        boton.onClick.AddListener(AlPulsar);
        // El padre recibe el clic; las capas visuales no ejecutan acciones duplicadas.
        foreach (var hijo in boton.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            if (hijo != boton) hijo.enabled = false;
        foreach (var grafico in boton.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            if (grafico.gameObject != boton.gameObject) grafico.raycastTarget = false;
        // Compromiso tiene un icono check oculto dentro del fondo de seleccion.
        foreach (Transform detalle in check.GetComponentsInChildren<Transform>(true))
            if (detalle != check.transform && detalle.name.StartsWith("check", StringComparison.OrdinalIgnoreCase))
                detalle.gameObject.SetActive(true);
        configurado = true;
        Restablecer();
    }

    private void OnEnable()
    {
        if (configurado) Restablecer();
    }

    private void OnDisable()
    {
        if (!configurado) return;
        if (animacion != null) StopCoroutine(animacion);
        animacion = null;
        if (ocupado) boton.interactable = true;
        ocupado = false;
        if (check != null) check.transform.localScale = escalaOriginal;
    }

    private void Restablecer()
    {
        check.transform.localScale = escalaOriginal;
        check.SetActive(false);
    }

    private void AlPulsar()
    {
        if (ocupado) return;
        if (animacion != null) StopCoroutine(animacion);
        if (esperarAntesDeContinuar)
        {
            ocupado = true;
            boton.interactable = false;
        }
        else
        {
            // Conserva la respuesta elegida, oculta las otras y habilita Confirmar.
            accionOriginal.Invoke();
        }
        if (!isActiveAndEnabled) return;
        animacion = StartCoroutine(Animar());
    }

    private IEnumerator Animar()
    {
        check.SetActive(true);
        check.transform.localScale = Vector3.zero;
        float duracionPop = esperarAntesDeContinuar ? 0.32f : Duracion;
        float tiempo = 0f;
        while (tiempo < duracionPop)
        {
            // Si se selecciona otra respuesta, su controlador oculta este check.
            if (!check.activeSelf) break;
            float progreso = Mathf.Clamp01(tiempo / duracionPop);
            float escala = progreso < 0.65f
                ? Mathf.Lerp(0f, 1.15f, progreso / 0.65f)
                : Mathf.Lerp(1.15f, 1f, (progreso - 0.65f) / 0.35f);
            check.transform.localScale = escalaOriginal * escala;
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        check.transform.localScale = escalaOriginal;
        if (esperarAntesDeContinuar)
        {
            // Dejar visible el verde un instante antes de caminar, entrar o recoger la llave.
            yield return new WaitForSecondsRealtime(0.28f);
        }
        animacion = null;
        if (esperarAntesDeContinuar)
        {
            // Solo despues del POP se oculta el panel y comienza el movimiento habitual.
            ocupado = false;
            boton.interactable = true;
            accionOriginal.Invoke();
        }
    }
}
