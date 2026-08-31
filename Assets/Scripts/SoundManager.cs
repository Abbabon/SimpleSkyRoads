using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum SoundEffect
{
    Shield,
    Explosion,
    RockExplosion,
    Points,
    Boost,
}

public class SoundManager : MonoBehaviour
{
    #region Singleton Implementation

    private static SoundManager _instance;
    public static SoundManager Instance { get { return _instance; } }

    private static readonly object padlock = new object();

    private void Awake()
    {
        lock (padlock)
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
                Initialize();
            }
        }
    }

    private void Initialize()
    {
        LoadSoundEffects();
        _musicAudioSource.clip = Resources.Load<AudioClip>("Music/Menu");
        _musicAudioSource.Play();
        GameManager.OnSessionStarted += PlayLevelMusic;
    }

    #endregion

    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;

    private Dictionary<SoundEffect, AudioClip> soundEffects;

    private void LoadSoundEffects()
    {
        soundEffects = new Dictionary<SoundEffect, AudioClip>();
        foreach (SoundEffect soundEffect in (SoundEffect[])Enum.GetValues(typeof(SoundEffect)))
        {
            soundEffects.Add(soundEffect, Resources.Load<AudioClip>(String.Format("Effects/{0}", soundEffect)));
        }
    }

    public void PlaySoundEffect(SoundEffect soundEffect, bool cancelIfNotPlaying = false)
    {
        //C# doesnt have unless :(((
        if (!(_sfxAudioSource.isPlaying && cancelIfNotPlaying))
            _sfxAudioSource.PlayOneShot(soundEffects[soundEffect]);
    }

    private void PlayLevelMusic()
    {
        _musicAudioSource.clip = Resources.Load<AudioClip>("Music/Game");
        _musicAudioSource.Stop();
        _musicAudioSource.Play();
    }
}
