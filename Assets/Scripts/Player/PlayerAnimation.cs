using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Разворачивать спрайт по направлению движения")]
    public bool flipOnMove = true;

    [Tooltip("Аниматор (если есть)")]
    public Animator animator;

    private SpriteRenderer sr;
    private bool isMoving = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Обработать анимацию (вызывается из PlayerController)
    /// </summary>
    public void HandleAnimation(Vector2 moveInput, Vector2 velocity)
    {
        if (sr == null) return;

        // Определение движения
        isMoving = velocity.magnitude > 0.1f;

        // Разворот спрайта
        if (flipOnMove && moveInput.x != 0)
        {
            sr.flipX = moveInput.x < 0;
        }

        // Обновление аниматора
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
    }

    /// <summary>
    /// Проверка: игрок движется?
    /// </summary>
    public bool IsMoving() => isMoving;
}