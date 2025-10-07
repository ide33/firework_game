using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Detect,
        Escape
    }

    [Header("参照")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("視認設定")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private LayerMask obstacleMask; // 障害物用レイヤー

    [Header("移動設定")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float escapeSpeed = 4f;

    private int currentPatrolIndex = 0;
    private EnemyState currentState = EnemyState.Patrol;
    private NavMeshAgent agent;
    private bool isReacting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (CanSeePlayer())
                {
                    currentState = EnemyState.Detect;
                }
                break;

            case EnemyState.Detect:
                if (!isReacting)
                {
                    StartCoroutine(ReactAndEscape());
                }
                break;

            case EnemyState.Escape:
                Escape();
                break;
        }
    }

    // ======================
    // ▼ 巡回処理
    // ======================
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // ======================
    // ▼ 発見処理
    // ======================
    IEnumerator ReactAndEscape()
    {
        isReacting = true;
        agent.isStopped = true;

        // 例：リアクションアニメ or SE再生
        Debug.Log("プレイヤーを発見！リアクション中...");

        yield return new WaitForSeconds(1.0f);

        currentState = EnemyState.Escape;
        agent.isStopped = false;
        agent.speed = escapeSpeed;

        isReacting = false;
    }

    // ======================
    // ▼ 逃走処理
    // ======================
    void Escape()
    {
        Vector3 dirToPlayer = (transform.position - player.position).normalized;
        Vector3 escapeTarget = transform.position + dirToPlayer * 5f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(escapeTarget, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        if (Vector3.Distance(transform.position, player.position) > viewDistance * 2f)
        {
            agent.speed = patrolSpeed;
            currentState = EnemyState.Patrol;
            GoToNextPatrolPoint();
        }
    }

    // ======================
    // ▼ 視認判定（視野角＋遮蔽）
    // ======================
    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > viewDistance) return false;
        if (angle > viewAngle / 2f) return false;

        // Raycastで遮蔽物チェック
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, out RaycastHit hit, viewDistance, ~0))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }
        return false;
    }

    // ======================
    // ▼ デバッグ視野表示
    // ======================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
}

