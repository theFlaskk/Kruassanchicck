using UnityEngine;

/// <summary>
/// Скрипт пули с разделением на пули игрока и врагов.
/// Урон наносится только после задержки (защита от самопопадания).
/// </summary>
public class Projectile : MonoBehaviour
{
    public enum ShooterType { Player, Enemy }

    [Header("Settings")]
    [Tooltip("Кто выстрелил")]
    public ShooterType shooterType = ShooterType.Player;

    [Tooltip("Время до включения урона (секунды)")]
    public float damageDelay = 0.3f;

    [Tooltip("Урон пули")]
    public int damage = 10;

    [Header("Lifetime")]
    [Tooltip("Время жизни пули (секунды)")]
    public float lifetime = 5f;

    [Header("References")]
    [Tooltip("Кто выстрелил (заполняется автоматически)")]
    public GameObject shooter;

    private float damageTimer = 0f;
    private bool hasHit = false;

    private void Start()
    {
        // Уничтожить пулю через время жизни
        Destroy(gameObject, lifetime);

        // Запустить таймер задержки урона
        damageTimer = damageDelay;
    }

    private void Update()
    {
        // Уменьшаем таймер до включения урона
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Игнорируем саму себя
        if (other.gameObject == gameObject) return;

        // ✅ ПРОВЕРКА: Урон ещё не включён — игнорируем ВСЕ попадания
        if (damageTimer > 0f)
        {
            return;
        }

        // ============================================
        // ✅ ПУЛЯ ИГРОКА
        // ============================================
        if (shooterType == ShooterType.Player)
        {
            // Игнорируем игрока (свой не дамажит себя)
            if (other.CompareTag("Player")) return;

            // Дамажим врагов
            if (other.CompareTag("Enemy"))
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                hasHit = true;
                Destroy(gameObject);
                return;
            }
        }

        // ============================================
        // ✅ ПУЛЯ ВРАГА
        // ============================================
        if (shooterType == ShooterType.Enemy)
        {
            // Игнорируем врагов (свой не дамажит своих)
            if (other.CompareTag("Enemy")) return;

            // Дамажим игрока
            if (other.CompareTag("Player"))
            {
                // Урон игроку (если есть система здоровья)
                // PlayerStats player = other.GetComponent<PlayerStats>();
                // if (player != null) player.TakeDamage(damage);

                hasHit = true;
                Destroy(gameObject);
                return;
            }
        }

        // Попали в стену (не триггер)?
        if (!other.isTrigger)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (!hasHit)
        {
            Debug.Log($"[Projectile] 🗑️ Пуля уничтожена (время жизни истекло)");
        }
    }
}