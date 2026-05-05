using UnityEngine;

public class ElementAltar : MonoBehaviour
{
    public RouletteWheel.TipoElemento tipoAltar;
    public float interactionDistance = 3f;
    
    [Header("Referencias a Desbloquear")]
    public GameObject objectBehindDoor; 
    public Animator doorAnimator;      
    public string boolName = "isOpened"; 

    [Header("Visual del Objeto en Altar")]
    [Tooltip("El objeto que ya está posicionado sobre el altar.")]
    public GameObject visualItemOnAltar; 
    
    [Header("Ajustes de Animación")]
    public Vector3 rotationSpeed = new Vector3(0, 120, 0);
    public float floatAmplitude = 0.15f;
    public float floatSpeed = 1.5f;

    private bool activated = false;
    private Vector3 initialPos;
    private float offset;

    void Start()
    {
        // 1. Aseguramos que el objeto esté desactivado al inicio
        if (visualItemOnAltar != null)
        {
            visualItemOnAltar.SetActive(false);
            // Guardamos la posición exacta donde TÚ lo pusiste
            initialPos = visualItemOnAltar.transform.position;
        }
        
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Interacción con E
        if (Input.GetKeyDown(KeyCode.E) && !activated)
        {
            if (IsPlayerClose()) TryActivateAltar();
        }

        // 2. Si está activado, el objeto gira y flota en su sitio
        if (activated && visualItemOnAltar != null)
        {
            AnimateObject();
        }
    }

    private void AnimateObject()
    {
        // Rotación
        visualItemOnAltar.transform.Rotate(rotationSpeed * Time.deltaTime);

        // Flotación (respetando la posición original del editor)
        float newY = initialPos.y + Mathf.Sin(Time.time * floatSpeed + offset) * floatAmplitude;
        visualItemOnAltar.transform.position = new Vector3(initialPos.x, newY, initialPos.z);
    }

    private bool IsPlayerClose()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= interactionDistance;
    }

    private void TryActivateAltar()
    {
        PlayerRouteData routeData = FindFirstObjectByType<PlayerRouteData>();

        // Solo se activa si el jugador TIENE físicamente el objeto que coincide con este altar
        if (routeData != null && routeData.tieneObjetoFisico && routeData.tipoObjetoRecogido == tipoAltar)
        {
            ActivateSequence();
            routeData.tieneObjetoFisico = false; // "Entregamos" el objeto
        }
        else
        {
            Debug.Log($"<color=orange>Altar {tipoAltar}:</color> No tienes el objeto necesario.");
        }
    }

    private void ActivateSequence()
    {
        activated = true;

        // 3. Simplemente lo activamos. Como ya está posicionado, aparecerá donde debe.
        if (visualItemOnAltar != null)
        {
            visualItemOnAltar.SetActive(true);
        }

        if (objectBehindDoor != null) objectBehindDoor.SetActive(true);
        if (doorAnimator != null) doorAnimator.SetBool(boolName, true);
    }
}