using UnityEngine;

//This script plays a sound upon exiting an animation state

//full credit to Small Hedge Games: 
//github repo: https://github.com/SmallHedge/SoundManager

public class PlaySoundExit : StateMachineBehaviour {
    [SerializeField] private SoundType sound;
    [SerializeField, Range(0,1)] private float volume = 1;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        SoundManager.PlaySound(sound, null, volume);
    }
}