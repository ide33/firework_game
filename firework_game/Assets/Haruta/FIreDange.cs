using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyClickMover : MonoBehaviour
{
    [SerializeField] private EnemyAIData aiData; // 同じScriptableObjectを使う
    private NavMeshAgent agent;
    private bool isMoving = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        HandleClickInput();
    }

    void HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, aiData.groundMask))
            {
                float dist = Vector3.Distance(transform.position, hit.point);
                if (dist <= aiData.clickRange)
                {
                    StartCoroutine(MoveTowardClick(hit.point));
                }
            }
        }
    }

    IEnumerator MoveTowardClick(Vector3 target)
    {
        if (isMoving) yield break;

        isMoving = true;
        if (agent.isOnNavMesh) agent.isStopped = true;

        // 回転処理
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

        // 前進処理
        float moveTime = 0f;
        while (moveTime < aiData.moveDuration)
        {
            transform.position += transform.forward * aiData.moveSpeed * Time.deltaTime;
            moveTime += Time.deltaTime;
            yield return null;
        }

        if (agent.isOnNavMesh) agent.isStopped = false;
        isMoving = false;
    }
}
