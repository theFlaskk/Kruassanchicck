using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats stats;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        // Считываем ввод
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(moveX, moveY).normalized;

        // ✅ ЗДЕСЬ БОЛЬШЕ НЕТ НИЧЕГО ПРО АНИМАЦИИ И ПОВОРОТЫ
        // Этим занимается скрипт PlayerAnimation и WeaponAim
    }

    private void FixedUpdate()
    {
        // Физическое движение
        // Примечание: rb.linearVelocity работает в Unity 2022.2+, если у тебя ошибка, замени на rb.velocity
        rb.linearVelocity = movementInput * stats.CurrentMoveSpeed;
    }
}