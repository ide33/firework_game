using UnityEngine;

public class RocketFirework : MonoBehaviour
{
    [Header("NPC探索設定")]
    [SerializeField] private string npcTag = "NPC";     // 対象となるタグ
    [SerializeField] private float searchRadius = 10f;  // 探索範囲

    [Header("発射設定")]
    [SerializeField] private GameObject rocketPrefab;   // ロケット花火のPrefab
    [SerializeField] private Transform firePoint;       // 発射位置（FireworkManagerの持ち手位置など）

    private bool isReady = true;  // クールダウンなどに使えるフラグ

    void Update()
    {
        // 左クリックで発射
        if (Input.GetMouseButtonDown(0) && isReady)
        {
            TryAttachToNearbyNPC();
        }
    }

    /// <summary>
    /// 周囲のNPCを探し、近くにいればロケットをアタッチ
    /// </summary>
    private void TryAttachToNearbyNPC()
    {
        // 周囲のColliderを取得
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);

        GameObject nearestNPC = null;
        float nearestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(npcTag))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestNPC = hit.gameObject;
                }
            }
        }

        if (nearestNPC != null)
        {
            AttachRocket(nearestNPC);
        }
        else
        {
            Debug.Log("周囲にNPCがいません。");
        }
    }

    /// <summary>
    /// NPCにロケット花火をアタッチする処理
    /// </summary>
    private void AttachRocket(GameObject npc)
    {
        if (rocketPrefab == null)
        {
            Debug.LogWarning("Rocket Prefab が設定されていません。");
            return;
        }

        // NPCの位置にロケット花火を生成
        GameObject rocketInstance = Instantiate(rocketPrefab, npc.transform);

        // NPCの頭上などに少しずらしてつけたい場合
        rocketInstance.transform.localPosition = new Vector3(0, 2f, 0);

        Debug.Log($"{npc.name} にロケット花火を取り付けました！");

        // 任意で爆発処理などを後で追加
    }

    /// <summary>
    /// NPC探索範囲をシーン上で可視化（デバッグ用）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
