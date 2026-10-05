using System.Collections;
using System.Collections.Generic;
using BitWave_Labs.AnimatedTextReveal;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestiona el sistema de tutorial del joc, controlant la visualització de missatges,
/// pausa del joc i posició del guardià.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    [Header("Referències UI")]
    [Tooltip("Panel que conté tot el UI del tutorial")]
    public GameObject tutorialPanel;
    
    [Tooltip("TextMeshPro on es mostra el nom del personatge que parla")]
    public TextMeshProUGUI characterNameText;
    
    [Tooltip("Component TextMeshPro on es mostra el text")]
    public TextMeshProUGUI tutorialText;
    
        [Header("Fletxa")]
    [Tooltip("Objecte de la fletxa animada del diàleg")]
    public GameObject dialogueArrowObject;

    [Tooltip("Animator de la fletxa (opcional)")]
    public Animator dialogueArrowAnimator;

    [Tooltip("Trigger per forçar l'animació de la fletxa en iniciar diàleg")]
    public string dialogueArrowAnimationTrigger = "isOpen";

    [Header("Guardià")]
    [Tooltip("Transform del guardià que es mou durant els missatges")]
    public Transform guardianTransform;
    
    [Tooltip("GameObject del guardià per mostrar/amagar")]
    public GameObject guardianObject;
    
    [Tooltip("Animator del guardià per controlar animacions")]
    public Animator guardianAnimator;
    
    [Tooltip("Posició per defecte del guardià quan no hi ha missatge")]
    public Vector3 guardianDefaultPosition = new Vector3(-10, 0, 0);
    
    [Header("Configuració")]
    [Tooltip("Aplicar pausa artificial per defecte quan el missatge no té override")]
    public bool pausaArtificialDefault = true;
    
    [Tooltip("Objectes que es desactiven durant el tutorial (ex: HUD, target...)")]
    public List<GameObject> objectsToHideDuringTutorial = new List<GameObject>();
    
    [Header("Esdeveniments")]
    public UnityEvent OnTutorialMessageStart;
    public UnityEvent OnTutorialMessageEnd;
    
    // Control de estat
    private bool isShowingMessage = false;
    private Coroutine currentMessageCoroutine;
    private HashSet<string> shownMessages = new HashSet<string>();
    private Camera mainCamera;
    private AnimatedTextReveal textAnimator;
    private bool currentMessageUsedPausaArtificial = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        mainCamera = Camera.main;
        
        // Auto-asignar Animator si no està assignat
        if (guardianAnimator == null && guardianObject != null)
        {
            guardianAnimator = guardianObject.GetComponent<Animator>();
        }
    }
    
    private void Start()
    {
        // Inicialitzar estat
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        if (guardianObject != null)
        {
            guardianObject.SetActive(false);
        }

        // Amagar el nom del personatge si existeix
        if (characterNameText != null)
        {
            characterNameText.gameObject.SetActive(false);
            characterNameText.alignment = TextAlignmentOptions.Center;
        }
        
        // Auto-asignar components si no estan assignats
              if (dialogueArrowAnimator == null && dialogueArrowObject != null)
        {
            dialogueArrowAnimator = dialogueArrowObject.GetComponent<Animator>();
        }

        if (dialogueArrowObject != null)
        {
            dialogueArrowObject.SetActive(false);
        }
        // Centrar text del diàleg per defecte
        if (tutorialText != null)
        {
            tutorialText.alignment = TextAlignmentOptions.Center;
        }
        
        // Validar referències
        if (textAnimator == null && tutorialText != null)
        {
            textAnimator = tutorialText.GetComponent<AnimatedTextReveal>();
        }
        
        if (guardianAnimator == null && guardianObject != null)
        {
            guardianAnimator = guardianObject.GetComponent<Animator>();
        }
    }
    
    /// <summary>
    /// Mostra un missatge del tutorial.
    /// </summary>
    /// <param name="message">El TutorialMessage a mostrar</param>
    /// <param name="forceShow">Si true, mostra el missatge encara que ja s'hagi mostrat abans</param>
    /// <param name="pausaArtificialOverride">Si té valor, indica si s'ha d'aplicar pausa artificial; si és null usa configuració del missatge o per defecte</param>
    public void ShowMessage(TutorialMessage message, bool forceShow = false, bool? pausaArtificialOverride = null)
    {
        if (message == null)
        {
            Debug.LogWarning("TutorialManager: Intentant mostrar un missatge null");
            return;
        }
        
        // Comprovar si ja s'ha mostrat
        if (!forceShow && shownMessages.Contains(message.messageId))
        {
            Debug.Log($"TutorialManager: Missatge '{message.messageId}' ja mostrat prèviament");
            return;
        }
        
        // Si ja s'està mostrant un missatge, aturar-lo
        if (isShowingMessage && currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            EndMessage(currentMessageUsedPausaArtificial);
        }
        
        // Determinar si s'ha d'aplicar pausa artificial
        bool shouldApplyPausaArtificial = pausaArtificialOverride.HasValue
            ? pausaArtificialOverride.Value
            : (message != null ? message.pausaArtificial : pausaArtificialDefault);

        // Iniciar nou missatge
        currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(message, shouldApplyPausaArtificial));
    }

    private IEnumerator ShowMessageCoroutine(TutorialMessage message, bool applyPausaArtificial)
    {
        isShowingMessage = true;
        shownMessages.Add(message.messageId);

        currentMessageUsedPausaArtificial = applyPausaArtificial;

        // Aplicar pausa artificial (bloquear movimiento y score)
        if (applyPausaArtificial)
        {
            MoveCharacter playerController = FindFirstObjectByType<MoveCharacter>();
            if (playerController != null)
            {
                playerController.SetBlockMovement(true);
            }

            Score scoreScript = FindFirstObjectByType<Score>();
            if (scoreScript != null)
            {
                scoreScript.FreezeGameplay();
            }
        }
        
                if (dialogueArrowObject != null)
        {
            dialogueArrowObject.SetActive(true);
            if (dialogueArrowAnimator != null && !string.IsNullOrEmpty(dialogueArrowAnimationTrigger))
            {
                dialogueArrowAnimator.ResetTrigger(dialogueArrowAnimationTrigger);
                dialogueArrowAnimator.SetTrigger(dialogueArrowAnimationTrigger);
            }
        }

        // Amagar objectes especificats
        foreach (GameObject obj in objectsToHideDuringTutorial)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        // Activar panel del tutorial
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        // Mostrar camp de nom si escau
        if (characterNameText != null)
        {
            characterNameText.gameObject.SetActive(false);
        }
        
        // Posicionar i mostrar guardià
        if (message.showGuardian && guardianTransform != null && guardianObject != null)
        {
            Vector3 targetPosition = message.guardianPosition;
            
            // Si és relatiu a càmera, calcular posició world
            if (message.guardianRelativeToCamera && mainCamera != null)
            {
                targetPosition = mainCamera.transform.position + mainCamera.transform.TransformDirection(message.guardianPosition);
            }
            
            // Aplicar transformació completa
            guardianTransform.position = targetPosition;
            guardianTransform.rotation = Quaternion.Euler(message.guardianRotation);
            guardianTransform.localScale = message.guardianScale;
            
            // Activar animació si està especificada
            if (guardianAnimator != null && !string.IsNullOrEmpty(message.guardianAnimationTrigger))
            {
                guardianAnimator.SetTrigger(message.guardianAnimationTrigger);
            }
            
            guardianObject.SetActive(true);
        }
        
        // Disparar esdeveniment
        OnTutorialMessageStart?.Invoke();
        
        // Mostrar cada línia de text
        for (int i = 0; i < message.lines.Count; i++)
        {
            TutorialLine line = message.lines[i];
            bool isLastLine = (i == message.lines.Count - 1);
            
            // Actualitzar nom del personatge
            if (characterNameText != null)
            {
                if (!string.IsNullOrEmpty(line.characterName))
                {
                    characterNameText.text = line.characterName;
                    characterNameText.gameObject.SetActive(true);
                }
                else
                {
                    characterNameText.gameObject.SetActive(false);
                }
            }
            
            // Actualitzar text
            if (tutorialText != null)
            {
                tutorialText.text = line.text;
                // CRITICAL: Force mesh update after changing text
                tutorialText.ForceMeshUpdate();
            }
            
            // Actualitzar guardià si aquesta línia té configuració específica
            if (message.showGuardian && guardianObject != null && guardianObject.activeSelf)
            {
                // Animació
                if (!string.IsNullOrEmpty(line.guardianAnimationTrigger) && guardianAnimator != null)
                {
                    guardianAnimator.SetTrigger(line.guardianAnimationTrigger);
                }
                
                // Posició override
                if (line.overrideGuardianPosition && guardianTransform != null)
                {
                    Vector3 newPosition = line.guardianPosition;
                    if (message.guardianRelativeToCamera && mainCamera != null)
                    {
                        newPosition = mainCamera.transform.position + mainCamera.transform.TransformDirection(line.guardianPosition);
                    }
                    guardianTransform.position = newPosition;
                }
                
                // Rotació override
                if (line.overrideGuardianRotation && guardianTransform != null)
                {
                    guardianTransform.rotation = Quaternion.Euler(line.guardianRotation);
                }
            }
            
            // Esperar un frame per assegurar que el mesh s'ha actualitzat
            yield return null;
            
            // Fade in
            if (textAnimator != null)
            {
                textAnimator.SetAllCharactersAlpha(0);
                yield return StartCoroutine(textAnimator.FadeText(true));
            }
            else
            {
                // Si no hi ha animador, mostrar text directament
                if (tutorialText != null)
                {
                    tutorialText.alpha = 1f;
                }
            }
            
            // Esperar input del jugador o tiempo según configuración
            if (message.playerControlledAdvance)
            {
                // Modo: El jugador controla l'avanç
                bool playerAdvanced = false;
                while (!playerAdvanced)
                {
                    // Comprovar si es vol avançar
                    if (Input.GetKeyDown(message.advanceKey))
                    {
                        playerAdvanced = true;
                    }
                    
                    // Comprovar si es vol saltar
                    if (message.canSkip && Input.GetKeyDown(message.skipKey))
                    {
                        // Saltar a l'última línia si no hi som ja
                        if (!isLastLine)
                        {
                            i = message.lines.Count - 2; // -2 perquè el for farà +1
                            break;
                        }
                        else
                        {
                            // Si ja som a l'última, acabar
                            playerAdvanced = true;
                            break;
                        }
                    }
                    
                    yield return null;
                }
            }
            else
            {
                // Modo: Automàtic (comportament original)
                float timeWaited = 0f;
                while (timeWaited < message.lineDisplayTime)
                {
                    // Comprovar si es vol saltar
                    if (message.canSkip && Input.GetKeyDown(message.skipKey))
                    {
                        // Saltar a l'última línia si no hi som ja
                        if (!isLastLine)
                        {
                            i = message.lines.Count - 2; // -2 perquè el for farà +1
                            break;
                        }
                        else
                        {
                            // Si ja som a l'última, acabar
                            timeWaited = message.lineDisplayTime;
                            break;
                        }
                    }
                    
                    yield return null;
                    timeWaited += Time.deltaTime;
                }
            }
            
            // Fade out si no és l'última línia o si està configurat per fer-ho
            if (!isLastLine || message.fadeOutLastLine)
            {
                if (textAnimator != null)
                {
                    yield return StartCoroutine(textAnimator.FadeText(false));
                }
                else if (tutorialText != null)
                {
                    tutorialText.alpha = 0f;
                }
                
                // CRITICAL: Netejar el text després del fade-out per evitar veure'l
                if (tutorialText != null && !isLastLine)
                {
                    tutorialText.text = "";
                }
                
                // Esperar entre línies
                if (!isLastLine)
                {
                    yield return new WaitForSeconds(message.timeBetweenLines);
                }
            }
        }
        
        // Acabar missatge
        EndMessage(currentMessageUsedPausaArtificial);
    }

    private void EndMessage(bool hadPausaArtificial = false)
    {
        isShowingMessage = false;

        if (dialogueArrowObject != null)
        {
            dialogueArrowObject.SetActive(false);
        }

        // Quitar pausa artificial
        if (hadPausaArtificial)
        {
            MoveCharacter playerController = FindFirstObjectByType<MoveCharacter>();
            if (playerController != null)
            {
                playerController.SetBlockMovement(false);
            }

            Score scoreScript = FindFirstObjectByType<Score>();
            if (scoreScript != null)
            {
                scoreScript.UnfreezeGameplay();
            }
        }
        
        // Desactivar panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // Amagar guardià
        if (guardianObject != null)
        {
            guardianObject.SetActive(false);
        }
        
        // Restaurar objectes amagats
        foreach (GameObject obj in objectsToHideDuringTutorial)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        
        // Disparar esdeveniment
        OnTutorialMessageEnd?.Invoke();
    }
    
    /// <summary>
    /// Comprova si un missatge ja s'ha mostrat.
    /// </summary>
    public bool HasShownMessage(string messageId)
    {
        return shownMessages.Contains(messageId);
    }
    
    /// <summary>
    /// Comprova si el TutorialManager està mostrant un missatge en aquest moment.
    /// </summary>
    public bool IsShowingMessage()
    {
        return isShowingMessage;
    }
    
    /// <summary>
    /// Reinicia el tracking de missatges mostrats (útil per reinicis/debug).
    /// </summary>
    public void ResetShownMessages()
    {
        shownMessages.Clear();
    }
    
    /// <summary>
    /// Atura el missatge actual si n'hi ha un mostrant-se.
    /// </summary>
    public void StopCurrentMessage()
    {
        if (isShowingMessage && currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            EndMessage(currentMessageUsedPausaArtificial);
        }
    }
}