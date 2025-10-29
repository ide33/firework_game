using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIData", menuName = "GameData/EnemyAIData")]
public class EnemyAIData : ScriptableObject
{
    [Header("巡回範囲設定")]
    public Vector3 patrolCenterOffset = Vector3.zero;
    public float patrolRadius;
    public Transform[] patrolPoints;

    [Header("視認設定")]
    public float viewDistance;
    public float viewAngle;
    public float eyeHeight;
    public LayerMask obstacleMask;

    [Header("移動設定")]
    public float patrolSpeed;
    public float chaseSpeed;
    public float searchDuration;
    public float lostSearchTime;

    [Header("巡回中の挙動")]
    public float lookAroundInterval;
    public float lookAroundDuration;

    [Header("クリック反応設定")]
    public float clickRange;
    public float moveDuration;
    public float moveSpeed;
    public LayerMask groundMask;
}
