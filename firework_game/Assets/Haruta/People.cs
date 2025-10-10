using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAIFull : MonoBehaviour
{
    public enum EnemyState { Patrol, Detect, Chase }

    [Header("参照")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("視認設定")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float eyeHeight = 1f;

    [Header("移動設定")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float lostSearchTime = 2.5f;

    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Patrol;
    private int currentPatrolIndex = 0;
    private bool isReacting = false;
    private float lostTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        // NavMesh上にいるか確認
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position); // AgentをNavMeshに瞬間移動
            }
            else
            {
                Debug.LogError($"{name}: NavMesh上に配置されていません。");
                enabled = false;
                return;
            }
        }

        StartCoroutine(StartPatrolCoroutine());
    }

    IEnumerator StartPatrolCoroutine()
    {
        // NavMeshAgentが登録されるまで最大30フレーム待機
        int maxFrames = 30;
        while (!agent.isOnNavMesh && maxFrames > 0)
        {
            maxFrames--;
            yield return null;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name}: NavMesh上に配置できませんでした。");
            yield break;
        }

        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (CanSeePlayer()) currentState = EnemyState.Detect;
                break;

            case EnemyState.Detect:
                if (!isReacting) StartCoroutine(ReactAndChase());
                break;

            case EnemyState.Chase:
                Chase();
                break;
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    IEnumerator ReactAndChase()
    {
        isReacting = true;
        agent.isStopped = true;

        // TODO: アニメやSEをここに入れる
        Debug.Log($"{name}: プレイヤー発見！");

        yield return new WaitForSeconds(1f);

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        currentState = EnemyState.Chase;
        isReacting = false;
    }

    void Chase()
    {
        if (!CanSeePlayer())
        {
            // プレイヤーを見失った場合
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= lostSearchTime)
                {
                    Debug.Log($"{name}: プレイヤーを見失った。巡回へ復帰。");
                    agent.speed = patrolSpeed;
                    currentState = EnemyState.Patrol;
                    GoToNextPatrolPoint();
                    lostTimer = 0f;
                }
            }
        }
        else
        {
            // 見えている間は追跡
            agent.SetDestination(player.position);
            lostTimer = 0f;
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = player.position - origin;

        if (toPlayer.magnitude > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
        if (angle > viewAngle * 0.5f) return false;

        if (obstacleMask != 0)
        {
            if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude, obstacleMask))
            {
                return false;
            }
        }

        if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit finalHit, toPlayer.magnitude))
        {
            return finalHit.transform == player;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * eyeHeight, transform.position + Vector3.up * eyeHeight + left * viewDistance);
        Gizmos.DrawLine(transform.position + Vector3.up * eyeHeight, transform.position + Vector3.up * eyeHeight + right * viewDistance);
    }
}
