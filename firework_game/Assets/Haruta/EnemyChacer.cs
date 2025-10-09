using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_RandomPatrol : MonoBehaviour
{
    public enum EnemyState { Patrol, Detect, Chase, Search }

    [Header("参照")]
    [SerializeField] private Transform player;
    [SerializeField] private LineRenderer visionRenderer;

    [Header("AI設定データ (ScriptableObject)")]
    [SerializeField] private EnemyAIData aiData;

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

        patrolCenter = transform.position + aiData.patrolCenterOffset;

        if (visionRenderer == null)
        {
            GameObject visionObj = new GameObject("VisionRenderer");
            visionObj.transform.SetParent(transform);
            visionRenderer = visionObj.AddComponent<LineRenderer>();
            visionRenderer.useWorldSpace = false;
            visionRenderer.loop = true;
            visionRenderer.widthMultiplier = 0.05f;
        }
    }

    void Update()
    {
        HandleClickInput();
        UpdateVisionDisplay();

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
                break;
        }
    }

    // ==============================
    // 視野の表示処理
    // ==============================
    void UpdateVisionDisplay()
    {
        if (visionRenderer == null) return;

        int segments = 30;
        float halfAngle = aiData.viewAngle * 0.5f;
        float step = aiData.viewAngle / (segments - 1);

        Vector3 origin = transform.position + Vector3.up * aiData.eyeHeight;
        Vector3[] points = new Vector3[segments + 2];
        points[0] = origin;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -halfAngle + i * step;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = transform.rotation * rot * Vector3.forward;
            points[i + 1] = origin + dir * aiData.viewDistance;
        }

        visionRenderer.useWorldSpace = true;
        visionRenderer.positionCount = points.Length;
        visionRenderer.SetPositions(points);

        Color col = CanSeePlayer() ? Color.red : new Color(1f, 0.5f, 0f, 0.6f);
        visionRenderer.startColor = col;
        visionRenderer.endColor = col;
    }

    // ==============================
    // クリック入力と行動
    // ==============================
    void HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, aiData.groundMask))
            {
                lastClickPos = hit.point;
                hasClickPos = true;

                if (Vector3.Distance(transform.position, lastClickPos) <= aiData.clickRange)
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }

        float moveTime = 0f;
        while (moveTime < aiData.moveDuration)
        {
            transform.position += transform.forward * aiData.moveSpeed * Time.deltaTime;
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
        if (!isLookingAround && patrolTimer >= aiData.lookAroundInterval)
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
        Vector3 randomPoint = GetRandomPointOnNavMesh(patrolCenter, aiData.patrolRadius);
        agent.speed = aiData.patrolSpeed;
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
        agent.speed = aiData.chaseSpeed;
        currentState = EnemyState.Chase;
        isReacting = false;
    }

    void Chase()
    {
        if (!agent.isOnNavMesh)
        {
            currentState = EnemyState.Patrol;
            agent.speed = aiData.patrolSpeed;
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
            if (lostTimer >= aiData.lostSearchTime)
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
        while (elapsed < aiData.searchDuration)
        {
            float angle = Mathf.Sin(elapsed * 2f) * 90f;
            transform.rotation = originalRotation * Quaternion.Euler(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        agent.speed = aiData.patrolSpeed;
        currentState = EnemyState.Patrol;
        GoToNextRandomPatrolPoint();
    }

    IEnumerator LookAroundRoutine()
    {
        isLookingAround = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        originalRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < aiData.lookAroundDuration)
        {
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
        Vector3 origin = transform.position + Vector3.up * aiData.eyeHeight;
        Vector3 toPlayer = player.position - origin;
        float distance = toPlayer.magnitude;
        if (distance > aiData.viewDistance) return false;

        Vector3 dir = toPlayer.normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > aiData.viewAngle * 0.5f) return false;

        if (aiData.obstacleMask != 0 && Physics.Raycast(origin, dir, out RaycastHit hit, distance, aiData.obstacleMask))
            return false;

        if (Physics.Raycast(origin, dir, out RaycastHit finalHit, distance))
            return finalHit.transform == player;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + aiData.patrolCenterOffset;
        Gizmos.DrawWireSphere(center, aiData.patrolRadius);

        if (hasClickPos)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastClickPos, aiData.clickRange);
        }
    }
}
