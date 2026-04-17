using UnityEngine;

/// <summary>
/// Главный контроллер игрока.
/// Объединяет Movement, Animation, Stats и поддержку переключения.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(WeaponManager))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Ссылка на Movement (заполняется автоматически)")]
    public PlayerMovement movement;

    [Tooltip("Ссылка на Animation (заполняется автоматически)")]
    public PlayerAnimation animationController;

    [Tooltip("Ссылка на Stats (заполняется автоматически)")]
    public PlayerStats stats;

    [Tooltip("Ссылка на WeaponManager (заполняется автоматически)")]
    public WeaponManager weaponManager;

    [Header("Switch Settings")]
    [Tooltip("Может ли игрок получать ввод")]
    private bool canReceiveInput = true;

    [Tooltip("Индикатор активного игрока")]
    public GameObject activeIndicator;

    // Компоненты
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Camera mainCamera;

    // События
    public System.Action<bool> OnControlStateChanged;

    private void Awake()
    {
        // Автоматическое получение компонентов
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (animationController == null) animationController = GetComponent<PlayerAnimation>();
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();

        // Проверка критических компонентов
        if (movement == null || stats == null)
        {
            Debug.LogError($"[PlayerController] ❌ Критические компоненты отсутствуют на {gameObject.name}!");
            enabled = false;
        }
    }

    private void Start()
    {
        Debug.Log($"[PlayerController] ✅ Игрок инициализирован: {gameObject.name}");

        // Подписка на события Stats (смерть)
        if (stats != null)
        {
            stats.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        // ✅ ✅ ✅ ЖЁСТКАЯ ПРОВЕРКА В САМОМ НАЧАЛЕ
        if (!canReceiveInput)
        {
            return;  // Ничего не делаем если управление выключено!
        }

        // ✅ ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: сверяемся с менеджером
        if (PlayerSwitchManager.Instance != null)
        {
            if (PlayerSwitchManager.Instance.GetActivePlayer() != this)
            {
                return;  // Это не активный игрок — не обрабатываем ввод!
            }
        }

        // Получаем ввод
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(moveX, moveY).normalized;

        // ✅ ОТЛАДКА: лог движения
        if (moveInput.magnitude > 0.1f)
        {
            Debug.Log($"[PlayerController] 🎮 {gameObject.name}: Движение {moveInput}");
        }

        // Передаём ввод в Movement
        if (movement != null)
        {
            movement.HandleMovement(moveInput);
        }

        // Передаём направление в Animation
        if (animationController != null)
        {
            Vector2 velocity = movement != null ? movement.GetVelocity() : Vector2.zero;
            animationController.HandleAnimation(moveInput, velocity);
        }

        // Обновляем WeaponManager (если есть)
        if (weaponManager != null)
        {
            weaponManager.enabled = true;
        }
    }

    /// <summary>
    /// Включить/выключить управление игроком
    /// </summary>
    public void SetControlEnabled(bool enabled)
    {
        canReceiveInput = enabled;

        Debug.Log($"[PlayerController] 🎮 {gameObject.name}: Управление = {(enabled ? "✅ ВКЛ" : "❌ ВЫКЛ")}");

        // Визуальный эффект
        if (sr != null)
        {
            sr.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }

        // ✅ Остановить движение если отключили
        if (!enabled)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                Debug.Log($"[PlayerController] 🛑 {gameObject.name} остановлен (velocity сброшен)");
            }

            if (movement != null)
            {
                movement.Stop();  // Останавливаем Movement тоже!
                Debug.Log($"[PlayerController] 🛑 {gameObject.name} Movement остановлен");
            }
        }

        // Отключить WeaponManager если не активен
        if (weaponManager != null)
        {
            weaponManager.enabled = enabled;
            Debug.Log($"[PlayerController] 🔫 WeaponManager = {(enabled ? "✅ ВКЛ" : "❌ ВЫКЛ")}");
        }

        // Индикатор
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(enabled);
        }

        // Событие
        OnControlStateChanged?.Invoke(enabled);
    }

    /// <summary>
    /// Проверка: может ли игрок получать ввод
    /// </summary>
    public bool CanReceiveInput() => canReceiveInput;

    /// <summary>
    /// Обработка смерти игрока
    /// </summary>
    private void HandleDeath()
    {
        Debug.Log($"[PlayerController] ☠️ {gameObject.name} умер!");

        // Отключаем управление
        SetControlEnabled(false);

        // Если есть менеджер переключения — переключаемся на другого
        if (PlayerSwitchManager.Instance != null)
        {
            PlayerSwitchManager.Instance.OnPlayerDeath(this);
        }
    }

    /// <summary>
    /// Получить текущее здоровье
    /// </summary>
    public int GetCurrentHealth() => stats != null ? stats.currentHealth : 0;

    /// <summary>
    /// Получить макс. здоровье
    /// </summary>
    public int GetMaxHealth() => stats != null ? stats.maxHealth : 100;

    /// <summary>
    /// Проверка: игрок жив?
    /// </summary>
    public bool IsAlive() => stats != null && stats.currentHealth > 0;

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDeath -= HandleDeath;
        }
    }
}