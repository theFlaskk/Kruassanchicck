using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;  // Префаб пули
    public Transform firePoint;          // Точка выстрела
    public float projectileForce = 10f;  // Сила выстрела

    private PlayerStats stats;
    private float fireTimer = 0f;
    private Camera mainCamera; // Кэшируем камеру для производительности

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        mainCamera = Camera.main; // Находим камеру один раз при старте
    }

    private void Update()
    {
        // Уменьшаем таймер
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        // Если нажата ЛКМ и таймер готов
        if (Input.GetButton("Fire1") && fireTimer <= 0)
        {
            Shoot();
            fireTimer = 1f / stats.CurrentFireRate;
        }
    }

    private void Shoot()
    {
        // Проверки на всякий случай
        if (projectilePrefab == null || firePoint == null || mainCamera == null)
        {
            Debug.LogWarning("Проверь настройки стрельбы в Инспекторе!");
            return;
        }

        // 1. Создаем пулю
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 2. Узнаем позицию мыши в мире
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // 3. Считаем направление ОТ ОГНЕВОЙ ТОЧКИ к курсору
            Vector2 shootDirection = (mouseWorldPos - firePoint.position).normalized;

            // 4. ПОВОРАЧИВАЕМ пулю лицом к цели (визуал)
            // Если пуля смотрит не туда, поменяй 90f на 0f или -90f
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.Euler(0, 0, 90f);

            // 5. Толкаем пулю
            rb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
        }
    }
}