using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Скорость передвижения")]
    public float moveSpeed = 5f;

    [Tooltip("Ускорение")]
    public float acceleration = 10f;

    [Tooltip("Замедление")]
    public float deceleration = 10f;

    private Rigidbody2D rb;
    private Vector2 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Обработать ввод движения (вызывается из PlayerController)
    /// </summary>
    public void HandleMovement(Vector2 moveInput)
    {
        // ✅ ЖЁСТКАЯ ПРОВЕРКА
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && !controller.CanReceiveInput())
        {
            return;  // Не двигаемся если управление выключено!
        }

        if (rb == null) return;

        // Плавное ускорение/замедление
        if (moveInput.magnitude > 0.1f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, moveInput * moveSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }

        rb.linearVelocity = currentVelocity;
    }

    /// <summary>
    /// Получить текущую скорость
    /// </summary>
    public Vector2 GetVelocity() => rb != null ? rb.linearVelocity : Vector2.zero;

    /// <summary>
    /// Остановить игрока
    /// </summary>
    public void Stop()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        Debug.Log($"[PlayerMovement] 🛑 {gameObject.name} остановлен");
    }
}