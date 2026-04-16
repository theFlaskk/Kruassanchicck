using UnityEngine;
using System;

/// <summary>
/// Базовый класс врага с здоровьем и смертью.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Максимальное здоровье")]
    public int maxHealth = 100;

    [Tooltip("Текущее здоровье")]
    public int currentHealth;

    [Header("Drop Settings")]
    [Tooltip("Выпадать ли оружие после смерти")]
    public bool dropWeaponOnDeath = true;

    [Tooltip("Префаб для выпавшего оружия (из папки Pickups)")]
    public WeaponBase weaponDropPrefab;

    // События
    public Action<int, int> OnHealthChanged;
    public Action OnDeath;
    public Action OnSpawn;

    protected bool isDead = false;
    protected Rigidbody2D rb;
    protected Animator animator;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        OnSpawn?.Invoke();
        Debug.Log($"[EnemyBase] 🎯 Враг spawned: {gameObject.name} | HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Получить урон
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (isDead)
        {
            Debug.Log($"[EnemyBase] ⚠️ Враг уже мёртв, урон игнорируется");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[EnemyBase] 💥 Урон получен: {damage} | HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Смерть врага
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[EnemyBase] ☠️ Враг умер: {gameObject.name}");

        OnDeath?.Invoke();

        if (dropWeaponOnDeath && weaponDropPrefab != null)
        {
            DropWeapon();
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Destroy(gameObject, 3f);
    }

    /// <summary>
    /// Выбросить оружие после смерти
    /// </summary>
    protected virtual void DropWeapon()
    {
        if (weaponDropPrefab == null) return;

        GameObject dropped = Instantiate(weaponDropPrefab.gameObject, transform.position, Quaternion.identity);

        EnemyWeapon enemyWeapon = GetComponent<EnemyWeapon>();
        if (enemyWeapon != null && enemyWeapon.currentWeapon != null)
        {
            WeaponDrop weaponDrop = dropped.GetComponent<WeaponDrop>();
            if (weaponDrop != null)
            {
                weaponDrop.savedAmmoInMag = enemyWeapon.currentWeapon.GetCurrentAmmoInMag();
            }
        }

        Debug.Log($"[EnemyBase] 📦 Оружие выпало: {weaponDropPrefab.weaponName}");
    }

    /// <summary>
    /// Проверка жив ли враг
    /// </summary>
    public bool IsAlive() => !isDead;
}