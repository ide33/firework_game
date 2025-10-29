using UnityEngine;

public class HitNPC : MonoBehaviour
{
    [Header("参照設定")]
    public FireworkLibrary fireworkLibrary; // 花火データ集
    public ScoreData scoreData;             // 総スコアを保持するSO
    public int fireworkIndex = 0;           // 使用する花火データのインデックス

    // Trigger-based detection
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            HandleNPCDestroy(other.gameObject);
        }
    }

    // Collision-based detection
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("NPC"))
        {
            HandleNPCDestroy(collision.gameObject);
        }
    }

    void HandleNPCDestroy(GameObject npc)
    {
        Destroy(npc);

        if (fireworkLibrary != null && scoreData != null)
        {
            // 花火データを取得
            FireworkData data = fireworkLibrary.GetFireworkData(fireworkIndex);
            if (data != null)
            {
                scoreData.AddScore(data.Score); // ScoreData に加算
                Debug.Log($"NPC撃破！スコア +{data.Score} （合計: {scoreData.totalScore}）");
            }
        }
        else
        {
            Debug.LogWarning("FireworkLibrary または ScoreData が設定されていません。");
        }
    }
}
