using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI для отображения текущего оружия и патронов в магазине.
/// </summary>
public class AmmoUI : MonoBehaviour
{
    [Header("Weapon Info")]
    [Tooltip("Текст названия оружия")]
    public GameObject weaponNameObj;

    [Header("Ammo Texts")]
    [Tooltip("Текст: текущие патроны в магазине (например '12')")]
    public GameObject ammoInMagObj;

    [Tooltip("Текст: ёмкость магазина (например '/ 12')")]
    public GameObject maxMagObj;

    [Header("Reload UI")]
    public GameObject reloadTextObj;
    public Slider reloadSlider;

    [Header("Inventory Slots")]
    [Tooltip("3 объекта для отображения слотов инвентаря")]
    public GameObject[] slotIndicators;

    [Tooltip("Тексты для каждого слота (опционально)")]
    public GameObject[] slotTexts;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color lowAmmoColor = Color.yellow;
    public Color emptyAmmoColor = Color.red;
    public Color activeSlotColor = Color.green;
    public Color inactiveSlotColor = Color.gray;
    public Color emptySlotColor = Color.darkGray;

    // Компоненты
    private TextMeshProUGUI weaponNameText;
    private TextMeshProUGUI ammoText;
    private TextMeshProUGUI maxText;
    private Slider reloadSliderComp;
    private Image[] slotImages;
    private TextMeshProUGUI[] slotTextComponents;

    private WeaponManager weaponManager;
    private WeaponBase currentWeapon;
    private int currentSlotIndex = -1;

    private void Start()
    {
        Debug.Log("[AmmoUI] 🔍 Start() начался");

        weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError("[AmmoUI] ❌ WeaponManager не найден!");
            enabled = false;
            return;
        }

        Debug.Log("[AmmoUI] ✅ WeaponManager найден");

        // Подписываемся на события
        weaponManager.OnWeaponChanged += OnWeaponChanged;
        weaponManager.OnInventoryUpdated += OnInventoryUpdated;

        // Кэшируем компоненты С ПРОВЕРКОЙ
        CacheText(weaponNameObj, ref weaponNameText, "WeaponName");
        CacheText(ammoInMagObj, ref ammoText, "AmmoInMag");
        CacheText(maxMagObj, ref maxText, "MaxMag");

        if (reloadSlider != null)
            reloadSliderComp = reloadSlider.GetComponent<Slider>();

        // Кэшируем индикаторы слотов
        if (slotIndicators != null && slotIndicators.Length > 0)
        {
            slotImages = new Image[slotIndicators.Length];
            for (int i = 0; i < slotIndicators.Length; i++)
            {
                if (slotIndicators[i] != null)
                    slotImages[i] = slotIndicators[i].GetComponent<Image>();
            }
        }

        // Кэшируем тексты слотов
        if (slotTexts != null && slotTexts.Length > 0)
        {
            slotTextComponents = new TextMeshProUGUI[slotTexts.Length];
            for (int i = 0; i < slotTexts.Length; i++)
            {
                if (slotTexts[i] != null)
                    slotTextComponents[i] = slotTexts[i].GetComponent<TextMeshProUGUI>();
            }
        }

        Debug.Log("[AmmoUI] ✅ UI запущен");

        // ✅ ПРОВЕРКА: отображаем тестовые значения
        Debug.Log($"[AmmoUI] 📝 ammoText: {(ammoText != null ? "✅" : "❌")}");
        Debug.Log($"[AmmoUI] 📝 maxText: {(maxText != null ? "✅" : "❌")}");
    }

    private void CacheText(GameObject obj, ref TextMeshProUGUI text, string name)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[AmmoUI] ⚠️ {name}Obj = NULL! Перетащи объект в Inspector");
            return;
        }

        text = obj.GetComponent<TextMeshProUGUI>();
        if (text != null)
            Debug.Log($"[AmmoUI] 📝 {name}: ✅ НАЙДЕН на объекте {obj.name}");
        else
            Debug.LogError($"[AmmoUI] ❌ {name}: НЕ НАЙДЕН TextMeshPro на {obj.name}! Добавь компонент!");
    }

    private void Update()
    {
        if (weaponManager != null)
        {
            WeaponBase newWeapon = weaponManager.GetCurrentWeapon();
            int newSlotIndex = weaponManager.GetCurrentSlotIndex();

            if (newWeapon != currentWeapon || newSlotIndex != currentSlotIndex)
            {
                Debug.Log($"[AmmoUI] 🔄 Смена оружия: {currentWeapon?.weaponName ?? "NULL"} → {newWeapon?.weaponName ?? "NULL"}");

                if (currentWeapon != null)
                {
                    currentWeapon.OnAmmoUpdated -= UpdateAmmo;
                    currentWeapon.OnReloadUpdated -= UpdateReload;
                }

                currentWeapon = newWeapon;
                currentSlotIndex = newSlotIndex;

                if (currentWeapon != null)
                {
                    // ✅ Подписываемся на события оружия
                    currentWeapon.OnAmmoUpdated += UpdateAmmo;
                    currentWeapon.OnReloadUpdated += UpdateReload;

                    Debug.Log($"[AmmoUI] ✅ Подписан на события: {currentWeapon.weaponName}");

                    if (weaponNameText != null)
                        weaponNameText.text = currentWeapon.weaponName;

                    // ✅ Обновляем патроны сразу при смене оружия
                    int ammo = currentWeapon.GetCurrentAmmoInMag();
                    int maxMag = currentWeapon.GetMaxMagazineSize();
                    Debug.Log($"[AmmoUI] 📊 Начальные патроны: {ammo} / {maxMag}");

                    UpdateAmmo(ammo, 0, maxMag);
                    UpdateSlotIndicators();
                }
            }
        }
    }

    private void OnWeaponChanged(int slotIndex, WeaponBase weapon)
    {
        Debug.Log($"[AmmoUI] 🔄 Смена оружия: Слот {slotIndex + 1} — {weapon.weaponName}");
    }

    private void OnInventoryUpdated(System.Collections.Generic.List<WeaponBase> inventory)
    {
        Debug.Log("[AmmoUI] 🎒 Инвентарь обновлён");
        UpdateSlotIndicators();
    }

    /// <summary>
    /// Вызывается СОБЫТИЕМ из WeaponBase при изменении патронов
    /// </summary>
    private void UpdateAmmo(int inMag, int inReserve, int maxMag)
    {
        // ✅ ОТЛАДКА: подробный лог
        Debug.Log($"[AmmoUI] 📊 UpdateAmmo вызван: {inMag} / {maxMag}");
        Debug.Log($"[AmmoUI] 📝 ammoText = {(ammoText != null ? "✅" : "❌")}");
        Debug.Log($"[AmmoUI] 📝 maxText = {(maxText != null ? "✅" : "❌")}");

        // ✅ Обновляем текущие патроны
        if (ammoText != null)
        {
            ammoText.text = inMag.ToString();
            Debug.Log($"[AmmoUI] ✏️ ammoText.text = '{ammoText.text}'");
        }
        else
        {
            Debug.LogError("[AmmoUI] ❌ ammoText = NULL! Текст не обновится!");
        }

        // ✅ Обновляем ёмкость магазина
        if (maxText != null)
        {
            maxText.text = $"/ {maxMag}";
            Debug.Log($"[AmmoUI] ✏️ maxText.text = '{maxText.text}'");
        }
        else
        {
            Debug.LogWarning("[AmmoUI] ⚠️ maxText = NULL! Ёмкость не отобразится!");
        }

        // ✅ Цвет текста в зависимости от количества патронов
        if (ammoText != null)
        {
            if (inMag == 0)
            {
                ammoText.color = emptyAmmoColor;
                Debug.Log("[AmmoUI] 🎨 Цвет: КРАСНЫЙ (пусто)");
            }
            else if (inMag <= maxMag * 0.25f)
            {
                ammoText.color = lowAmmoColor;
                Debug.Log("[AmmoUI] 🎨 Цвет: ЖЁЛТЫЙ (мало)");
            }
            else
            {
                ammoText.color = normalColor;
                Debug.Log("[AmmoUI] 🎨 Цвет: БЕЛЫЙ (норма)");
            }
        }
    }

    private void UpdateReload(bool isReloading, float progress)
    {
        Debug.Log($"[AmmoUI] 🔄 UpdateReload: {isReloading} | {progress:P0}");

        if (reloadTextObj != null)
            reloadTextObj.SetActive(isReloading);

        if (reloadSliderComp != null)
        {
            reloadSliderComp.gameObject.SetActive(isReloading);
            reloadSliderComp.value = Mathf.Clamp01(progress);
        }
    }

    private void UpdateSlotIndicators()
    {
        if (slotImages == null || slotImages.Length < 3) return;

        for (int i = 0; i < 3; i++)
        {
            bool isEmpty = weaponManager.IsSlotEmpty(i);
            bool isActive = (i == currentSlotIndex);

            if (slotImages[i] != null)
            {
                if (isEmpty)
                    slotImages[i].color = emptySlotColor;
                else if (isActive)
                    slotImages[i].color = activeSlotColor;
                else
                    slotImages[i].color = inactiveSlotColor;
            }

            if (slotTextComponents != null && i < slotTextComponents.Length && slotTextComponents[i] != null)
            {
                slotTextComponents[i].text = weaponManager.GetWeaponName(i);
            }
        }
    }

    private void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
            weaponManager.OnInventoryUpdated -= OnInventoryUpdated;
        }

        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoUpdated -= UpdateAmmo;
            currentWeapon.OnReloadUpdated -= UpdateReload;
        }
    }
}