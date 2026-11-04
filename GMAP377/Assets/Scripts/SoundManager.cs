using System;
using UnityEngine;
using UnityEngine.Audio;

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

// #if UNITY_EDITOR
//     private void OnEnable() {
//         string[] names = Enum.GetNames(typeof(SoundType));
//         Array.Resize(ref soundList, names.Length);
//         for (int i = 0; i < soundList.Length; i++) {
//             soundList[i].name = names[i];
//         }
//     }
// #endif

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
