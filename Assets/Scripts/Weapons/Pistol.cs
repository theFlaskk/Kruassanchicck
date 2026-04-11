using UnityEngine;

/// <summary>
/// Пример конкретного оружия. Наследуйся от WeaponBase для новых пушек.
/// </summary>
public class Pistol : WeaponBase
{
    [Header("Pistol Specific")]
    public float spread = 1f; // Разброс пуль

    protected override void Shoot()
    {
        // Уникальная логика стрельбы пистолета
        if (projectilePrefab == null)
        {
            Debug.Log($"[{weaponName}] Базовый выстрел (нет префаба пули)");
            return;
        }

        // Создаём пулю
        GameObject bullet = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Добавляем разброс
        float randomSpread = Random.Range(-spread, spread);
        Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);
        Vector2 shootDirection = spreadRotation * firePoint.up;

        // Запускаем пулю
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
        }
    }
}