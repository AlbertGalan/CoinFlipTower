using UnityEngine;

/// <summary>
/// Componente que hace que un objeto se desmorone o caiga cuando es agarrado por el jugador.
/// Simplemente agrega este componente a cualquier objeto que quieras que sea derribable.
/// </summary>
 [RequireComponent(typeof(Rigidbody))]
 public class DestructibleOnGrab : MonoBehaviour
 {
     [Header("Comportamiento al agarrar")]
     [Tooltip("Tiempo tras agarrar antes de iniciar la caída")]
     public float collapseDelay = 0.25f;
     [Tooltip("Impulso aplicado para derribar")]
     public float collapseForce = 50f;
     [Tooltip("Altura del punto de aplicación de la fuerza para provocar vuelco")]
     public float forceHeight = 1.0f;
     [Tooltip("Factor de peso extra hacia abajo")]
     public float extraDownFactor = 0.35f;

     [Header("Fragmentación (Opcional)")]
     [Tooltip("Si true, el objeto se fragmentará en piezas")]
     public bool fragmentOnCollapse = false;
     [Tooltip("Prefab de fragmentos a instanciar (opcional)")]
     public GameObject fragmentPrefab;

     private Rigidbody rb;
     private bool hasCollapsed = false;
     private bool allowDestroyOnImpact = false;
     private Vector3 pushDirection = Vector3.forward;
     private FixedJoint jointToBreak;

     void Start()
     {
         rb = GetComponent<Rigidbody>();
     }

     // Llamado desde PushPullController al agarrar con intención de derribar
     public void TriggerCollapse(Vector3 pushDir, FixedJoint joint)
     {
         if (hasCollapsed) return;
         SetPushDirection(pushDir);
         jointToBreak = joint;
         StartCoroutine(Collapse());
     }

     private System.Collections.IEnumerator Collapse()
     {
         hasCollapsed = true;

         // Breve tiempo para que se vea el empuje
         yield return new WaitForSeconds(collapseDelay);

         // Soltar el joint para que el objeto caiga solo
         if (jointToBreak != null)
         {
             Destroy(jointToBreak);
             jointToBreak = null;
         }

         if (rb != null)
         {
             rb.isKinematic = false;

             // Combinar empuje horizontal + peso hacia abajo
             Vector3 forceDir = (pushDirection + Vector3.down * extraDownFactor).normalized;
             Vector3 forcePoint = rb.worldCenterOfMass + Vector3.up * forceHeight;

             rb.AddForceAtPosition(forceDir * collapseForce, forcePoint, ForceMode.Impulse);
             rb.AddTorque(Random.insideUnitSphere * collapseForce * 0.15f, ForceMode.Impulse);
         }

         if (fragmentOnCollapse && fragmentPrefab != null)
         {
             CreateFragments();
         }

         allowDestroyOnImpact = true;
         Debug.Log($"Objeto '{gameObject.name}' derrumbado en dirección {pushDirection}");
     }

     private void CreateFragments()
     {
         int fragmentCount = Random.Range(3, 6);
         for (int i = 0; i < fragmentCount; i++)
         {
             Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
             Instantiate(fragmentPrefab, transform.position + randomOffset, Random.rotation);
         }
     }

     public void ResetDestructible()
     {
         hasCollapsed = false;
         allowDestroyOnImpact = false;
         jointToBreak = null;
     }

     public void SetPushDirection(Vector3 direction)
     {
         Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
         pushDirection = planar.sqrMagnitude > 0.001f ? planar.normalized : Vector3.forward;
     }

     private void OnCollisionEnter(Collision collision)
     {
         if (!allowDestroyOnImpact) return;

         if (fragmentOnCollapse && fragmentPrefab != null)
         {
             CreateFragments();
         }

         Destroy(gameObject);
     }
}
