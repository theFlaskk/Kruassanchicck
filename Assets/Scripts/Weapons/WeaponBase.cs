using UnityEngine;

/// <summary>
/// Базовый класс оружия с системой патронов только в магазине (без резерва).
/// </summary>
public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName = "New Weapon";
    public Transform firePoint;
    public float fireRate = 2f;
    public int damage = 10;
    public float projectileForce = 10f;
    public GameObject projectilePrefab;

    [Header("Ammo System")]
    [Tooltip("Ёмкость магазина")]
    public int magazineSize = 12;

    [Tooltip("Время перезарядки в секундах")]
    public float reloadTime = 2f;

    [Header("Position")]
    public Vector3 anchorOffset = new Vector3(0f, 0.3f, 0f);

    [Header("Visual - Flip Settings")]
    public bool flipHorizontallyWhenAimingLeft = true;
    public bool flipVerticallyWhenAimingLeft = false;

    // === ТЕКУЩЕЕ СОСТОЯНИЕ ===
    protected int currentAmmoInMag;      // Патроны в магазине
    protected bool isReloading = false;
    protected float reloadTimer = 0f;
    protected float fireTimer = 0f;

    // Внутренние ссылки
    protected Transform weaponAnchor;
    protected SpriteRenderer weaponSprite;
    protected Camera mainCamera;

    // === СОБЫТИЯ ДЛЯ UI ===
    // Передаём: (в магазине, 0, макс. магазин) — второй параметр всегда 0 для совместимости
    public System.Action<int, int, int> OnAmmoUpdated;
    public System.Action<bool, float> OnReloadUpdated;

    private void Awake()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        ResetAmmoToStartingValues();
    }

    /// <summary>
    /// Сбросить патроны к стартовым значениям (полный магазин)
    /// </summary>
    public virtual void ResetAmmoToStartingValues()
    {
        currentAmmoInMag = magazineSize;
        NotifyAmmoUpdated();
    }

    public virtual void OnEquip(Transform anchor)
    {
        if (anchor == null)
        {
            Debug.LogError($"[{weaponName}] Anchor is null!");
            return;
        }

        weaponAnchor = anchor;

        // Чистая привязка
        transform.SetParent(null);
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;
        transform.SetParent(anchor);
        transform.localPosition = anchorOffset;
        transform.localRotation = Quaternion.identity;

        if (weaponSprite != null)
        {
            weaponSprite.flipX = false;
            weaponSprite.flipY = false;
        }

        gameObject.SetActive(true);
        NotifyAmmoUpdated();

        Debug.Log($"[{weaponName}] ✅ Экипирован | Магазин: {currentAmmoInMag}/{magazineSize}");
    }

    public virtual void OnUnequip()
    {
        weaponAnchor = null;
        gameObject.SetActive(false);
    }

    public virtual void UpdateAimDirection(Vector3 mouseWorldPos)
    {
        if (weaponSprite == null || weaponAnchor == null) return;

        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)weaponAnchor.position;
        bool isAimingLeft = direction.x < 0;

        if (flipHorizontallyWhenAimingLeft)
            weaponSprite.flipX = isAimingLeft;

        if (flipVerticallyWhenAimingLeft)
            weaponSprite.flipY = isAimingLeft;
    }

    public virtual bool TryShoot()
    {
        if (isReloading)
        {
            Debug.LogWarning($"[{weaponName}] ⚠️ Нельзя стрелять во время перезарядки!");
            return false;
        }

        if (currentAmmoInMag <= 0)
        {
            Debug.LogWarning($"[{weaponName}] ⚠️ Магазин пуст! Перезарядка...");
            TryReload();
            return false;
        }

        if (fireTimer > 0) return false;
        if (firePoint == null)
        {
            Debug.LogError($"[{weaponName}] ❌ FirePoint не назначен!");
            return false;
        }
        if (!gameObject.activeInHierarchy) return false;

        // ✅ Выстрел: уменьшаем патроны
        currentAmmoInMag--;
        NotifyAmmoUpdated();

        Shoot();
        fireTimer = 1f / fireRate;

        return true;
    }

    public virtual bool TryReload()
    {
        if (isReloading) return false;
        if (currentAmmoInMag >= magazineSize) return false;

        // ✅ Всегда можно перезарядиться (нет резерва)
        StartReload();
        return true;
    }

    protected virtual void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;

        OnReloadUpdated?.Invoke(true, 0f);
        Debug.Log($"[{weaponName}] 🔄 Перезарядка... ({reloadTime} сек)");
    }

    protected virtual void FinishReload()
    {
        // ✅ Просто заполняем магазин до максимума
        currentAmmoInMag = magazineSize;

        isReloading = false;
        reloadTimer = 0f;

        OnReloadUpdated?.Invoke(false, 1f);
        NotifyAmmoUpdated();

        Debug.Log($"[{weaponName}] ✅ Перезаряжено! Магазин: {currentAmmoInMag}/{magazineSize}");
    }

    protected virtual void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(firePoint.right * projectileForce, ForceMode2D.Impulse);
            }
        }
    }

    public virtual void WeaponUpdate()
    {
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        if (isReloading && reloadTimer > 0)
        {
            reloadTimer -= Time.deltaTime;
            float progress = 1f - (reloadTimer / reloadTime);
            OnReloadUpdated?.Invoke(true, progress);

            if (reloadTimer <= 0)
            {
                FinishReload();
            }
        }
    }

    /// <summary>
    /// Уведомить UI об изменении патронов
    /// Второй параметр всегда 0 (для совместимости с старым UI)
    /// </summary>
    protected virtual void NotifyAmmoUpdated()
    {
        OnAmmoUpdated?.Invoke(currentAmmoInMag, 0, magazineSize);
    }

    // === ГЕТТЕРЫ ДЛЯ UI ===
    public int GetCurrentAmmoInMag() => currentAmmoInMag;
    public int GetMaxMagazineSize() => magazineSize;
    public bool IsReloading() => isReloading;
    public float GetReloadProgress() => isReloading ? 1f - (reloadTimer / reloadTime) : 0f;
    public float GetReloadTime() => reloadTime;

    /// <summary>
    /// Установить патроны (для подобранного оружия)
    /// Второй параметр игнорируется (нет резерва)
    /// </summary>
    public virtual void SetAmmo(int ammoInMag, int ammoReserve)
    {
        currentAmmoInMag = Mathf.Clamp(ammoInMag, 0, magazineSize);
        NotifyAmmoUpdated();
        Debug.Log($"[{weaponName}] 📊 Патроны установлены: {currentAmmoInMag}/{magazineSize}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 1.5f);
        }
    }
#endif
}