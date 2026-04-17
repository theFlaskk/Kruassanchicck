using UnityEngine;
using System;

/// <summary>
/// Статы игрока: здоровье, урон, защита.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Максимальное здоровье")]
    public int maxHealth = 100;

    [Tooltip("Текущее здоровье")]
    public int currentHealth;

    [Header("Combat Stats")]
    [Tooltip("Базовый урон")]
    public int damage = 10;

    [Tooltip("Защита")]
    public int defense = 0;

    [Tooltip("Базовая скорострельность (выстрелов в секунду)")]
    public float baseFireRate = 2f;

    [Tooltip("Множитель скорострельности (от баффов)")]
    public float fireRateMultiplier = 1f;

    // События
    public Action<int, int> OnHealthChanged;
    public Action OnDeath;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// ✅ Текущая скорострельность с учётом множителей
    /// </summary>
    public float CurrentFireRate => baseFireRate * fireRateMultiplier;

    /// <summary>
    /// Получить урон
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int actualDamage = Mathf.Max(1, damage - defense);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[PlayerStats] 💥 Урон: {actualDamage} | HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Лечение
    /// </summary>
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[PlayerStats] 💚 Лечение: {amount} | HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Смерть
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[PlayerStats] ☠️ {gameObject.name} умер!");
        OnDeath?.Invoke();
    }

    /// <summary>
    /// Проверка: жив ли игрок
    /// </summary>
    public bool IsAlive() => !isDead;

    /// <summary>
    /// Воскресить игрока
    /// </summary>
    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[PlayerStats] ✨ {gameObject.name} воскрешён!");
    }

    /// <summary>
    /// Изменить множитель скорострельности (для баффов)
    /// </summary>
    public void ModifyFireRateMultiplier(float multiplier)
    {
        fireRateMultiplier = multiplier;
        Debug.Log($"[PlayerStats] 🔄 Скорострельность изменена: {CurrentFireRate}");
    }
}