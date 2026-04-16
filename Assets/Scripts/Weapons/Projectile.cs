using UnityEngine;

/// <summary>
/// Скрипт пули с разделением на пули игрока и врагов.
/// </summary>
public class Projectile : MonoBehaviour
{
    public enum ShooterType { Player, Enemy }

    [Header("Settings")]
    [Tooltip("Кто выстрелил")]
    public ShooterType shooterType = ShooterType.Player;

    [Tooltip("Время игнорирования стрелка (секунды)")]
    public float ignoreShooterTime = 0.3f;

    [Tooltip("Урон пули")]
    public int damage = 10;

    [Header("Lifetime")]
    [Tooltip("Время жизни пули (секунды)")]
    public float lifetime = 5f;

    [Header("References")]
    [Tooltip("Кто выстрелил (заполняется автоматически)")]
    public GameObject shooter;

    private Collider2D shooterCollider;
    private float ignoreTimer = 0f;
    private bool hasHit = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);

        if (shooter != null)
        {
            shooterCollider = shooter.GetComponent<Collider2D>();
            if (shooterCollider == null)
            {
                shooterCollider = shooter.GetComponentInChildren<Collider2D>();
            }
        }

        Debug.Log($"[Projectile] 🚀 Пуля создана | Тип: {shooterType} | Урон: {damage}");
    }

    private void Update()
    {
        if (ignoreTimer < ignoreShooterTime)
        {
            ignoreTimer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Игнорируем саму себя
        if (other.gameObject == gameObject) return;

        // Игнорируем стрелка в первые 0.3 секунды (защита от вертикальной стрельбы)
        if (ignoreTimer < ignoreShooterTime)
        {
            if (shooterCollider != null && other == shooterCollider)
            {
                Debug.Log($"[Projectile] ⚠️ Игнорируем стрелка: {other.name}");
                return;
            }
        }

        // ============================================
        // ✅ ПУЛЯ ИГРОКА
        // ============================================
        if (shooterType == ShooterType.Player)
        {
            // Игнорируем игрока (свой не дамажит себя)
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[Projectile] 👤 Пуля игрока игнорирует игрока: {other.name}");
                return;
            }

            // Дамажим врагов
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"[Projectile] 💥 Пуля игрока попала во врага: {other.name} | Урон: {damage}");

                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[Projectile] 🗡️ Урон нанесён: {damage}");
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
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"[Projectile] 👤 Пуля врага игнорирует врага: {other.name}");
                return;
            }

            // Дамажим игрока
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[Projectile] 💥 Пуля врага попала в игрока: {other.name} | Урон: {damage}");

                // Здесь можно добавить урон игроку
                // PlayerHealth player = other.GetComponent<PlayerHealth>();
                // if (player != null) player.TakeDamage(damage);

                hasHit = true;
                Destroy(gameObject);
                return;
            }
        }

        // Попали в стену (не Trigger)?
        if (!other.isTrigger)
        {
            Debug.Log($"[Projectile] 🧱 Пуля попала в стену: {other.name}");
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