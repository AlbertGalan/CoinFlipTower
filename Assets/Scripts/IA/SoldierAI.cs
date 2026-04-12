using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SoldierAI : MonoBehaviour
{
    public enum State { Idle, Patrolling, Alerted, Chasing, Attacking }
    // Nueva enumeración para el tipo de patrulla
    public enum PatrolType { Both, OnlyGround, OnlyCeiling }

    [Header("Configuración de IA")]
    public State currentState = State.Patrolling;
    public PatrolType patrolType = PatrolType.Both; // Opción seleccionable en Inspector
    public Transform[] groundWaypoints;
    public Transform[] ceilingWaypoints;
    public float detectionRange = 10f;
    [Range(0, 180)] public float fieldOfViewAngle = 60f; 
    public LayerMask blockDetectionMask = ~0;
    public LayerMask obstructionMask; 
    public int waypointsBeforeGravitySwitch = 4;

    [Header("Configuració de Visió (Llum)")]
    public Light viewLight;
    public Color colorPatrol = Color.yellow;
    public Color colorAlert = new Color(1f, 0.5f, 0f);
    public Color colorCombat = Color.red;

    [Header("Configuració de Patrulla")]
    public float waitTimeAtWaypoint = 2f;
    public float gravityTransitionDuration = 5f;
    private float waitTimer = 0f;
    private int currentWaypointIndex = 0;
    private int visitedWaypointCount = 0;
    private bool switchSurfaceAfterIdle = false;

    [Header("Configuració de Combat")]
    public float attackRange = 4f;
    public float damagePerShot = 20f;
    public float fireRate = 0.5f;
    public Transform firePoint;
    public LineRenderer laserLine;
    public float laserDuration = 0.05f;
    private float nextFireTime = 0f;
    private DestructibleBlock currentTargetBlock;

    [Header("Configuració de Superficie")]
    public bool isCeilingSoldier = false;

    private NavMeshAgent agent;
    private Animator anim;
    private bool isTransitioningSurface = false;
    private State lastAnimationState;

    private float detectionRangeSqr;
    private float attackRangeSqr;
    private WaitForSeconds laserWait;
    private float blockSearchInterval = 0.2f;
    private float blockSearchTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        detectionRangeSqr = detectionRange * detectionRange;
        attackRangeSqr = attackRange * attackRange;
        laserWait = new WaitForSeconds(laserDuration);

        if (laserLine != null)
        {
            laserLine.enabled = false;
            laserLine.positionCount = 2;
            laserLine.useWorldSpace = true;
        }

        SetupLight();
    }

    void SetupLight()
    {
        if (viewLight != null)
        {
            viewLight.type = LightType.Spot;
            viewLight.range = detectionRange;
            viewLight.spotAngle = fieldOfViewAngle;
            viewLight.color = colorPatrol;
        }
    }

    void Start()
    {
        // Forzar estado inicial según el tipo de patrulla elegido
        if (patrolType == PatrolType.OnlyCeiling) isCeilingSoldier = true;
        if (patrolType == PatrolType.OnlyGround) isCeilingSoldier = false;

        if (isCeilingSoldier) 
            agent.updateUpAxis = false;
        else 
            agent.updateUpAxis = true;

        lastAnimationState = currentState;
        FireAnimationTriggerForState(currentState);
        GoToNextWaypoint();
    }

    void Update()
    {
        if (isTransitioningSurface) return;

        if (currentState == State.Patrolling || currentState == State.Idle)
        {
            blockSearchTimer -= Time.deltaTime;
            if (blockSearchTimer <= 0f)
            {
                blockSearchTimer = blockSearchInterval;
                DestructibleBlock found = FindVisibleBlock();
                if (found != null)
                {
                    currentTargetBlock = found;
                    StartCoroutine(AlertSequence());
                }
            }
        }

        switch (currentState)
        {
            case State.Patrolling: UpdatePatrol();   break;
            case State.Idle:       UpdateIdle();     break;
            case State.Chasing:    UpdateChasing();  break;
            case State.Attacking:  UpdateAttack();   break;
        }

        UpdateAnimations();
    }

    DestructibleBlock FindVisibleBlock()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, detectionRange, blockDetectionMask);
        foreach (var col in nearby)
        {
            DestructibleBlock block = col.GetComponentInParent<DestructibleBlock>() ?? col.GetComponentInChildren<DestructibleBlock>();
            if (block == null) continue;

            Vector3 dirToBlock = (block.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToBlock) < fieldOfViewAngle / 2f)
            {
                float distToBlock = Vector3.Distance(transform.position, block.transform.position);
                if (!Physics.Raycast(transform.position + transform.up * 0.5f, dirToBlock, distToBlock, obstructionMask))
                {
                    return block;
                }
            }
        }
        return null;
    }

    IEnumerator AlertSequence()
    {
        currentState = State.Alerted;
        agent.isStopped = true;
        if (viewLight != null) viewLight.color = colorAlert;
        FireAnimationTriggerForState(State.Idle);
        yield return new WaitForSeconds(1.0f); 

        if (currentTargetBlock != null)
        {
            if (viewLight != null) viewLight.color = colorCombat;
            currentState = State.Chasing;
            agent.isStopped = false;
        }
        else
        {
            ReturnToPatrol();
        }
    }

    void UpdatePatrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            visitedWaypointCount++;
            // Solo intentamos cambiar de superficie si el tipo es 'Both'
            switchSurfaceAfterIdle = (patrolType == PatrolType.Both) && ShouldSwitchPatrolSurface();
            
            currentState = State.Idle;
            waitTimer = waitTimeAtWaypoint;
        }
    }

    void UpdateIdle()
    {
        Transform[] activeWaypoints = GetActiveWaypoints();
        if (activeWaypoints.Length == 0) return;

        agent.isStopped = true;
        Quaternion targetRotation = activeWaypoints[currentWaypointIndex].rotation;
        Quaternion finalRotation = isCeilingSoldier
            ? Quaternion.Euler(0f, targetRotation.eulerAngles.y, 180f)
            : targetRotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * 5f);

        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        if (switchSurfaceAfterIdle)
        {
            SwitchPatrolSurface();
            return;
        }

        currentWaypointIndex = (currentWaypointIndex + 1) % activeWaypoints.Length;
        agent.isStopped = false;
        currentState = State.Patrolling;
        GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        Transform[] activeWaypoints = GetActiveWaypoints();
        if (activeWaypoints.Length == 0) return;
        if (currentWaypointIndex >= activeWaypoints.Length) currentWaypointIndex = 0;
        agent.SetDestination(activeWaypoints[currentWaypointIndex].position);
    }

    Transform[] GetActiveWaypoints() =>
        (isCeilingSoldier ? ceilingWaypoints : groundWaypoints) ?? System.Array.Empty<Transform>();

    bool ShouldSwitchPatrolSurface() =>
        waypointsBeforeGravitySwitch > 0 && visitedWaypointCount >= waypointsBeforeGravitySwitch;

    void SwitchPatrolSurface()
    {
        if (isTransitioningSurface) return;
        visitedWaypointCount = 0;
        switchSurfaceAfterIdle = false;
        isCeilingSoldier = !isCeilingSoldier;
        currentWaypointIndex = 0;
        StartCoroutine(HandleSurfaceTransition());
    }

    IEnumerator HandleSurfaceTransition()
    {
        isTransitioningSurface = true;
        currentState = State.Idle;
        Transform[] activeWaypoints = GetActiveWaypoints();
        if (activeWaypoints.Length == 0) { isTransitioningSurface = false; yield break; }

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = activeWaypoints[0].position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = isCeilingSoldier
            ? Quaternion.Euler(0f, activeWaypoints[0].rotation.eulerAngles.y, 180f)
            : activeWaypoints[0].rotation;

        if (agent != null) { agent.isStopped = true; agent.enabled = false; }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, gravityTransitionDuration);
        while (elapsed < duration)
        {
            float easedT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        if (agent != null)
        {
            agent.enabled = true;
            agent.updateUpAxis = !isCeilingSoldier;
            if (agent.isOnNavMesh) agent.Warp(targetPosition);
        }

        agent.isStopped = false;
        currentState = State.Patrolling;
        isTransitioningSurface = false;
        GoToNextWaypoint();
    }

    void UpdateChasing()
    {
        if (currentTargetBlock != null && currentTargetBlock.isPlaced) 
{
    ReturnToPatrol(); // El soldado lo ignora porque ya está "protegido" en el slot
    return;
}
        if (currentTargetBlock == null) { ReturnToPatrol(); return; }
        float sqrDist = (currentTargetBlock.transform.position - transform.position).sqrMagnitude;
        if (sqrDist > detectionRangeSqr) { ReturnToPatrol(); return; }
        if (sqrDist <= attackRangeSqr) { currentState = State.Attacking; agent.isStopped = true; return; }
        agent.isStopped = false;
        agent.SetDestination(currentTargetBlock.transform.position);
    }

    void UpdateAttack()
    {
        if (currentTargetBlock != null && currentTargetBlock.isPlaced) 
{
    ReturnToPatrol(); // El soldado lo ignora porque ya está "protegido" en el slot
    return;
}
        if (currentTargetBlock == null) { ReturnToPatrol(); return; }
        float sqrDist = (currentTargetBlock.transform.position - transform.position).sqrMagnitude;
        Vector3 dir = (currentTargetBlock.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position + transform.up * 0.5f, dir, Vector3.Distance(transform.position, currentTargetBlock.transform.position), obstructionMask))
        {
            currentState = State.Chasing;
            agent.isStopped = false;
            return;
        }

        if (sqrDist > attackRangeSqr) { currentState = State.Chasing; agent.isStopped = false; return; }

        agent.isStopped = true;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, transform.up), Time.deltaTime * 5f);
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (currentTargetBlock == null) return;
        if (laserLine != null && firePoint != null) StartCoroutine(ShowLaserBeam());
        currentTargetBlock.TakeDamage(damagePerShot);
    }

    void ReturnToPatrol()
    {
        if (viewLight != null) viewLight.color = colorPatrol;
        currentTargetBlock = null;
        currentState = State.Patrolling;
        agent.isStopped = false;
        GoToNextWaypoint();
    }

    IEnumerator ShowLaserBeam()
    {
        laserLine.enabled = true;
        if (firePoint != null && currentTargetBlock != null)
        {
            laserLine.SetPosition(0, firePoint.position);
            laserLine.SetPosition(1, currentTargetBlock.transform.position);
        }
        yield return laserWait;
        laserLine.enabled = false;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;
        if (currentState != State.Alerted && agent.enabled && !agent.isStopped && agent.velocity.sqrMagnitude > 0.1f)
        {
            State moveState = (currentState == State.Chasing) ? State.Chasing : State.Patrolling;
            if (lastAnimationState != moveState)
            {
                FireAnimationTriggerForState(moveState);
                lastAnimationState = moveState;
            }
            return;
        }
        if (currentState == lastAnimationState) return;
        FireAnimationTriggerForState(currentState);
        lastAnimationState = currentState;
    }

    void FireAnimationTriggerForState(State state)
    {
        anim.ResetTrigger("isWalking");
        anim.ResetTrigger("isIdle");
        anim.ResetTrigger("isRunning");
        anim.ResetTrigger("isShooting");
        switch (state)
        {
            case State.Patrolling: anim.SetTrigger("isWalking");  break;
            case State.Idle:       anim.SetTrigger("isIdle");     break;
            case State.Alerted:    anim.SetTrigger("isIdle");     break;
            case State.Chasing:    anim.SetTrigger("isRunning");  break;
            case State.Attacking:  anim.SetTrigger("isShooting"); break;
        }
    }

    private void OnDrawGizmosSelected()
    {
         Gizmos.color = Color.yellow;
         Gizmos.DrawWireSphere(transform.position, detectionRange);
         Vector3 forward = transform.forward;
         Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * forward;
         Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * forward;
         Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRange);
         Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRange);
    }
}