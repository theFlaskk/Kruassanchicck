using UnityEngine;

/// <summary>
/// Автомат — высокая скорострельность, средний урон
/// </summary>
public class AssaultRifle : WeaponBase
{
    [Header("Assault Rifle Settings")]
    [Tooltip("Разброс пуль")]
    public float spread = 2f;

    private void Start()
    {
        // Настройки автомата
        weaponName = "Assault Rifle";
        magazineSize = 30;
        fireRate = 10f;  // 10 выстрелов в секунду
        damage = 8;
        reloadTime = 2.5f;
        projectileForce = 15f;
    }

    protected override void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Добавляем разброс
                float randomSpread = Random.Range(-spread, spread);
                Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);
                Vector2 shootDirection = spreadRotation * firePoint.right;

                rb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
            }
        }
    }
}