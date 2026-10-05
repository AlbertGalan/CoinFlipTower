using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("Referencias Audio")]
    [SerializeField] private AudioMixer mainMixer;
    
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("UI Buttons")]
    [SerializeField] private Button closeButton; 

    [Header("UI Panels")]
    [SerializeField] private GameObject optionsPanel;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseOptions);
    }

private void Start()
{
    // 1. Cargamos los valores (0.75f es el 75% del volumen, puedes poner 0.5f si prefieres)
    float savedMusic = PlayerPrefs.GetFloat("musicVolume", -20f);
    float savedSFX = PlayerPrefs.GetFloat("sfxVolume", -15f);

    // 2. Sincronizamos los Sliders visualmente
    if (musicSlider != null) musicSlider.value = savedMusic;
    if (sfxSlider != null) sfxSlider.value = savedSFX;

    // 3. ¡IMPORTANTE! Aplicamos el volumen al Mixer inmediatamente
    // Usamos las mismas funciones que usan los sliders para que la lógica sea igual
    UpdateMusicVolume(savedMusic);
    UpdateSFXVolume(savedSFX);
}
    // Estas funciones las conectas a los Sliders en el OnValueChanged
public void UpdateMusicVolume(float value)
{
    // 1. Calculamos los decibelios
    float dB = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20;

    // 2. Aplicamos al Mixer (Asegúrate de que el nombre sea exacto)
    if (mainMixer != null)
    {
        mainMixer.SetFloat("musicVol", dB);
        
        // DEBUG: Esto te dirá en consola si Unity está aceptando el cambio
        // Si sale "False", el nombre 'musicVol' no está bien expuesto en el Mixer
        // Debug.Log("¿Se aplicó la música?: " + mainMixer.SetFloat("musicVol", dB));
    }
}

public void UpdateSFXVolume(float value)
{
    float dB = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20;
    if (mainMixer != null)
    {
        mainMixer.SetFloat("sfxVol", dB);
    }
}

    public void ApplyOptions()
    {
        // Al aplicar, guardamos los valores actuales de los sliders en las preferencias
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
        PlayerPrefs.Save();

        Debug.Log("<color=cyan>Opciones Aplicadas y Guardadas.</color>");
    }

    public void CloseOptions()
    {
        // Opcional: Podrías llamar a ApplyOptions() aquí si quieres que cerrar también guarde,
        // o simplemente cerrar si quieres que el jugador pierda los cambios no aplicados.
        // Aquí lo haremos para que guarde por seguridad:
        ApplyOptions();

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }
}