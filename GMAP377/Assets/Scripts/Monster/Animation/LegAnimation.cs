using System.Collections;
using Pathfinding.BehaviourTrees;
using UnityEngine;

//THIS IS A TEMPORARY SCRIPT. placeholder for animation
//meant to communicate attack (windup, attack, and recovery)

public class LegAnimation : MonoBehaviour {
    [SerializeField] private Monster monster;

    [Header("Animation Targets")]
    [SerializeField] private Transform windupTransform;
    [SerializeField] private Transform attackTransform;
    private Vector3 restingLocalPosition;
    private Quaternion restingLocalRotation;

    [Header("Animation Durations")]
    //windup and attack durations should equal attackDuration in Monster.cs
    [SerializeField] private float windupDuration = 0.5f;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float recoveryDuration = 0.4f;

    private Coroutine animationRoutine;

    private void OnEnable() {
        AttackStrategy.OnAttackStarted += PlayAttackAnimation;
        AttackStrategy.OnAttackFinished += PlayRecoveryAnimation;
    }

    private void OnDisable() {
        AttackStrategy.OnAttackStarted -= PlayAttackAnimation;
        AttackStrategy.OnAttackFinished -= PlayRecoveryAnimation;
        StopAnimation();
    }

    private void Awake() {
        restingLocalPosition = transform.localPosition;
        restingLocalRotation = transform.localRotation;
    }

    //Attack Animation
    private void PlayAttackAnimation() {
        StopAnimation();
        animationRoutine = StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence() {
        Transform parent = transform.parent;

        //Windup
        Vector3 windupPosition = parent.InverseTransformPoint(windupTransform.position);
        Quaternion windupRotation = Quaternion.Inverse(parent.rotation) * windupTransform.rotation;

        //Attack
        Vector3 attackPosition = parent.InverseTransformPoint(attackTransform.position);
        Quaternion attackRotation = Quaternion.Inverse(parent.rotation) * attackTransform.rotation;

        // Debug.Log($"Rest position: {restingLocalPosition}");
        // Debug.Log($"Windup position: {windupPosition}");
        // Debug.Log($"Attack position: {attackPosition}");
        
        yield return MoveToLocalTransform(windupPosition, windupRotation, windupDuration);
        yield return MoveToLocalTransform(attackPosition, attackRotation, attackDuration);

        animationRoutine = null;
    }

    //Recovery Animation
    private void PlayRecoveryAnimation() {
        StopAnimation();
        animationRoutine = StartCoroutine(RecoverySequence());
    }

    private IEnumerator RecoverySequence() {
        //Recovery
        yield return MoveToLocalTransform(restingLocalPosition, restingLocalRotation, recoveryDuration);

        animationRoutine = null;
    }

    //Helper Functions
    private IEnumerator MoveToLocalTransform(Vector3 targetPosition, Quaternion targetRotation, float duration) {
        Vector3 startPosition = transform.localPosition;
        Quaternion startRotation = transform.localRotation;

        if (duration <= 0f) {
            transform.localPosition = targetPosition;
            transform.localRotation = targetRotation;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            float time = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            transform.localPosition = Vector3.Lerp(
                startPosition, targetPosition, time);

            transform.localRotation = Quaternion.Slerp(
                startRotation, targetRotation, time);

            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localRotation = targetRotation;
    }

    private void StopAnimation() {
        if (animationRoutine != null) {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }
}
