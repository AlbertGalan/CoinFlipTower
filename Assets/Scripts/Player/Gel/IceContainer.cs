using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class IceContainer : MonoBehaviour
{
    public bool estaLleno = false;
    public string tagBloque = "IceBlock";
    
    [Header("Referencias de Piezas")]
    [Tooltip("Arrastra aquí el objeto PADRE que contiene todas las piezas del contenedor para que bajen juntas.")]
    public GameObject contenedorPadre;
    
    [Tooltip("El objeto que emergerá del suelo (ej. un pilar o interruptor).")]
    public GameObject objetoQueSube;

    [Header("Ajustes de Movimiento")]
    public Vector3 offsetBajada = new Vector3(0, -0.5f, 0);
    public Vector3 offsetSubida = new Vector3(0, 1.0f, 0);
    public float velocidadMovimiento = 2f;

    [HideInInspector] public UnityEvent onBlockEntered = new UnityEvent();

    private void Start()
    {
        // Failsafe: Si olvidas asignar el padre, usamos este mismo objeto
        if (contenedorPadre == null) contenedorPadre = this.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya está lleno o no es el bloque de hielo, ignoramos
        if (estaLleno || !other.CompareTag(tagBloque)) return;

        estaLleno = true;
        
        // Eliminamos el bloque de hielo al entrar
        Destroy(other.gameObject); 

        // Iniciamos la secuencia de movimiento de piezas
        StartCoroutine(SecuenciaMovimiento());
        
        onBlockEntered.Invoke();
        Debug.Log($"Mecanismo activado: {contenedorPadre.name} baja y {objetoQueSube?.name} sube.");
    }

    IEnumerator SecuenciaMovimiento()
    {
        // --- Configuración Contenedor (BAJA) ---
        Vector3 posInicialContenedor = contenedorPadre.transform.position;
        Vector3 posFinalContenedor = posInicialContenedor + offsetBajada;

        // --- Configuración Objeto Recompensa (SUBE) ---
        Vector3 posInicialObjeto = Vector3.zero;
        Vector3 posFinalObjeto = Vector3.zero;

        if (objetoQueSube != null)
        {
            posInicialObjeto = objetoQueSube.transform.position;
            posFinalObjeto = posInicialObjeto + offsetSubida;
        }

        float tiempo = 0;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * velocidadMovimiento;

            // Movemos el conjunto del contenedor (toda la pieza visual)
            contenedorPadre.transform.position = Vector3.Lerp(posInicialContenedor, posFinalContenedor, tiempo);

            // Movemos el objeto que emerge del suelo
            if (objetoQueSube != null)
            {
                objetoQueSube.transform.position = Vector3.Lerp(posInicialObjeto, posFinalObjeto, tiempo);
            }

            yield return null;
        }
        
        // Aseguramos precisión en las posiciones finales
        contenedorPadre.transform.position = posFinalContenedor;
        if (objetoQueSube != null) objetoQueSube.transform.position = posFinalObjeto;
    }
}