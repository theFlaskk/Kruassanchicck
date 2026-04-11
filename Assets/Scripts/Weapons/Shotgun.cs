using UnityEngine;

/// <summary>
/// Дробовик — низкая скорострельность, высокий урон, несколько пуль
/// </summary>
public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    [Tooltip("Количество пуль за выстрел")]
    public int pelletsPerShot = 5;

    [Tooltip("Разброс между пулями")]
    public float spreadAngle = 15f;

    private void Start()
    {
        // Настройки дробовика
        weaponName = "Shotgun";
        magazineSize = 6;
        fireRate = 1f;  // 1 выстрел в секунду
        damage = 6;  // Урон за пулю (всего 6*5=30)
        reloadTime = 3f;
        projectileForce = 12f;
    }

    protected override void Shoot()
    {
        if (projectilePrefab != null)
        {
            // Создаём несколько пуль
            for (int i = 0; i < pelletsPerShot; i++)
            {
                GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Разброс между пулями
                    float angleOffset = Mathf.Lerp(-spreadAngle, spreadAngle, (float)i / (pelletsPerShot - 1));
                    Quaternion spreadRotation = Quaternion.Euler(0, 0, angleOffset);
                    Vector2 shootDirection = spreadRotation * firePoint.right;

                    rb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}