using UnityEngine;

/// <summary>
/// Базовый класс оружия.
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
    protected int currentAmmoInMag;
    protected bool isReloading = false;
    protected float reloadTimer = 0f;
    protected float fireTimer = 0f;

    // Внутренние ссылки
    protected Transform weaponAnchor;
    protected SpriteRenderer weaponSprite;
    protected Camera mainCamera;

    // === СОБЫТИЯ ДЛЯ UI ===
    public System.Action<int, int, int> OnAmmoUpdated;
    public System.Action<bool, float> OnReloadUpdated;

    private void Awake()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        ResetAmmoToStartingValues();
    }

    public virtual void ResetAmmoToStartingValues()
    {
        currentAmmoInMag = magazineSize;
        NotifyAmmoUpdated();
    }

    public virtual void OnEquip(Transform anchor)
    {
        if (anchor == null) return;
        weaponAnchor = anchor;
        transform.SetParent(null);
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;
        transform.SetParent(anchor);
        transform.localPosition = anchorOffset;
        transform.localRotation = Quaternion.identity;
        if (weaponSprite != null) { weaponSprite.flipX = false; weaponSprite.flipY = false; }
        gameObject.SetActive(true);
        NotifyAmmoUpdated();
    }

    public virtual void OnUnequip() { weaponAnchor = null; gameObject.SetActive(false); }

    public virtual void UpdateAimDirection(Vector3 mouseWorldPos)
    {
        if (weaponSprite == null || weaponAnchor == null) return;
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)weaponAnchor.position;
        bool isAimingLeft = direction.x < 0;
        if (flipHorizontallyWhenAimingLeft) weaponSprite.flipX = isAimingLeft;
        if (flipVerticallyWhenAimingLeft) weaponSprite.flipY = isAimingLeft;
    }

    public virtual bool TryShoot()
    {
        if (isReloading) return false;
        if (currentAmmoInMag <= 0) { TryReload(); return false; }
        if (fireTimer > 0) return false;
        if (firePoint == null) return false;
        if (projectilePrefab == null) return false;
        if (!gameObject.activeInHierarchy) return false;

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
        StartReload();
        return true;
    }

    protected virtual void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
        OnReloadUpdated?.Invoke(true, 0f);
    }

    protected virtual void FinishReload()
    {
        currentAmmoInMag = magazineSize;
        isReloading = false;
        reloadTimer = 0f;
        OnReloadUpdated?.Invoke(false, 1f);
        NotifyAmmoUpdated();
    }

    protected virtual void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.AddForce(firePoint.right.normalized * projectileForce, ForceMode2D.Impulse);
    }

    public virtual void WeaponUpdate()
    {
        if (fireTimer > 0) fireTimer -= Time.deltaTime;
        if (isReloading && reloadTimer > 0)
        {
            reloadTimer -= Time.deltaTime;
            float progress = 1f - (reloadTimer / reloadTime);
            OnReloadUpdated?.Invoke(true, progress);
            if (reloadTimer <= 0) FinishReload();
        }
    }

    protected virtual void NotifyAmmoUpdated() { OnAmmoUpdated?.Invoke(currentAmmoInMag, 0, magazineSize); }

    // === ГЕТТЕРЫ ===
    public int GetCurrentAmmoInMag() => currentAmmoInMag;
    public int GetMaxMagazineSize() => magazineSize;
    public bool IsReloading() => isReloading;
    public float GetReloadProgress() => isReloading ? 1f - (reloadTimer / reloadTime) : 0f;

    // ✅ НОВЫЙ МЕТОД: Сброс fireTimer (для врагов)
    public void ResetFireTimer() { fireTimer = 0f; }

    public virtual void SetAmmo(int ammoInMag, int ammoReserve)
    {
        currentAmmoInMag = Mathf.Clamp(ammoInMag, 0, magazineSize);
        NotifyAmmoUpdated();
    }
}