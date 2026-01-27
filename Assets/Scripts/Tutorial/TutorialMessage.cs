using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("Línies de text que es mostraran seqüencialment")]
    [TextArea(3, 10)]
    public List<string> textLines = new List<string>();
    
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
    [Tooltip("Posició del guardià durant aquest missatge (worldspace o offset respecte camera)")]
    public Vector3 guardianPosition = new Vector3(0, 0, 0);
    
    [Tooltip("Si true, guardianPosition és relatiu a la càmera; si false, és posició world")]
    public bool guardianRelativeToCamera = false;
    
    [Tooltip("Mostrar el guardià durant aquest missatge")]
    public bool showGuardian = true;
    
    [Header("Animació de text")]
    [Tooltip("Fer fade-out de l'última línia o deixar-la visible")]
    public bool fadeOutLastLine = false;
}
