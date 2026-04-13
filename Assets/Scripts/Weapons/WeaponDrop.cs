using UnityEngine;

/// <summary>
/// Компонент для выпавшего оружия на земле.
/// Сохраняет только патроны в магазине.
/// </summary>
public class WeaponDrop : MonoBehaviour
{
    [Header("Weapon Data")]
    [Tooltip("Префаб оружия для добавления в инвентарь")]
    public WeaponBase weaponPrefab;

    [Header("Saved Ammo")]
    [Tooltip("Патроны в магазине при выпадении")]
    public int savedAmmoInMag = 0;

    [Header("Visual")]
    [Tooltip("Вращать оружие на земле")]
    public bool rotateOnGround = true;
    public float rotateSpeed = 50f;

    [Tooltip("Покачивать оружие вверх-вниз")]
    public bool bobOnGround = true;
    public float bobHeight = 0.3f;
    public float bobSpeed = 2f;

    private Vector3 startPosition;
    private bool isPlayerNear = false;
    private WeaponManager playerManager;

    private void Start() => startPosition = transform.position;

    private void Update()
    {
        if (rotateOnGround) transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        if (bobOnGround)
        {
            float y = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, y, startPosition.z);
        }
        if (isPlayerNear && playerManager != null && Input.GetKeyDown(KeyCode.E))
            TryPickup();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerManager = other.GetComponent<WeaponManager>();
            UIManager.Instance?.ShowPickupPrompt(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerManager = null;
            UIManager.Instance?.ShowPickupPrompt(false);
        }
    }

    private void TryPickup()
    {
        if (playerManager == null || weaponPrefab == null) return;
        Debug.Log($"[WeaponDrop] 📥 Подбор: {weaponPrefab.weaponName} | Магазин: {savedAmmoInMag}");
        bool success = playerManager.PickUpWeaponWithAmmo(weaponPrefab, savedAmmoInMag, 0);
        if (success)
        {
            UIManager.Instance?.ShowPickupPrompt(false);
            Destroy(gameObject);
        }
    }
}