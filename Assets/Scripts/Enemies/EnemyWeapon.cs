using UnityEngine;

/// <summary>
/// Контроллер оружия для врага.
/// </summary>
public class EnemyWeapon : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("Префаб оружия для врага (чистый, не из Pickups!)")]
    public WeaponBase weaponPrefab;

    [Header("Settings")]
    public float spread = 0.1f;

    [HideInInspector] public WeaponBase currentWeapon;
    private Transform currentFirePoint;
    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    private void Start()
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("[EnemyWeapon] ❌ weaponPrefab = NULL!");
            return;
        }

        currentWeapon = Instantiate(weaponPrefab);
        if (currentWeapon == null)
        {
            Debug.LogError("[EnemyWeapon] ❌ Instantiate вернул NULL!");
            return;
        }

        // Привязка к врагу
        currentWeapon.transform.SetParent(transform);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.gameObject.SetActive(true);

        // Поиск FirePoint
        currentFirePoint = currentWeapon.transform.Find("FirePoint");
        if (currentFirePoint == null)
        {
            Debug.LogWarning("[EnemyWeapon] ⚠️ FirePoint не найден! Используем transform");
            currentFirePoint = currentWeapon.transform;
        }

        // ✅ СБРОС fireTimer оружия (чтобы мог стрелять сразу)
        currentWeapon.ResetFireTimer();

        Debug.Log($"[EnemyWeapon] 🔫 Оружие экипировано: {currentWeapon.weaponName}");
        Debug.Log($"[EnemyWeapon] 📊 Патроны: {currentWeapon.GetCurrentAmmoInMag()}/{currentWeapon.GetMaxMagazineSize()}");
    }

    public void TryShoot(Vector2 direction)
    {
        if (currentWeapon == null)
        {
            Debug.LogError("[EnemyWeapon] ❌ currentWeapon = NULL!");
            return;
        }

        if (enemyBase == null || !enemyBase.IsAlive()) return;

        // Поворот оружия
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Выстрел
        currentWeapon.TryShoot();
    }

    public WeaponBase GetCurrentWeapon() => currentWeapon;
}