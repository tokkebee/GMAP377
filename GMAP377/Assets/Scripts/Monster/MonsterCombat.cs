using System;
using UnityEngine;

//This script handles damage
    //attack is automatically done via Monster.cs 's Behavior tree
    //this script holds damage

public class MonsterCombat : MonoBehaviour {
    [SerializeField] private float damage;
    [SerializeField] private Collider leg;
    [SerializeField] private LayerMask playerLayer;

    public static Action<float> OnMonsterDealtDamage; //sends amount of damage

    void OnCollisionEnter(Collision collision) {
        if (((1 << collision.gameObject.layer) & playerLayer.value) != 0) {
            OnMonsterDealtDamage?.Invoke(damage);
        }
    }

}