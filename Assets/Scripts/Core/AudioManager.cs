using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace IslamicGame.Core
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource uiSource;
        
        [Header("Audio Mixer")]
        public AudioMixer audioMixer;
        
        [Header("Sound Library")]
        public List<Sound> sounds = new List<Sound>();
        
        private Dictionary<string, AudioClip> soundDictionary;

        protected override void Awake()
        {
            base.Awake();
            InitializeSounds();
            PlayMusic("background_music");
        }
        
        void InitializeSounds()
        {
            soundDictionary = new Dictionary<string, AudioClip>();
            foreach (var sound in sounds)
            {
                if (!soundDictionary.ContainsKey(sound.name))
                    soundDictionary.Add(sound.name, sound.clip);
            }
        }
        
        public void PlaySound(string soundName)
        {
            if (soundDictionary.ContainsKey(soundName))
            {
                sfxSource.PlayOneShot(soundDictionary[soundName]);
            }
            else
            {
                Debug.LogWarning($"Sound {soundName} not found!");
            }
        }
        
        public void PlayUISound(string soundName)
        {
            if (soundDictionary.ContainsKey(soundName))
            {
                uiSource.PlayOneShot(soundDictionary[soundName]);
            }
        }
        
        public void PlayMusic(string musicName)
        {
            if (soundDictionary.ContainsKey(musicName))
            {
                musicSource.clip = soundDictionary[musicName];
                musicSource.Play();
                musicSource.loop = true;
            }
        }
        
        public void StopMusic()
        {
            musicSource.Stop();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicSource.volume = volume;
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = volume;
            uiSource.volume = volume;
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
        
        public void SetMasterVolume(float volume)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
        
        public void SetSoundEnabled(bool enabled)
        {
            sfxSource.mute = !enabled;
            uiSource.mute = !enabled;
        }
        
        public void SetMusicEnabled(bool enabled)
        {
            musicSource.mute = !enabled;
        }
    }
    
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }
}