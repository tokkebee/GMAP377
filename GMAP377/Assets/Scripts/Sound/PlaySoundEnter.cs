using UnityEngine;

//This script plays a sound upon entering an animation state

//full credit to Small Hedge Games: 
//github repo: https://github.com/SmallHedge/SoundManager

public class PlaySoundEnter : StateMachineBehaviour {
    [SerializeField] private SoundType sound;
    [SerializeField, Range(0,1)] private float volume = 1;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        SoundManager.PlaySound(sound, null, volume);
    }
}