using UnityEngine;

/// <summary>
/// Триггер подбора оружия на клавишу E.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon to Give")]
    [Tooltip("Префаб оружия которое подберёт игрок")]
    public WeaponBase weaponPrefab;

    [Header("Visual")]
    [Tooltip("Вращать оружие на земле")]
    public bool rotateOnGround = true;

    [Tooltip("Скорость вращения")]
    public float rotateSpeed = 50f;

    [Header("Bobbing")]
    [Tooltip("Покачивать оружие вверх-вниз")]
    public bool bobOnGround = true;

    [Tooltip("Амплитуда покачивания")]
    public float bobAmount = 0.3f;

    [Tooltip("Скорость покачивания")]
    public float bobSpeed = 2f;

    // Ссылка на игрока в зоне подбора
    private GameObject playerInRange;
    private WeaponManager playerWeaponManager;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Вращение для красоты
        if (rotateOnGround)
        {
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        }

        // Покачивание вверх-вниз
        if (bobOnGround)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // ✅ Проверка нажатия E если игрок в зоне
        if (playerInRange != null && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем что это игрок
        if (other.CompareTag("Player"))
        {
            playerInRange = other.gameObject;
            playerWeaponManager = other.GetComponent<WeaponManager>();

            Debug.Log($"[WeaponPickup] 📍 Игрок в зоне: {weaponPrefab?.weaponName}");

            // ✅ Показываем подсказку
            UIManager.Instance?.ShowPickupPrompt(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = null;
            playerWeaponManager = null;

            Debug.Log($"[WeaponPickup] 📍 Игрок вышел из зоны");

            // ✅ Скрываем подсказку
            UIManager.Instance?.ShowPickupPrompt(false);
        }
    }

    private void PickUp()
    {
        if (playerWeaponManager != null && weaponPrefab != null)
        {
            // Подбираем оружие
            playerWeaponManager.PickUpWeapon(weaponPrefab);

            Debug.Log($"[WeaponPickup] ✅ Оружие подобрано: {weaponPrefab.weaponName}");

            // ✅ Скрываем подсказку
            UIManager.Instance?.ShowPickupPrompt(false);

            // Уничтожаем оружие с земли
            Destroy(gameObject);
        }
    }
}