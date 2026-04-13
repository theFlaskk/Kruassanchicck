using UnityEngine;

/// <summary>
/// ИИ врага: обнаруживает игрока, преследует и атакует.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public EnemyBase enemyBase;

    [Header("Detection")]
    [Tooltip("Радиус обнаружения игрока")]
    public float detectionRange = 15f;

    [Tooltip("Радиус атаки")]
    public float attackRange = 10f;

    [Tooltip("Минимальная дистанция атаки")]
    public float minAttackDistance = 5f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float patrolSpeed = 2f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    public bool randomPatrol = true;
    public float patrolRadius = 5f;

    [Header("Attack")]
    public float fireCooldown = 1.5f;
    public float accuracy = 0.1f;

    private enum AIState { Patrol, Chase }
    private AIState currentState = AIState.Patrol;

    private Transform player;
    private EnemyWeapon enemyWeapon;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private Vector3 randomPatrolTarget;
    private float fireTimer = 0f;
    private bool canSeePlayer = false;
    private float distanceToPlayer = 0f;

    private LayerMask detectableLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyWeapon = GetComponent<EnemyWeapon>();
        if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[EnemyAI] ❌ Игрок НЕ найден! Проверь Tag = 'Player'");
        }

        detectableLayer = LayerMask.GetMask("Player");
    }

    private void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            randomPatrolTarget = patrolPoints[0].position;
        }
        else if (randomPatrol)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            randomPatrolTarget = new Vector3(
                transform.position.x + randomCircle.x,
                transform.position.y + randomCircle.y,
                transform.position.z
            );
        }
    }

    private void Update()
    {
        if (enemyBase == null || !enemyBase.IsAlive()) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else return;
        }

        // ✅ ✅ ✅ ВЫЗЫВАЕМ WeaponUpdate() для оружия врага (УМЕНЬШАЕТ fireTimer!)
        if (enemyWeapon != null && enemyWeapon.currentWeapon != null)
        {
            enemyWeapon.currentWeapon.WeaponUpdate();
        }

        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        canSeePlayer = CanSeePlayer();

        Debug.DrawRay(
            transform.position,
            (player.position - transform.position).normalized * detectionRange,
            canSeePlayer ? Color.green : Color.red,
            0.1f
        );

        switch (currentState)
        {
            case AIState.Patrol: UpdatePatrol(); break;
            case AIState.Chase: UpdateChase(); break;
        }

        // Кулдаун ИИ
        if (fireTimer > 0) fireTimer -= Time.deltaTime;
    }

    private void UpdatePatrol()
    {
        Vector2 direction = (randomPatrolTarget - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, randomPatrolTarget);

        if (distance < 0.5f)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0)
            {
                SelectNextPatrolPoint();
                patrolWaitTimer = patrolWaitTime;
            }
        }
        else
        {
            rb.linearVelocity = direction * patrolSpeed;
            FlipSprite(direction.x);
        }

        if (canSeePlayer)
        {
            ChangeState(AIState.Chase);
        }
    }

    private void UpdateChase()
    {
        if (player == null) { ChangeState(AIState.Patrol); return; }

        if (!canSeePlayer && distanceToPlayer > detectionRange)
        {
            ChangeState(AIState.Patrol);
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;

        if (distanceToPlayer > minAttackDistance)
        {
            rb.linearVelocity = direction * moveSpeed;
            FlipSprite(direction.x);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Атака
        if (canSeePlayer && distanceToPlayer <= attackRange)
        {
            if (enemyWeapon != null && fireTimer <= 0)
            {
                enemyWeapon.TryShoot(direction);
                fireTimer = fireCooldown;
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectionRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, detectableLayer);

        if (hit.collider == null) return false;
        if (hit.collider.gameObject == gameObject) return false;
        if (hit.collider.transform.IsChildOf(transform)) return false;

        return hit.collider.CompareTag("Player");
    }

    private void SelectNextPatrolPoint()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            randomPatrolTarget = patrolPoints[currentPatrolIndex].position;
        }
        else if (randomPatrol)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            randomPatrolTarget = new Vector3(
                transform.position.x + randomCircle.x,
                transform.position.y + randomCircle.y,
                transform.position.z
            );
        }
    }

    private void ChangeState(AIState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        fireTimer = 0f;  // Сброс кулдауна при смене состояния
    }

    private void FlipSprite(float directionX)
    {
        if (spriteRenderer == null || directionX == 0) return;
        spriteRenderer.flipX = directionX < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
    }
}