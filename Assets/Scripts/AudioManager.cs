using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips - Arrays")]
    public AudioClip[] menuMusicArray;
    public AudioClip[] levelMusicArray;
    public AudioClip[] bossMusicArray; 

    [Header("SFX Clips")]
    public AudioClip buttonClickSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    [Header("UI Sliders")]
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;

    private string currentSceneType = ""; // Храним тип текущей музыки

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        LoadVolumeSettings();
        SetupSliders();
        PlayMusicForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Invoke("PlayMusicForCurrentScene", 0.1f);
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string newSceneType = "";
        AudioClip[] targetArray = null;

        if (sceneName == "Menu")
        {
            newSceneType = "Menu";
            targetArray = menuMusicArray;
            Debug.Log("Сцена Menu - загружаем музыку меню");
        }
        else if (sceneName == "Levels")
        {
            newSceneType = "Menu";
            targetArray = menuMusicArray;
            Debug.Log("Сцена Levels (выбор уровней) - загружаем музыку меню");
        }
        else if (sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3")
        {
            newSceneType = "Level";
            targetArray = levelMusicArray;
            Debug.Log($"Сцена {sceneName} - загружаем музыку уровня");
        }
        else if (sceneName == "LevelBoss" || sceneName.Contains("Boss"))
        {
            newSceneType = "Boss";
            targetArray = bossMusicArray;
            Debug.Log($"Сцена {sceneName} - загружаем музыку босса");
        }
        else
        {
            newSceneType = "Level";
            targetArray = levelMusicArray;
            Debug.Log($"Неизвестная сцена {sceneName} - загружаем музыку уровня");
        }

        if (currentSceneType == newSceneType && musicSource.isPlaying)
        {
            Debug.Log($"Тип музыки не изменился ({currentSceneType}), продолжаем играть ту же музыку");
            return;
        }

        currentSceneType = newSceneType;

        PlayRandomMusicFromArray(targetArray);
    }

    private void PlayRandomMusicFromArray(AudioClip[] musicArray)
    {
        if (musicArray == null || musicArray.Length == 0)
        {
            Debug.LogWarning($"Нет музыки в массиве! Тип: {currentSceneType}");
            return;
        }

        int randomIndex = Random.Range(0, musicArray.Length);
        AudioClip selectedClip = musicArray[randomIndex];

        Debug.Log($"Выбран трек #{randomIndex + 1}: {selectedClip.name}");
        PlayMusic(selectedClip);
    }

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        if (musicSource != null)
            musicSource.volume = musicVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    private void SetupSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();

        Debug.Log($"Воспроизводится музыка: {clip.name}");
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void ForceChangeMusic(string sceneType)
    {
        currentSceneType = "";
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"Музыка: {volume * 100}%");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        if (sfxSource != null)
            sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"Звуки: {volume * 100}%");
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}