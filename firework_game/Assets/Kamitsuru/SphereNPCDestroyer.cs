using UnityEngine;

public class SphereNPCDestroyer : MonoBehaviour
{
    [Header("検知対象のタグ")]
    public string targetTag = "NPC"; // 検知するオブジェクトのタグ

    [Header("破壊タイミング設定")]
    public float destroyDelay = 0f; // NPC破壊までの遅延時間（0なら即時）

    [Header("自己消滅設定")]
    public float selfDestructTime = 1f; // 生成から何秒後に自動削除するか

    void Start()
    {
        // SphereCollider がなければ自動で追加
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
        }

        // トリガー設定
        col.isTrigger = true;

        // Rigidbody がなければ追加（トリガー判定を受け取るには必要）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;

        // 一定時間後に球を自壊
        Destroy(gameObject, selfDestructTime);
    }

    // NPC が isTrigger = true の場合はこちらが呼ばれる
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            HandleDestroy(other.gameObject);
        }
    }

    // NPC が isTrigger = false（通常のCollider）の場合はこちらが呼ばれる
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
        Debug.Log($"SphereNPCDestroyer: NPC検出 - {npc.name} 位置: {npcPos}");

        Destroy(npc, destroyDelay);
    }
}
