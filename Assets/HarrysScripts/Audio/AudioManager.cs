using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixerGroup audioMixer, musicAudioMixer, sfxAudioMixer;
    public List<Sound> sounds;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            s.source.loop = s.loop;

            if (s.soundType == Sound.SoundType.SFX)
            {
                s.source.outputAudioMixerGroup = sfxAudioMixer;
            }
            else if (s.soundType == Sound.SoundType.MUSIC)
            {
                s.source.outputAudioMixerGroup = musicAudioMixer;
            }
        }
    }
    
    public void Play (string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("No sound with name " + name);
            return;
        }
            
        s.source.Play();
    }

    public void Stop (string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("No sound with name " + name);
            return;
        }
            
        s.source.Stop();
    }

    public void Pause (string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("No sound with name " + name);
            return;
        }
            
        s.source.Pause();
    }

    public bool IsPlaying (string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("No sound with name " + name);
            return false;
        }

        if (s.source.isPlaying)
        {
            return true;
        }

        return false;
    }
}
