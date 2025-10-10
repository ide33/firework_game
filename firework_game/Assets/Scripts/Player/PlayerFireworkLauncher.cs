using UnityEngine;

public class PlayerFireworkLauncher : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private FireworkManager fireworkManager; // FireworkManagerをInspectorで指定

    [Header("NPC探索設定")]
    [SerializeField] private string npcTag = "NPC";     // 対象となるタグ
    [SerializeField] private float searchRadius = 10f;  // 探索範囲

    void Update()
    {
        // 左クリックで花火発射
        if (Input.GetMouseButtonDown(0))
        {
            if (fireworkManager == null)
            {
                Debug.LogWarning("FireworkManagerがアタッチされていません。");
                return;
            }

            // FireworkManagerで選択中の花火を発射
            fireworkManager.LaunchFirework();

            // 周囲のNPCを探索
            GameObject nearestNPC = FindNearestNPC();

            // NPCが見つかれば、そこに花火モデルをインスタンス
            if (nearestNPC != null)
            {
                AttachFireworkToNPC(nearestNPC);
            }
            else
            {
                Debug.Log("周囲にNPCがいません。");
            }
        }
    }

    private GameObject FindNearestNPC()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        GameObject nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(npcTag))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.gameObject;
                }
            }
        }

        return nearest;
    }

    private void AttachFireworkToNPC(GameObject npc)
    {
        var fireworkData = fireworkManager.CurrentFireworkData;
        if (fireworkData == null || fireworkData.FireworkModel == null)
        {
            Debug.LogWarning("現在の花火データまたはモデルが設定されていません。");
            return;
        }

        // NPCに花火を生成してアタッチ
        GameObject instance = Instantiate(fireworkData.FireworkModel, npc.transform);
        instance.transform.localPosition = new Vector3(0, 1f, 0);

        Debug.Log($"{npc.name} に {fireworkData.FireworkName} を取り付けました！");

        // 3秒後に削除
        Destroy(instance, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
