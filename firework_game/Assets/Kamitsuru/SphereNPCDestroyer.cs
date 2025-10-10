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

    private List<Vector3> destroyedNPCPositions = new List<Vector3>();

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

        if (!destroyedNPCPositions.Contains(npcPos))
        {
            destroyedNPCPositions.Add(npcPos);
            Debug.Log($"SphereNPCDestroyer: NPC検出 - {npc.name} 位置: {npcPos}");

            Destroy(npc, destroyDelay);

            // パーティクルを生成
            if (particlePrefab != null)
            {
                Quaternion rotation = Quaternion.Euler(fixedRotation); // 固定角度を使用
                GameObject particle = Instantiate(particlePrefab, npcPos, rotation);

                // スケール調整
                particle.transform.localScale = Vector3.one * particleScale;

                // 任意で自動削除（5秒後など）
                Destroy(particle, 5f);
            }
        }
    }
}
