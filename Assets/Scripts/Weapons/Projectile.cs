using UnityEngine;

/// <summary>
/// Скрипт пули с ЗАЩИТОЙ от столкновения со стрелком.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Время игнорирования стрелка (секунды)")]
    public float ignoreShooterTime = 0.3f;  // ✅ Увеличил с 0.1 до 0.3

    [Tooltip("Урон пули")]
    public int damage = 10;

    [Header("Lifetime")]
    [Tooltip("Время жизни пули (секунды)")]
    public float lifetime = 5f;

    private Collider2D shooterCollider;
    private float ignoreTimer = 0f;
    private bool hasHit = false;

    private void Start()
    {
        // ✅ Уничтожить пулю через 5 секунд
        Destroy(gameObject, lifetime);

        // ✅ Найти коллайдер стрелка (родитель или Weapon)
        Transform parent = transform.parent;
        if (parent != null)
        {
            shooterCollider = parent.GetComponent<Collider2D>();
            if (shooterCollider == null)
            {
                shooterCollider = parent.GetComponentInParent<Collider2D>();
            }
        }

        Debug.Log($"[Projectile] 🎯 Пуля создана | Игнор стрелка: {ignoreShooterTime}с");
        Debug.Log($"[Projectile] 🔍 shooterCollider: {(shooterCollider != null ? shooterCollider.name : "NULL")}");
    }

    private void Update()
    {
        // ✅ Таймер неуязвимости от стрелка
        if (ignoreTimer < ignoreShooterTime)
        {
            ignoreTimer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Лог для отладки
        Debug.Log($"[Projectile] 🎯 OnTriggerEnter: {other.name} | Tag: {other.tag} | ignoreTimer: {ignoreTimer:F2}");

        // ✅ 1. Игнорируем стрелка в первые 0.3 секунды
        if (ignoreTimer < ignoreShooterTime)
        {
            if (shooterCollider != null && other == shooterCollider)
            {
                Debug.Log($"[Projectile] ⚠️ Игнорируем стрелка: {other.name} (timer={ignoreTimer:F2})");
                return;
            }

            // ✅ 2. Игнорируем ВСЕХ врагов в первые 0.3 секунды (защита от других врагов)
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"[Projectile] ⚠️ Игнорируем врага: {other.name} (timer={ignoreTimer:F2})");
                return;
            }
        }

        // ✅ 3. Игнорируем саму себя
        if (other.gameObject == gameObject) return;

        // ✅ 4. Попали во врага?
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"[Projectile] 💥 Попали во врага: {other.name} | Урон: {damage}");

            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // ✅ 5. Попали в игрока?
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Projectile] 💥 Попали в игрока: {other.name} | Урон: {damage}");

            // Тут можно добавить урон игроку
            // PlayerHealth player = other.GetComponent<PlayerHealth>();
            // if (player != null) player.TakeDamage(damage);

            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // ✅ 6. Попали в стену (не Trigger)?
        if (!other.isTrigger)
        {
            Debug.Log($"[Projectile] 🧱 Попали в стену: {other.name}");
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