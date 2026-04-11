using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Settings")]
    public float mouseDeadZone = 0.1f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    // УБРАЛИ DirectionHash — он больше не нужен!

    private bool isFacingLeft = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null) Debug.LogError("[PlayerAnimation] Не найден Animator!");
        if (spriteRenderer == null) Debug.LogError("[PlayerAnimation] Не найден SpriteRenderer!");
        if (mainCamera == null) Debug.LogError("[PlayerAnimation] Не найдена камера!");
    }

    private void Update()
    {
        if (animator == null || mainCamera == null || spriteRenderer == null) return;

        // 1. Движение
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(moveX, moveY);

        if (movement.sqrMagnitude > 1f) movement.Normalize();

        // Передаем ТОЛЬКО скорость (не направление!)
        animator.SetFloat(SpeedHash, movement.magnitude);

        // 2. Направление по мыши
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)mousePos - (Vector2)transform.position;

        if (Mathf.Abs(direction.x) > mouseDeadZone)
        {
            isFacingLeft = direction.x < 0;
        }

        // УБРАЛИ animator.SetInteger — больше не передаём направление!
    }

    private void LateUpdate()
    {
        // Принудительно устанавливаем flipX КАЖДЫЙ КАДР
        spriteRenderer.flipX = isFacingLeft;
    }
}