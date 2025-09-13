using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public List<AudioClip> musicClips; // Assign in inspector
    public List<AudioClip> sfxClips;   // Assign in inspector

    private Dictionary<string, AudioClip> musicDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    [Range(0.5f, 5f)]
    public float musicFadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build dictionaries
        foreach (var clip in musicClips)
            if (clip != null) musicDict[clip.name] = clip;
        foreach (var clip in sfxClips)
            if (clip != null) sfxDict[clip.name] = clip;
    }

    private void Start()
    {
        if (musicClips.Count > 0)
            PlayMusic(musicClips[0].name);
    }

    // Smoothly transition to new music by name
    public void PlayMusic(string clipName)
    {
        if (musicDict.TryGetValue(clipName, out AudioClip newClip))
        {
            if (musicSource.isPlaying && musicSource.clip == newClip)
                return;
            StartCoroutine(FadeInMusic(newClip));
        }
    }

    // Play SFX by name
    public void PlaySFX(string clipName)
    {
        if (sfxDict.TryGetValue(clipName, out AudioClip clip))
            sfxSource.PlayOneShot(clip);
    }

    // Fade out current music and fade in new music
    private IEnumerator FadeInMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;
        // Fade out
        for (float t = 0; t < musicFadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
            yield return null;
        }
        musicSource.volume = 0f;
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();
        // Fade in
        for (float t = 0; t < musicFadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / musicFadeDuration);
            yield return null;
        }
        musicSource.volume = startVolume;
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void ToggleMute()
    {
        bool isMuted = AudioListener.volume == 0f;
        AudioListener.volume = isMuted ? 1f : 0f;
    }

    public bool IsMuted()
    {
        return AudioListener.volume == 0f;
    }
}