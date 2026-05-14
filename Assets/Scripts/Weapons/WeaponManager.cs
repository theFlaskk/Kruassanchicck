using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WeaponManager : MonoBehaviour
{
    [Header("Anchor")]
    public Transform weaponAnchor;

    [Header("Settings")]
    [Range(1f, 1000f)] public float rotateSpeed = 1000f;
    [Range(-180f, 180f)] public float angleOffset = -90f;

    [Header("Starting Weapons")]
    public WeaponBase startingWeaponSlot1;
    public WeaponBase startingWeaponSlot2;
    public WeaponBase startingWeaponSlot3;

    [Header("Pickup Prefabs")]
    public WeaponBase[] pickupPrefabs;

    [Header("Pickup Settings")]
    public bool replaceCurrentWeaponOnFull = true;

    private List<WeaponBase> inventory = new List<WeaponBase>(new WeaponBase[3]);
    private int currentSlotIndex = 0;
    private WeaponBase currentWeapon;

    private Camera mainCamera;
    private SpriteRenderer playerSprite;
    private bool isFacingLeft = false;

    public System.Action<int, WeaponBase> OnWeaponChanged;
    public System.Action<List<WeaponBase>> OnInventoryUpdated;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start() => InitializeInventory();

    private void InitializeInventory()
    {
        if (startingWeaponSlot1 != null) { inventory[0] = Instantiate(startingWeaponSlot1); inventory[0].gameObject.SetActive(false); }
        if (startingWeaponSlot2 != null) { inventory[1] = Instantiate(startingWeaponSlot2); inventory[1].gameObject.SetActive(false); }
        if (startingWeaponSlot3 != null) { inventory[2] = Instantiate(startingWeaponSlot3); inventory[2].gameObject.SetActive(false); }
        EquipFirstAvailableWeapon();
        OnInventoryUpdated?.Invoke(inventory);
    }

    private void EquipFirstAvailableWeapon()
    {
        for (int i = 0; i < 3; i++)
        {
            if (inventory[i] != null) { currentSlotIndex = i; EquipCurrentWeapon(); return; }
        }
        currentSlotIndex = 0;
        currentWeapon = null;
    }

    private void Update()
    {
        if (playerSprite == null || mainCamera == null) return;

        if (currentWeapon != null)
        {
            currentWeapon.WeaponUpdate();
            if (Input.GetButton("Fire1")) currentWeapon.TryShoot();
            if (Input.GetKeyDown(KeyCode.R)) currentWeapon.TryReload();
        }

        bool currentlyFacingLeft = playerSprite.flipX;
        if (currentlyFacingLeft != isFacingLeft)
        {
            isFacingLeft = currentlyFacingLeft;
            if (currentWeapon != null)
                currentWeapon.UpdateAimDirection(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        HandleWeaponSwitch();
        AimAtMouse();
    }

    private void HandleWeaponSwitch()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SwitchToNextWeapon();
        else if (scroll < 0f) SwitchToPreviousWeapon();
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToSlot(2);
    }

    private void SwitchToNextWeapon()
    {
        for (int i = 1; i <= 3; i++)
        {
            int slot = (currentSlotIndex + i) % 3;
            if (inventory[slot] != null) { SwitchToSlot(slot); return; }
        }
    }

    private void SwitchToPreviousWeapon()
    {
        for (int i = 1; i <= 3; i++)
        {
            int slot = (currentSlotIndex - i + 3) % 3;
            if (inventory[slot] != null) { SwitchToSlot(slot); return; }
        }
    }

    public void SwitchToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3 || inventory[slotIndex] == null || slotIndex == currentSlotIndex) return;
        if (currentWeapon != null) currentWeapon.OnUnequip();
        currentSlotIndex = slotIndex;
        EquipCurrentWeapon();
    }

    private void EquipCurrentWeapon()
    {
        currentWeapon = inventory[currentSlotIndex];
        if (currentWeapon == null) return;
        currentWeapon.OnEquip(weaponAnchor);
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.UpdateAimDirection(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        OnWeaponChanged?.Invoke(currentSlotIndex, currentWeapon);
        OnInventoryUpdated?.Invoke(inventory);
    }

    public void PickUpWeapon(WeaponBase weaponPrefab) => PickUpWeaponWithAmmo(weaponPrefab, -1, -1);

    public bool PickUpWeaponWithAmmo(WeaponBase weaponPrefab, int ammoInMag, int ammoReserve)
    {
        if (weaponPrefab == null) return false;

        int emptySlot = FindEmptySlot();
        WeaponBase newWeapon = null;

        if (emptySlot != -1)
        {
            newWeapon = Instantiate(weaponPrefab);
            inventory[emptySlot] = newWeapon;
            newWeapon.gameObject.SetActive(false);
            if (ammoInMag >= 0) newWeapon.SetAmmo(ammoInMag, 0);
            if (currentSlotIndex == emptySlot || currentWeapon == null)
            {
                currentSlotIndex = emptySlot;
                EquipCurrentWeapon();
            }
        }
        else if (replaceCurrentWeaponOnFull)
        {
            int oldSlotIndex = currentSlotIndex;
            WeaponBase oldWeapon = currentWeapon;

            if (oldWeapon != null) oldWeapon.OnUnequip();
            currentWeapon = null;
            inventory[oldSlotIndex] = null;

            WeaponBase pickupPrefab = FindPickupPrefabByName(oldWeapon.weaponName);
            if (pickupPrefab != null)
            {
                GameObject dropped = Instantiate(pickupPrefab.gameObject, transform.position, Quaternion.identity);
                WeaponDrop weaponDrop = dropped.GetComponent<WeaponDrop>();
                if (weaponDrop != null)
                    weaponDrop.savedAmmoInMag = oldWeapon.GetCurrentAmmoInMag();
            }

            if (oldWeapon != null) Destroy(oldWeapon.gameObject);

            newWeapon = Instantiate(weaponPrefab);
            inventory[oldSlotIndex] = newWeapon;
            newWeapon.gameObject.SetActive(false);
            if (ammoInMag >= 0) newWeapon.SetAmmo(ammoInMag, 0);

            currentSlotIndex = oldSlotIndex;
            EquipCurrentWeapon();
        }
        else
        {
            return false;
        }

        OnInventoryUpdated?.Invoke(inventory);
        return true;
    }

    public void DropCurrentWeaponAtPosition(Vector3 dropPosition, bool skipSwitch = false)
    {
        if (currentWeapon == null) return;

        WeaponBase pickupPrefab = FindPickupPrefabByName(currentWeapon.weaponName);
        if (pickupPrefab == null)
        {
            Destroy(currentWeapon.gameObject);
            inventory[currentSlotIndex] = null;
            if (!skipSwitch) SwitchToNextWeapon();
            return;
        }

        GameObject dropped = Instantiate(pickupPrefab.gameObject, dropPosition, Quaternion.identity);
        WeaponDrop weaponDrop = dropped.GetComponent<WeaponDrop>();
        if (weaponDrop != null)
        {
            weaponDrop.savedAmmoInMag = currentWeapon.GetCurrentAmmoInMag();
        }

        Destroy(currentWeapon.gameObject);
        inventory[currentSlotIndex] = null;
        if (!skipSwitch) SwitchToNextWeapon();
        OnInventoryUpdated?.Invoke(inventory);
    }

    public void DropCurrentWeapon() => DropCurrentWeaponAtPosition(transform.position);

    private WeaponBase FindPickupPrefabByName(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName) || pickupPrefabs == null) return null;
        return pickupPrefabs.FirstOrDefault(p => p != null && p.weaponName == weaponName);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < 3; i++) if (inventory[i] == null) return i;
        return -1;
    }

    private void AimAtMouse()
    {
        if (currentWeapon == null || mainCamera == null) return;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)mousePos - (Vector2)weaponAnchor.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
            currentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        currentWeapon.UpdateAimDirection(mousePos);
    }

    public WeaponBase GetCurrentWeapon() => currentWeapon;
    public int GetCurrentSlotIndex() => currentSlotIndex;
    public List<WeaponBase> GetInventory() => inventory;
    public string GetWeaponName(int slotIndex) => (slotIndex >= 0 && slotIndex < 3 && inventory[slotIndex] != null) ? inventory[slotIndex].weaponName : "Пусто";
    public bool IsSlotEmpty(int slotIndex) => slotIndex >= 0 && slotIndex < 3 && inventory[slotIndex] == null;
    public int GetEmptySlotsCount() { int c = 0; for (int i = 0; i < 3; i++) if (inventory[i] == null) c++; return c; }
}