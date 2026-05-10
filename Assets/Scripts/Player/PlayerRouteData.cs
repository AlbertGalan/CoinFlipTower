using UnityEngine;
using UnityEngine.Events; // Necesario para usar UnityEvent

public class PlayerRouteData : MonoBehaviour
{
    [Header("Estado de la Ruleta")]
    public RouletteWheel.TipoElemento rutaAsignada;
    public bool tieneRutaAsignada = false;

    [Header("Eventos")]
    [Tooltip("Eventos que se ejecutarán en el momento exacto de asignar una ruta")]
    public UnityEvent onRutaAsignada;

    [Header("Referencias UI de Rutas (Imágenes)")]
    [Tooltip("Arrastra aquí los GameObjects del panel de pausa correspondientes a cada ruta")]
    public GameObject imatgeRutaFoc;
    public GameObject imatgeRutaGel;
    public GameObject imatgeRutaTerra;

    [Header("Inventario de Objetos")]
    public bool tieneObjetoFisico = false;
    public RouletteWheel.TipoElemento tipoObjetoRecogido;
private void Start()
{
    // Solo cargamos si venimos de una transición (puedes ajustar esta lógica)
    CargarRutaPersistente();
}

public void CargarRutaPersistente()
{
    if (PlayerPrefs.GetInt("SavedHasRoute", 0) == 1)
    {
        int rutaIndex = PlayerPrefs.GetInt("SavedRouteType", 0);
        rutaAsignada = (RouletteWheel.TipoElemento)rutaIndex;
        tieneRutaAsignada = true;

        ActualizarImagenRutaUI();
        
        Debug.Log("<color=green>Ruta Recuperada:</color> " + rutaAsignada);

        // --- LA CLAVE ESTÁ AQUÍ ---
        // Forzamos el disparo del evento para que las puertas que escuchan se enteren
        if (onRutaAsignada != null)
        {
            Debug.Log("Disparando evento onRutaAsignada por carga persistente...");
            onRutaAsignada.Invoke();
        }
    }
}
    public void SetRuta(RouletteWheel.TipoElemento nuevaRuta)
    {
        if (!tieneRutaAsignada)
        {
            rutaAsignada = nuevaRuta;
            tieneRutaAsignada = true;
            Debug.Log("<color=green>Ruta Asignada:</color> " + nuevaRuta);

            // --- DISPARO DEL EVENTO ---
            // Invocamos cualquier función conectada desde el Inspector
            if (onRutaAsignada != null)
            {
                onRutaAsignada.Invoke();
            }
        }
    }

    // Método para activar la imagen correcta en el panel de pausa
    public void ActualizarImagenRutaUI()
    {
        // 1. Limpieza: Desactivamos las tres imágenes primero
        if (imatgeRutaFoc) imatgeRutaFoc.SetActive(false);
        if (imatgeRutaGel) imatgeRutaGel.SetActive(false);
        if (imatgeRutaTerra) imatgeRutaTerra.SetActive(false);

        // Si aún no hay ruta, no activamos nada
        if (!tieneRutaAsignada) return;

        // 2. Activamos la imagen que toca según el Enum
        switch (rutaAsignada)
        {
            case RouletteWheel.TipoElemento.Foc:
                if (imatgeRutaFoc) imatgeRutaFoc.SetActive(true);
                break;
            case RouletteWheel.TipoElemento.Gel:
                if (imatgeRutaGel) imatgeRutaGel.SetActive(true);
                break;
            case RouletteWheel.TipoElemento.Terra:
                if (imatgeRutaTerra) imatgeRutaTerra.SetActive(true);
                break;
        }
    }

    public void RecogerObjeto(RouletteWheel.TipoElemento tipo)
    {
        tieneObjetoFisico = true;
        tipoObjetoRecogido = tipo;
        Debug.Log("<color=cyan>Jugador:</color> He recogido el objeto de " + tipo);
    }
}