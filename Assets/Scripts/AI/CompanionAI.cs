using UnityEngine;

/// <summary>
/// ИИ союзника: следует за активным игроком, атакует врагов.
/// Работает с WeaponManager для стрельбы.
/// </summary>
public class CompanionAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Цель для следования (активный игрок)")]
    public Transform followTarget;

    [Tooltip("WeaponManager союзника")]
    public WeaponManager weaponManager;

    [Tooltip("PlayerController союзника")]
    public PlayerController playerController;

    [Header("Follow Settings")]
    public float minFollowDistance = 2f;
    public float maxFollowDistance = 5f;
    public float moveSpeed = 3f;
    public float followPrecision = 0.5f;

    [Header("Combat Settings")]
    public float detectionRange = 12f;
    public float attackRange = 10f;
    public float fireCooldown = 1.5f;
    [Range(90f, 360f)]
    public float fieldOfView = 180f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public GameObject indicatorObject;

    private Rigidbody2D rb;
    private float fireTimer = 0f;
    private bool isActive = false;
    private LayerMask enemyLayer;
    private LayerMask obstacleLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerController == null) playerController = GetComponent<PlayerController>();

        enemyLayer = LayerMask.GetMask("Enemy");
        obstacleLayer = LayerMask.GetMask("Default", "Enemy");
    }

    private void Start()
    {
        SetActiveState(false);
    }

    private void Update()
    {
        if (!isActive || followTarget == null || !gameObject.activeInHierarchy) return;
        if (playerController != null && !playerController.IsAlive()) return;

        if (fireTimer > 0) fireTimer -= Time.deltaTime;

        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            CombatBehavior(nearestEnemy);
        }
        else
        {
            FollowBehavior();
        }

        UpdateIndicator();
    }

    private void FollowBehavior()
    {
        if (followTarget == null) return;

        float distance = Vector2.Distance(transform.position, followTarget.position);

        if (distance > maxFollowDistance)
        {
            Vector2 direction = (followTarget.position - transform.position).normalized;
            if (rb != null) rb.linearVelocity = direction * moveSpeed;
            FlipSprite(direction.x);
        }
        else if (distance < minFollowDistance)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 direction = (followTarget.position - transform.position).normalized;
            if (rb != null) rb.linearVelocity = direction * moveSpeed * followPrecision;
            FlipSprite(direction.x);
        }
    }

    private void CombatBehavior(Transform enemy)
    {
        float distance = Vector2.Distance(transform.position, enemy.position);
        Vector2 direction = (enemy.position - transform.position).normalized;
        FlipSprite(direction.x);

        if (distance <= attackRange)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;

            if (fireTimer <= 0 && weaponManager != null)
            {
                weaponManager.ForceAimAtPosition(enemy.position);
                weaponManager.TryShootCurrent();
                fireTimer = fireCooldown;
                Debug.Log($"[CompanionAI] 💥 Атака: {enemy.name}");
            }
        }
        else if (distance <= detectionRange)
        {
            if (rb != null) rb.linearVelocity = direction * moveSpeed;
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);

        Transform nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Vector2 direction = (hit.transform.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, hit.transform.position);

            float angle = Vector2.Angle(transform.right, direction);
            if (angle > fieldOfView / 2) continue;

            RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
            if (ray.collider != hit) continue;

            if (distance < nearestDist)
            {
                nearestDist = distance;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    private void FlipSprite(float directionX)
    {
        if (spriteRenderer == null || directionX == 0) return;
        spriteRenderer.flipX = directionX < 0;
    }

    private void UpdateIndicator()
    {
        if (indicatorObject != null)
        {
            bool enemyNearby = Physics2D.OverlapCircle(transform.position, detectionRange, enemyLayer) != null;
            indicatorObject.SetActive(enemyNearby);
        }
    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        if (!active && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log($"[CompanionAI] 🤖 Состояние: {(active ? "✅ АКТИВЕН" : "⏸️ ОЖИДАНИЕ")}");
    }

    public void SetFollowTarget(Transform target) => followTarget = target;
    public bool IsActive() => isActive;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}