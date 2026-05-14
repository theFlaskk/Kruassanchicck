using UnityEngine;
using System;

/// <summary>
/// Базовый класс врага с здоровьем и системой смерти.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Максимальное здоровье")]
    public int maxHealth = 100;

    [Tooltip("Текущее здоровье")]
    public int currentHealth;

    [Header("Visual")]
    [Tooltip("Спрайтрендерер для изменения цвета")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Цвет при смерти")]
    public Color deathColor = Color.black;

    [Header("Drop Settings")]
    [Tooltip("Выпадать ли оружие после смерти")]
    public bool dropWeaponOnDeath = true;

    [Tooltip("Префаб для выпавшего оружия (из папки Pickups)")]
    public WeaponBase weaponDropPrefab;

    [Header("Death Settings")]
    [Tooltip("Время до исчезновения после смерти (секунды)")]
    public float deathDelay = 1.5f;

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

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        OnSpawn?.Invoke();
    }

    /// <summary>
    /// Получить урон
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

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

        OnDeath?.Invoke();

        // Визуальный эффект смерти
        if (spriteRenderer != null)
        {
            spriteRenderer.color = deathColor;
        }

        // Отключаем коллайдеры и физику
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Отключаем скрипты ИИ и оружия
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null) enemyAI.enabled = false;

        EnemyWeapon enemyWeapon = GetComponent<EnemyWeapon>();
        if (enemyWeapon != null) enemyWeapon.enabled = false;

        // Выбрасываем оружие
        if (dropWeaponOnDeath && weaponDropPrefab != null)
        {
            DropWeapon();
        }

        // Уничтожаем через заданное время
        Destroy(gameObject, deathDelay);
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
    }

    /// <summary>
    /// Проверка жив ли враг
    /// </summary>
    public bool IsAlive() => !isDead;
}