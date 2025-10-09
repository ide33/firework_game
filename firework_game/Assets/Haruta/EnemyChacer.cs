using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_RandomPatrol : MonoBehaviour
{
    public enum EnemyState { Patrol, Detect, Chase, Search }

    [Header("参照")]
    [SerializeField] private Transform player;

    [Header("巡回範囲設定")]
    [SerializeField] private Vector3 patrolCenterOffset = Vector3.zero;
    [SerializeField] private float patrolRadius = 10f;

    [Header("視認設定")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("移動設定")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private float lostSearchTime = 2.5f;

    [Header("巡回中の挙動")]
    [SerializeField] private float lookAroundInterval = 6f;
    [SerializeField] private float lookAroundDuration = 2f;

    [Header("クリック反応設定")]
    [SerializeField] private float clickRange = 8f;
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private LayerMask groundMask;

    private EnemyState currentState = EnemyState.Patrol;
    private NavMeshAgent agent;
    private bool isReacting = false;
    private bool isLookingAround = false;
    private bool isClickMoving = false;
    private float lostTimer = 0f;
    private float patrolTimer = 0f;
    private Vector3 patrolCenter;
    private Quaternion originalRotation;
    private Coroutine searchCoroutine;

    private Vector3 lastClickPos = Vector3.zero;
    private bool hasClickPos = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.radius = 0.4f;
        agent.height = 2.0f;

        patrolCenter = transform.position + patrolCenterOffset;
    }

    void Update()
    {
        HandleClickInput();

        // 👇 どんな状態でもプレイヤー検知を行う（追記）
        if (CanSeePlayer())
        {
            // クリック移動 or 見回し中なら即キャンセルして追跡に入る
            if (isClickMoving || isLookingAround)
            {
                StopAllCoroutines();
                if (agent.isOnNavMesh) agent.isStopped = false;
                isClickMoving = false;
                isLookingAround = false;
                currentState = EnemyState.Detect;
                return;
            }
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (!isClickMoving) Patrol();
                if (CanSeePlayer()) currentState = EnemyState.Detect;
                break;

            case EnemyState.Detect:
                if (!isReacting) StartCoroutine(ReactAndChase());
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Search:
                // 検索中はコルーチン任せ
                break;
        }
    }

    // ==============================
    // クリック入力と行動
    // ==============================
    void HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                lastClickPos = hit.point;
                hasClickPos = true;
                Debug.Log($"{name}: クリック地点 {lastClickPos}");

                if (Vector3.Distance(transform.position, lastClickPos) <= clickRange)
                {
                    StartCoroutine(MoveTowardClick(lastClickPos));
                }
            }
        }
    }

    IEnumerator MoveTowardClick(Vector3 target)
    {
        if (isClickMoving) yield break;

        isClickMoving = true;
        if (agent.isOnNavMesh) agent.isStopped = true;

        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        float t = 0f;
        while (t < 1f)
        {
            // 👇 クリック移動中もプレイヤー検知（追記）
            if (CanSeePlayer())
            {
                if (agent.isOnNavMesh) agent.isStopped = false;
                isClickMoving = false;
                currentState = EnemyState.Detect;
                yield break;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }

        float moveTime = 0f;
        while (moveTime < moveDuration)
        {
            // 👇 前進中も検知（追記）
            if (CanSeePlayer())
            {
                if (agent.isOnNavMesh) agent.isStopped = false;
                isClickMoving = false;
                currentState = EnemyState.Detect;
                yield break;
            }

            transform.position += transform.forward * moveSpeed * Time.deltaTime;
            moveTime += Time.deltaTime;
            yield return null;
        }

        if (agent.isOnNavMesh) agent.isStopped = false;
        isClickMoving = false;
    }

    // ==============================
    // 通常巡回系
    // ==============================
    void Patrol()
    {
        if (!agent.isOnNavMesh || agent.pathPending) return;

        patrolTimer += Time.deltaTime;

        if (!isLookingAround && patrolTimer >= lookAroundInterval)
        {
            patrolTimer = 0f;
            StartCoroutine(LookAroundRoutine());
            return;
        }

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            GoToNextRandomPatrolPoint();
        }
    }

    void GoToNextRandomPatrolPoint()
    {
        Vector3 randomPoint = GetRandomPointOnNavMesh(patrolCenter, patrolRadius);
        agent.speed = patrolSpeed;
        agent.SetDestination(randomPoint);
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return center;
    }

    IEnumerator ReactAndChase()
    {
        isReacting = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        yield return new WaitForSeconds(1f);

        if (agent.isOnNavMesh) agent.isStopped = false;
        agent.speed = chaseSpeed;
        currentState = EnemyState.Chase;
        isReacting = false;
    }

    void Chase()
    {
        if (!agent.isOnNavMesh)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
            GoToNextRandomPatrolPoint();
            return;
        }

        if (CanSeePlayer())
        {
            agent.SetDestination(player.position);
            lostTimer = 0f;
        }
        else
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= lostSearchTime)
            {
                lostTimer = 0f;
                StartSearch();
            }
        }
    }

    void StartSearch()
    {
        if (searchCoroutine != null)
            StopCoroutine(searchCoroutine);
        searchCoroutine = StartCoroutine(SearchRoutine());
    }

    IEnumerator SearchRoutine()
    {
        currentState = EnemyState.Search;
        agent.isStopped = true;
        originalRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < searchDuration)
        {
            float angle = Mathf.Sin(elapsed * 2f) * 90f;
            transform.rotation = originalRotation * Quaternion.Euler(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        currentState = EnemyState.Patrol;
        GoToNextRandomPatrolPoint();
    }

    IEnumerator LookAroundRoutine()
    {
        isLookingAround = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        originalRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < lookAroundDuration)
        {
            // 👇 見回し中も検知（追記）
            if (CanSeePlayer())
            {
                if (agent.isOnNavMesh) agent.isStopped = false;
                isLookingAround = false;
                currentState = EnemyState.Detect;
                yield break;
            }

            float angle = Mathf.Sin(elapsed * 2f) * 60f;
            transform.rotation = originalRotation * Quaternion.Euler(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (agent.isOnNavMesh) agent.isStopped = false;
        isLookingAround = false;
        GoToNextRandomPatrolPoint();
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = player.position - origin;
        float distance = toPlayer.magnitude;
        if (distance > viewDistance) return false;

        Vector3 dir = toPlayer.normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (obstacleMask != 0 && Physics.Raycast(origin, dir, out RaycastHit hit, distance, obstacleMask))
            return false;

        if (Physics.Raycast(origin, dir, out RaycastHit finalHit, distance))
            return finalHit.transform == player;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? patrolCenter : transform.position + patrolCenterOffset;
        Gizmos.DrawWireSphere(center, patrolRadius);

        if (hasClickPos)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastClickPos, clickRange);
        }
    }
}
