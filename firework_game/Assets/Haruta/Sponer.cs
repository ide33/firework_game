using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RandomNavMeshSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject policePrefab;
    public GameObject npcPrefab;

    [Header("参照")]
    public Transform player; // ← プレイヤーをInspectorで割り当て

    [Header("設定")]
    public int policeCount = 5;
    public int npcCount = 10;
    public Vector3 spawnAreaMin;
    public Vector3 spawnAreaMax;
    public float maxNavMeshDistance = 5f;

    [Header("NPC再スポーン設定")]
    public float respawnCheckInterval = 5f; // チェック間隔（秒）
    public int npcMaxCount = 10;           // 最大数
    public int npcRespawnBatch = 3;        // 一度にスポーンする最大数

    private List<GameObject> npcs = new List<GameObject>();

    void Start()
    {
        // 初期スポーン
        SpawnAndWarp(policePrefab, policeCount, assignPlayer: true);
        SpawnAndWarp(npcPrefab, npcCount, assignPlayer: false);

        // NPC再スポーンの定期チェックを開始
        InvokeRepeating(nameof(CheckAndRespawnNPCs), respawnCheckInterval, respawnCheckInterval);
    }

    void SpawnAndWarp(GameObject prefab, int count, bool assignPlayer)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);

            // NavMesh上の最寄り地点にワープ
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, maxNavMeshDistance, NavMesh.AllAreas))
            {
                NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
                if (agent != null)
                    agent.Warp(hit.position);
                else
                    obj.transform.position = hit.position;
            }

            // ✅ Policeならプレイヤーを割り当てる
            if (assignPlayer)
            {
                var policeScript = obj.GetComponent<EnemyAI_RandomPatrol>();
                if (policeScript != null && player != null)
                {
                    policeScript.AssignPlayer(player);
                }
            }
            else
            {
                npcs.Add(obj);
            }
        }
    }

    void CheckAndRespawnNPCs()
    {
        // nullになったNPCをリストから除外
        npcs.RemoveAll(npc => npc == null);

        int missingCount = npcMaxCount - npcs.Count;
        if (missingCount > 0)
        {
            int spawnCount = Mathf.Min(npcRespawnBatch, missingCount);
            Debug.Log($"NPCを{spawnCount}体再スポーンします（現在: {npcs.Count}体）");

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    Random.Range(spawnAreaMin.z, spawnAreaMax.z)
                );

                GameObject npc = Instantiate(npcPrefab, randomPos, Quaternion.identity);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPos, out hit, maxNavMeshDistance, NavMesh.AllAreas))
                {
                    NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
                    if (agent != null)
                        agent.Warp(hit.position);
                    else
                        npc.transform.position = hit.position;
                }

                npcs.Add(npc);
            }
        }
    }

}
