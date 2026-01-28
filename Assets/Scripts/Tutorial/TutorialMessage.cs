using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define una línia individual del tutorial amb el seu text i configuració d'animació del guardià.
/// </summary>
[System.Serializable]
public class TutorialLine
{
    [Header("Text")]
    [Tooltip("Nom del personatge que parla (deixar buit per no mostrar nom)")]
    public string characterName = "";
    
    [TextArea(2, 5)]
    [Tooltip("Text d'aquesta línia")]
    public string text;
    
    [Header("Animació del Guardià (opcional)")]
    [Tooltip("Trigger d'animació a reproduir quan apareix aquesta línia (deixar buit per no canviar)")]
    public string guardianAnimationTrigger = "";
    
    [Tooltip("Canviar posició del guardià per aquesta línia (deixar desmarcat per mantenir posició)")]
    public bool overrideGuardianPosition = false;
    
    [Tooltip("Nova posició del guardià per aquesta línia")]
    public Vector3 guardianPosition = Vector3.zero;
    
    [Tooltip("Canviar rotació del guardià per aquesta línia (deixar desmarcat per mantenir rotació)")]
    public bool overrideGuardianRotation = false;
    
    [Tooltip("Nova rotació del guardià per aquesta línia")]
    public Vector3 guardianRotation = Vector3.zero;
}

/// <summary>
/// ScriptableObject que define un mensaje del tutorial con sus líneas de texto y configuración.
/// </summary>
[CreateAssetMenu(fileName = "NewTutorialMessage", menuName = "Tutorial/Tutorial Message", order = 1)]
public class TutorialMessage : ScriptableObject
{
    [Header("Identificació")]
    [Tooltip("ID únic per aquest missatge (per control de quins s'han mostrat)")]
    public string messageId;
    
    [Header("Contingut")]
    [Tooltip("Línies de text que es mostraran seqüencialment amb les seves animacions")]
    public List<TutorialLine> lines = new List<TutorialLine>();
    
    [Header("Temps")]
    [Tooltip("Temps que cada línia es manté visible després de fer fade-in (segons)")]
    public float lineDisplayTime = 2f;
    
    [Tooltip("Temps entre línies després de fade-out (segons)")]
    public float timeBetweenLines = 0.5f;
    
    [Tooltip("Permet al jugador saltar el missatge amb una tecla")]
    public bool canSkip = true;
    
    [Tooltip("Tecla per saltar el missatge")]
    public KeyCode skipKey = KeyCode.Space;
    
    [Header("Guardià")]
    [Tooltip("Mostrar el guardià durant aquest missatge")]
    public bool showGuardian = true;
    
    [Tooltip("Posició del guardià durant aquest missatge (worldspace o offset respecte camera)")]
    public Vector3 guardianPosition = new Vector3(0, 0, 0);
    
    [Tooltip("Rotació del guardià (Euler angles)")]
    public Vector3 guardianRotation = new Vector3(0, 0, 0);
    
    [Tooltip("Escala del guardià (deixar en 1,1,1 per defecte)")]
    public Vector3 guardianScale = new Vector3(1, 1, 1);
    
    [Tooltip("Si true, guardianPosition és relatiu a la càmera; si false, és posició world")]
    public bool guardianRelativeToCamera = false;
    
    [Tooltip("Nom de l'animació a reproduir (deixar buit per no canviar animació)")]
    public string guardianAnimationTrigger = "";
    
    [Header("Animació de text")]
    [Tooltip("Fer fade-out de l'última línia o deixar-la visible")]
    public bool fadeOutLastLine = false;
}
