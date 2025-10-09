using UnityEngine;
using UnityEngine.AI;

public class NavMeshCheck : MonoBehaviour
{
    void Start()
    {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"{name} は NavMesh 上にいません。");
        }
        else
        {
            Debug.Log($"{name} は NavMesh 上にいます！");
        }
    }
}
