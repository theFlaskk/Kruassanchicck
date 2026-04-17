using UnityEngine;

/// <summary>
/// Менеджер переключения между двумя игроками.
/// Камера следует за активным, неактивный становится ИИ-компаньоном.
/// </summary>
public class PlayerSwitchManager : MonoBehaviour
{
    public static PlayerSwitchManager Instance { get; private set; }

    [Header("Players")]
    [Tooltip("Игрок 1")]
    public PlayerController player1;

    [Tooltip("Игрок 2")]
    public PlayerController player2;

    [Header("Camera")]
    [Tooltip("Основная камера")]
    public Camera mainCamera;

    [Tooltip("Скорость слежения камеры")]
    public float cameraFollowSpeed = 8f;

    [Tooltip("Смещение камеры по X и Y (Z всегда -10)")]
    public Vector2 cameraOffsetXY = new Vector2(0f, 0f);

    [Tooltip("Фиксированная Z-позиция камеры для 2D")]
    public float cameraZPosition = -10f;

    [Tooltip("Размер ортографической камеры (больше = дальше обзор)")]
    public float cameraOrthographicSize = 8f;

    [Header("Switch Settings")]
    [Tooltip("Клавиша переключения")]
    public KeyCode switchKey = KeyCode.Tab;

    [Tooltip("Задержка между переключениями")]
    public float switchCooldown = 0.5f;

    [Tooltip("Эффект при переключении")]
    public GameObject switchEffectPrefab;

    [Header("Companion AI")]
    [Tooltip("Скрипт ИИ союзника на неактивном игроке")]
    public CompanionAI companionAI;

    // Внутренние
    private PlayerController activePlayer;
    private PlayerController inactivePlayer;
    private float switchTimer = 0f;
    private int activeIndex = 0; // 0 = player1, 1 = player2

    // События
    public System.Action<int> OnPlayerSwitched; // Для обновления UI

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("[PlayerSwitchManager] ❌ Игроки не назначены!");
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[PlayerSwitchManager] ❌ Камера не найдена!");
                enabled = false;
                return;
            }
        }

        // ✅ Настройка камеры
        SetupCamera();

        activePlayer = player1;
        inactivePlayer = player2;
        activeIndex = 0;

        // Убедимся что оба игрока активны на сцене
        activePlayer.gameObject.SetActive(true);
        inactivePlayer.gameObject.SetActive(true);

        SetupPlayerStates();

        if (mainCamera != null && activePlayer != null)
        {
            mainCamera.transform.position = new Vector3(
                activePlayer.transform.position.x + cameraOffsetXY.x,
                activePlayer.transform.position.y + cameraOffsetXY.y,
                cameraZPosition
            );
            Debug.Log($"[PlayerSwitchManager] 📷 Камера установлена на: {mainCamera.transform.position}");
        }

        if (companionAI != null)
        {
            companionAI.SetFollowTarget(activePlayer.transform);
            companionAI.SetActiveState(true);
        }

        Debug.Log($"[PlayerSwitchManager] ✅ Запущен | Активен: Игрок {activeIndex + 1}");
        Debug.Log($"[PlayerSwitchManager] 📷 Камера: {mainCamera.name} | Z: {mainCamera.transform.position.z} | Size: {cameraOrthographicSize}");
    }

    /// <summary>
    /// Настройка камеры для 2D
    /// </summary>
    private void SetupCamera()
    {
        if (mainCamera == null) return;

        // Orthographic для 2D
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = cameraOrthographicSize;

        // Culling Mask - всё включено
        mainCamera.cullingMask = -1; // Everything

        // Background цвет
        mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.3f);

        // ✅ Z-позиция фиксирована!
        Vector3 pos = mainCamera.transform.position;
        pos.z = cameraZPosition;
        mainCamera.transform.position = pos;

        Debug.Log($"[PlayerSwitchManager] 📷 Камера настроена: Z = {cameraZPosition}, Size = {cameraOrthographicSize}");
    }

    private void Update()
    {
        if (switchTimer > 0) switchTimer -= Time.deltaTime;

        if (Input.GetKeyDown(switchKey) && switchTimer <= 0f)
        {
            SwitchPlayer();
        }

        UpdateCamera();
    }

    /// <summary>
    /// Переключить активного игрока
    /// </summary>
    public void SwitchPlayer()
    {
        if (switchTimer > 0f) return;

        Debug.Log($"[PlayerSwitchManager] 🔄 Переключение: Игрок {activeIndex + 1} → {1 - activeIndex + 1}");

        if (switchEffectPrefab != null)
        {
            Instantiate(switchEffectPrefab, activePlayer.transform.position, Quaternion.identity);
        }

        // Меняем роли
        var temp = activePlayer;
        activePlayer = inactivePlayer;
        inactivePlayer = temp;
        activeIndex = 1 - activeIndex;

        Debug.Log($"[PlayerSwitchManager] 🔄 Теперь активен: {activePlayer.gameObject.name}");

        // Обновляем состояния
        SetupPlayerStates();

        // Обновляем ИИ компаньона
        if (companionAI != null)
        {
            companionAI.SetFollowTarget(activePlayer.transform);
            companionAI.SetActiveState(true);
        }

        // Событие для UI
        OnPlayerSwitched?.Invoke(activeIndex);

        // Сброс кулдауна
        switchTimer = switchCooldown;
    }

    /// <summary>
    /// Настроить состояния игроков (активный/неактивный)
    /// </summary>
    private void SetupPlayerStates()
    {
        Debug.Log($"[PlayerSwitchManager] 🔧 SetupPlayerStates() вызван");

        if (activePlayer != null)
        {
            activePlayer.SetControlEnabled(true);
            int playerLayer = LayerMask.NameToLayer("Player");
            activePlayer.gameObject.layer = playerLayer != -1 ? playerLayer : 0;
            Debug.Log($"[PlayerSwitchManager] ✅ {activePlayer.gameObject.name} активирован");
        }

        if (inactivePlayer != null)
        {
            inactivePlayer.SetControlEnabled(false);
            int companionLayer = LayerMask.NameToLayer("Companion");
            inactivePlayer.gameObject.layer = companionLayer != -1 ? companionLayer : 0;
            Debug.Log($"[PlayerSwitchManager] ❌ {inactivePlayer.gameObject.name} деактивирован");
        }
    }

    /// <summary>
    /// Обновление камеры с фиксированным Z
    /// </summary>
    private void UpdateCamera()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[UpdateCamera] ⚠️ mainCamera = NULL!");
            return;
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[UpdateCamera] ⚠️ activePlayer = NULL!");
            return;
        }

        if (!activePlayer.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[UpdateCamera] ⚠️ activePlayer не активен!");
            return;
        }

        // ✅ XY следует за игроком, Z фиксирован
        Vector3 targetPosition = new Vector3(
            activePlayer.transform.position.x + cameraOffsetXY.x,
            activePlayer.transform.position.y + cameraOffsetXY.y,
            cameraZPosition  // ← Z ВСЕГДА -10!
        );

        // ✅ ОТЛАДКА: Лог каждые 60 кадров
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Camera] 📍 Игрок: {activePlayer.transform.position} | Камера: {mainCamera.transform.position} | Цель: {targetPosition}");
        }

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );

        // ✅ Защита от сброса Z
        if (Mathf.Abs(mainCamera.transform.position.z - cameraZPosition) > 0.01f)
        {
            Vector3 pos = mainCamera.transform.position;
            pos.z = cameraZPosition;
            mainCamera.transform.position = pos;
            Debug.LogWarning($"[PlayerSwitchManager] ⚠️ Z камеры сброшен! Восстановлен: {cameraZPosition}");
        }
    }

    /// <summary>
    /// Вызывается когда игрок умирает
    /// </summary>
    public void OnPlayerDeath(PlayerController deadPlayer)
    {
        if (deadPlayer == activePlayer)
        {
            Debug.Log("[PlayerSwitchManager] ⚠️ Активный игрок умер! Переключаемся...");
            SwitchPlayer();
        }
    }

    /// <summary>
    /// Получить активного игрока
    /// </summary>
    public PlayerController GetActivePlayer() => activePlayer;

    /// <summary>
    /// Получить индекс активного игрока (0 или 1)
    /// </summary>
    public int GetActiveIndex() => activeIndex;

    /// <summary>
    /// Можно ли переключаться?
    /// </summary>
    public bool CanSwitch() => switchTimer <= 0f;
}