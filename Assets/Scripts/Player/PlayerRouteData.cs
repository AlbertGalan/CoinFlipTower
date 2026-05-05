using UnityEngine;

public class PlayerRouteData : MonoBehaviour
{
    [Header("Estado de la Ruleta")]
    public RouletteWheel.TipoElemento rutaAsignada;
    public bool tieneRutaAsignada = false;

    [Header("Inventario de Objetos")]
    public bool tieneObjetoFisico = false;
    public RouletteWheel.TipoElemento tipoObjetoRecogido;

    public void SetRuta(RouletteWheel.TipoElemento nuevaRuta)
    {
        if (!tieneRutaAsignada)
        {
            rutaAsignada = nuevaRuta;
            tieneRutaAsignada = true;
        }
    }

    // Nuevo método para cuando recogemos el objeto del cofre
    public void RecogerObjeto(RouletteWheel.TipoElemento tipo)
    {
        tieneObjetoFisico = true;
        tipoObjetoRecogido = tipo;
        Debug.Log("<color=cyan>Jugador:</color> He recogido el objeto de " + tipo);
    }
}