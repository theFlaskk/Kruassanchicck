using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI для отображения текущего оружия и патронов в магазине.
/// Работает с упрощённой системой (без резерва).
/// </summary>
public class AmmoUI : MonoBehaviour
{
    [Header("Weapon Info")]
    [Tooltip("Текст названия оружия")]
    public GameObject weaponNameObj;

    [Header("Ammo Texts")]
    [Tooltip("Текст: патроны в магазине")]
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
        // ✅ Используем новый метод Unity 2023.1+
        weaponManager = FindFirstObjectByType<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError("[AmmoUI] ❌ WeaponManager не найден!");
            enabled = false;
            return;
        }

        // Подписываемся на события
        weaponManager.OnWeaponChanged += OnWeaponChanged;
        weaponManager.OnInventoryUpdated += OnInventoryUpdated;

        // Кэшируем компоненты
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
    }

    private void CacheText(GameObject obj, ref TextMeshProUGUI text, string name)
    {
        if (obj == null) return;
        text = obj.GetComponent<TextMeshProUGUI>();
        if (text != null)
            Debug.Log($"[AmmoUI] 📝 {name}: ✅ НАЙДЕН");
    }

    private void Update()
    {
        if (weaponManager != null)
        {
            WeaponBase newWeapon = weaponManager.GetCurrentWeapon();
            int newSlotIndex = weaponManager.GetCurrentSlotIndex();

            if (newWeapon != currentWeapon || newSlotIndex != currentSlotIndex)
            {
                if (currentWeapon != null)
                {
                    currentWeapon.OnAmmoUpdated -= UpdateAmmo;
                    currentWeapon.OnReloadUpdated -= UpdateReload;
                }

                currentWeapon = newWeapon;
                currentSlotIndex = newSlotIndex;

                if (currentWeapon != null)
                {
                    currentWeapon.OnAmmoUpdated += UpdateAmmo;
                    currentWeapon.OnReloadUpdated += UpdateReload;

                    if (weaponNameText != null)
                        weaponNameText.text = currentWeapon.weaponName;

                    // ✅ Обновляем патроны сразу
                    UpdateAmmo(
                        currentWeapon.GetCurrentAmmoInMag(),
                        0,  // ✅ Резерв всегда 0
                        currentWeapon.GetMaxMagazineSize()
                    );

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
    /// Обновляет тексты патронов (резерв игнорируется)
    /// </summary>
    private void UpdateAmmo(int inMag, int inReserve, int maxMag)
    {
        // ✅ Лог только с магазином
        Debug.Log($"[AmmoUI] 📊 Патроны: {inMag} / {maxMag}");

        if (ammoText != null)
            ammoText.text = inMag.ToString();

        // ✅ Показываем ёмкость магазина вместо резерва
        if (maxText != null)
            maxText.text = $"/ {maxMag}";

        // Цвет текста в зависимости от количества патронов
        if (ammoText != null)
        {
            if (inMag == 0)
                ammoText.color = emptyAmmoColor;
            else if (inMag <= maxMag * 0.25f)
                ammoText.color = lowAmmoColor;
            else
                ammoText.color = normalColor;
        }
    }

    private void UpdateReload(bool isReloading, float progress)
    {
        if (reloadTextObj != null)
            reloadTextObj.SetActive(isReloading);

        if (reloadSliderComp != null)
        {
            reloadSliderComp.gameObject.SetActive(isReloading);
            reloadSliderComp.value = Mathf.Clamp01(progress);
        }
    }

    /// <summary>
    /// Обновить индикаторы слотов инвентаря
    /// </summary>
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