using UnityEngine;
using System.Collections.Generic;

public class SphereNPCDestroyer : MonoBehaviour
{
    [Header("検知対象のタグ")]
    public string targetTag = "NPC"; // 検知するオブジェクトのタグ

    [Header("破壊タイミング設定")]
    public float destroyDelay = 0f; // NPC破壊までの遅延時間（0なら即時）

    [Header("自己消滅設定")]
    public float selfDestructTime = 1f; // 生成から何秒後に自動削除するか

    [Header("エフェクト設定")]
    public GameObject particlePrefab; // 生成するパーティクル
    public float particleScale = 1f;  // パーティクルの大きさ
    public Vector3 fixedRotation = Vector3.zero; // パーティクル生成時の固定角度

    [Header("スコア関連")]
    public FireworkLibrary fireworkLibrary; // 花火データ集
    public int fireworkIndex = 0;           // 使用する花火データのインデックス
    public ScoreData scoreData;             // 合計スコアSO

    private List<Vector3> destroyedNPCPositions = new List<Vector3>();
    private int destroyedNPCCount = 0; // 破壊したNPCの数

    void Start()
    {
        // SphereCollider がなければ自動で追加
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
        }

        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;

        // 一定時間後に自壊
        Destroy(gameObject, selfDestructTime);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            HandleDestroy(other.gameObject);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            HandleDestroy(collision.gameObject);
        }
    }

    void HandleDestroy(GameObject npc)
    {
        Vector3 npcPos = npc.transform.position;

        // 同じ位置のNPCを重複カウントしない
        if (!destroyedNPCPositions.Contains(npcPos))
        {
            destroyedNPCPositions.Add(npcPos);
            destroyedNPCCount++;

            Debug.Log($"SphereNPCDestroyer: NPC検出 - {npc.name} 位置: {npcPos}（累計破壊数: {destroyedNPCCount}）");

            Destroy(npc, destroyDelay);

            // パーティクル生成
            if (particlePrefab != null)
            {
                Quaternion rotation = Quaternion.Euler(fixedRotation);
                GameObject particle = Instantiate(particlePrefab, npcPos, rotation);
                particle.transform.localScale = Vector3.one * particleScale;
                Destroy(particle, 5f);
            }

            // スコア加算処理
            AddScoreFromFireworkLibrary();
        }
    }

    void AddScoreFromFireworkLibrary()
    {
        if (fireworkLibrary == null || scoreData == null)
            return;

        FireworkData data = fireworkLibrary.GetFireworkData(fireworkIndex);
        if (data != null)
        {
            int addScore = data.Score;
            scoreData.AddScore(addScore);

            Debug.Log($"スコア +{addScore} （花火: {data.name}） 合計スコア: {scoreData.totalScore}");
        }
        else
        {
            Debug.LogWarning($"FireworkLibrary: インデックス {fireworkIndex} のデータが存在しません。");
        }
    }
}
