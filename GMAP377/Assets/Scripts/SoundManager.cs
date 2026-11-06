using System;
using UnityEngine;
using UnityEngine.Audio;

//This script handles playing sound

//full credit to Small Hedge Games: 
//github repo: https://github.com/SmallHedge/SoundManager

[Serializable]
public struct SoundList {
    [HideInInspector] public string name;
    [Range(0,1)] public float volume;
    public AudioMixerGroup mixer;
    public AudioClip[] sounds;
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {
    private static SoundManager instance = null;
    private AudioSource audioSource;
    [SerializeField] private SoundsSO SO;

    private void Awake() {
        if (!instance) {
            instance = this;
            audioSource = GetComponent<AudioSource>();
        }
    }
    public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1) {
        SoundList soundList = instance.SO.sounds[(int)sound];
        AudioClip[] clips = soundList.sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        if(source) {
            source.outputAudioMixerGroup = soundList.mixer;
            source.clip = randomClip;
            source.volume = volume * soundList.volume;
            source.Play();
        } else {
            instance.audioSource.outputAudioMixerGroup = soundList.mixer;
            instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
        }
    }
}
