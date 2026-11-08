using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    [SerializeField] private float maxHealth = 1f;
    [SerializeField] private float currentHealth;
    private bool isDead;

    public static event Action<float> OnHealthChanged; //passes percentage. currentHealth / maxHealth
    public static event Action OnDamageTaken;
    public static event Action OnPlayerDeath;

    private void OnEnable() {
        MonsterCombat.OnMonsterDealtDamage += TakeDamage;
    }

    private void OnDisable() {
        MonsterCombat.OnMonsterDealtDamage -= TakeDamage;
    }

    private void Start() {
        currentHealth = maxHealth;
        isDead = false;
    }

    private void TakeDamage(float damage) {
        if (isDead) return;
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        OnDamageTaken?.Invoke();

        if (currentHealth <= 0f) {
            PlayerDeath();
        }
    }

    private void PlayerDeath() {
        isDead = true;
        Debug.Log("Player died");
        OnPlayerDeath?.Invoke();
    }
}
