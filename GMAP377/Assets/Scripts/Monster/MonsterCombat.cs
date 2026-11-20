using System;
using Pathfinding.BehaviourTrees;
using UnityEngine;

//This script handles damage
    //attack is automatically done via Monster.cs 's Behavior tree
    //this script holds damage

public class MonsterCombat : MonoBehaviour {
    [SerializeField] private float damage;
    [SerializeField] private Collider leg;
    [SerializeField] private LayerMask playerLayer;
    private bool hasHit;

    public static Action<float> OnMonsterDealtDamage; //sends amount of damage

    private void OnEnable() {
        AttackStrategy.OnAttackStarted += EnableHitbox;
        AttackStrategy.OnAttackFinished += DisableHitbox;
    }

    private void OnDisable() {
        AttackStrategy.OnAttackStarted -= EnableHitbox;
        AttackStrategy.OnAttackFinished -= DisableHitbox;
    }

    private void Awake() {
        leg.enabled = false; //hitbox only exists during an attack
    }

    private void EnableHitbox() {
        hasHit = false;
        leg.enabled = true;
    }

    private void DisableHitbox() {
        leg.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (hasHit) return;
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        hasHit = true;
        Debug.Log("Monster hit the player", this);
        OnMonsterDealtDamage?.Invoke(damage);
    }

    // private void TryHit() {
    //     Bounds bounds = leg.bounds;
    //     Collider[] hits = Physics.OverlapBox(bounds.center, bounds.extents, leg.transform.rotation, playerLayer);

    //     if (hits.Length > 0) {
    //         Debug.Log("Monster hit the player", this);
    //         OnMonsterDealtDamage?.Invoke(damage);
    //     }
    // }

    // void OnCollisionEnter(Collision collision) {
    //     if (((1 << collision.gameObject.layer) & playerLayer.value) != 0) {
    //         OnMonsterDealtDamage?.Invoke(damage);
    //     }
    // }
}