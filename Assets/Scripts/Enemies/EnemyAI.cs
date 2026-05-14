using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public EnemyBase enemyBase;

    [Header("Detection")]
    public float detectionRange = 15f;
    public float attackRange = 10f;
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

    [Header("Trail System")]
    public float trailFollowDuration = 5f;
    public float trailRefreshInterval = 1f;
    public float checkpointReachDistance = 0.5f;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color checkpointColor = Color.magenta;
    public float checkpointGizmoSize = 0.3f;

    private enum AIState { Patrol, Chase }
    private AIState currentState = AIState.Patrol;

    private Transform player;
    private EnemyWeapon enemyWeapon;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerTrailManager trailManager;

    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private Vector3 randomPatrolTarget;
    private float fireTimer = 0f;
    private bool canSeePlayer = false;
    private float distanceToPlayer = 0f;

    private LayerMask allLayers;

    private List<Vector3> trailCheckpoints = new List<Vector3>();
    private int currentCheckpointIndex = -1;
    private float trailFollowTimer = 0f;
    private float trailRefreshTimer = 0f;
    private bool isFollowingTrail = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyWeapon = GetComponent<EnemyWeapon>();

        if (enemyBase == null)
            enemyBase = GetComponent<EnemyBase>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            trailManager = playerObj.GetComponent<PlayerTrailManager>();
        }

        allLayers = -1;
    }

    private void Start()
    {
        SelectNewPatrolTarget();
    }

    private void Update()
    {
        if (enemyBase == null || !enemyBase.IsAlive()) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                trailManager = playerObj.GetComponent<PlayerTrailManager>();
            }
            else return;
        }

        if (enemyWeapon != null && enemyWeapon.currentWeapon != null)
        {
            enemyWeapon.currentWeapon.WeaponUpdate();
        }

        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            isFollowingTrail = false;
            trailFollowTimer = 0f;
            trailCheckpoints.Clear();
            currentCheckpointIndex = -1;
        }
        else
        {
            if (distanceToPlayer > detectionRange && currentState == AIState.Chase)
            {
                if (!isFollowingTrail)
                {
                    isFollowingTrail = true;
                    trailFollowTimer = trailFollowDuration;
                    currentCheckpointIndex = FindNearestCheckpoint();
                }
            }

            if (isFollowingTrail)
            {
                trailFollowTimer -= Time.deltaTime;

                trailRefreshTimer += Time.deltaTime;
                if (trailRefreshTimer >= trailRefreshInterval && trailManager != null)
                {
                    trailCheckpoints = trailManager.GetCheckpoints();
                    trailRefreshTimer = 0f;

                    if (currentCheckpointIndex == -1 && trailCheckpoints.Count > 0)
                    {
                        currentCheckpointIndex = FindNearestCheckpoint();
                    }
                }

                if (trailFollowTimer <= 0f)
                {
                    isFollowingTrail = false;
                    trailCheckpoints.Clear();
                    currentCheckpointIndex = -1;
                    ChangeState(AIState.Patrol);
                }
            }
        }

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
                SelectNewPatrolTarget();
                patrolWaitTimer = patrolWaitTime;
            }
        }
        else
        {
            rb.linearVelocity = direction * patrolSpeed;
            FlipSprite(direction.x);
        }

        if (canSeePlayer || isFollowingTrail)
        {
            ChangeState(AIState.Chase);
        }
    }

    private void UpdateChase()
    {
        if (player == null)
        {
            SelectNewPatrolTarget();
            ChangeState(AIState.Patrol);
            return;
        }

        if (!canSeePlayer && !isFollowingTrail && distanceToPlayer > detectionRange)
        {
            SelectNewPatrolTarget();
            ChangeState(AIState.Patrol);
            return;
        }

        Vector2 direction = Vector2.zero;

        if (canSeePlayer)
        {
            direction = (player.position - transform.position).normalized;
        }
        else if (isFollowingTrail && trailCheckpoints.Count > 0 && currentCheckpointIndex >= 0 && currentCheckpointIndex < trailCheckpoints.Count)
        {
            Vector3 checkpoint = trailCheckpoints[currentCheckpointIndex];
            float distanceToCheckpoint = Vector2.Distance(transform.position, checkpoint);

            if (distanceToCheckpoint <= checkpointReachDistance)
            {
                currentCheckpointIndex++;

                if (currentCheckpointIndex >= trailCheckpoints.Count)
                {
                    isFollowingTrail = false;
                    trailCheckpoints.Clear();
                    currentCheckpointIndex = -1;
                }
            }
            else
            {
                direction = (checkpoint - transform.position).normalized;
            }
        }

        if (direction != Vector2.zero)
        {
            if (distanceToPlayer > minAttackDistance)
            {
                rb.linearVelocity = direction * moveSpeed;
                FlipSprite(direction.x);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (canSeePlayer && distanceToPlayer <= attackRange)
        {
            if (enemyWeapon != null && fireTimer <= 0)
            {
                enemyWeapon.TryShoot(direction);
                fireTimer = fireCooldown;
            }
        }
    }

    /// <summary>
    /// ✅ Выбрать новую точку патруля ОТ ТЕКУЩЕЙ ПОЗИЦИИ ВРАГА
    /// </summary>
    private void SelectNewPatrolTarget()
    {
        // Генерируем случайную точку в радиусе от ТЕКУЩЕЙ позиции врага
        // Так враг всегда начинает патруль с того места где он сейчас находится
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        randomPatrolTarget = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y + randomCircle.y,
            transform.position.z
        );
    }

    private int FindNearestCheckpoint()
    {
        if (trailCheckpoints.Count == 0) return -1;

        int nearestIndex = 0;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < trailCheckpoints.Count; i++)
        {
            float distance = Vector2.Distance(transform.position, trailCheckpoints[i]);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectionRange) return false;

        float rayOffset = 0.5f;
        Vector2 rayStart = (Vector2)transform.position + (direction * rayOffset);

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, direction, distance - rayOffset, allLayers);

        Debug.DrawRay(rayStart, direction * (distance - rayOffset), Color.yellow, 0.1f);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            string hitTag = hit.collider.tag;

            if (hitTag == "Enemy") continue;
            if (hitTag == "Wall") return false;
            if (hitTag == "Player") return true;
        }

        return false;
    }

    private void ChangeState(AIState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        fireTimer = 0f;
    }

    private void FlipSprite(float directionX)
    {
        if (spriteRenderer == null || directionX == 0) return;
        spriteRenderer.flipX = directionX < 0;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = checkpointColor;

        if (trailCheckpoints.Count > 0)
        {
            for (int i = 0; i < trailCheckpoints.Count - 1; i++)
            {
                Gizmos.DrawLine(trailCheckpoints[i], trailCheckpoints[i + 1]);
            }

            for (int i = 0; i < trailCheckpoints.Count; i++)
            {
                if (i == currentCheckpointIndex)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(trailCheckpoints[i], checkpointGizmoSize * 1.5f);
                    Gizmos.color = checkpointColor;
                }
                else
                {
                    Gizmos.DrawSphere(trailCheckpoints[i], checkpointGizmoSize);
                }
            }
        }

        // Линия к цели патруля (голубая)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, randomPatrolTarget);
        Gizmos.DrawWireSphere(randomPatrolTarget, 0.5f);

        // Радиусы
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
    }

    private void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }
}