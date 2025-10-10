using UnityEngine;
using System.Collections.Generic;

public class FireworkManager : MonoBehaviour
{
    // FireworkLibraryのSO
    [SerializeField] private FireworkLibrary fireworkLibrary;

    // プレイヤーが持つ位置
    [SerializeField] private Transform fireworkHoldPoint;

    // 現在の花火
    private FireworkData currentFireworkData;

    private Dictionary<FireworkData, GameObject> fireworkInstances = new(); // 花火ごとのインスタンス管理

    private int currentIndex = -1; // 現在選択中のインデックス
    private GameObject currentFireworkInstance;

    void Start()
    {
        // 起動時に全花火を生成して非表示にしておく
        InitializeAllFireworks();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectFirework(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectFirework(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectFirework(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectFirework(3);
    }

    private void InitializeAllFireworks()
    {
        if (fireworkLibrary == null)
        {
            Debug.LogError("FireworkLibrary が設定されていません。");
            return;
        }

        foreach (var data in fireworkLibrary.GetAllFireworks())
        {
            if (data.FireworkModel == null)
            {
                Debug.LogWarning($"{data.FireworkName} のモデルが設定されていません。");
                continue;
            }

            // 子オブジェクトとして生成（最初は非表示）
            GameObject instance = Instantiate(
                data.FireworkModel,
                fireworkHoldPoint.position,
                fireworkHoldPoint.rotation,
                fireworkHoldPoint
            );
            instance.SetActive(false);

            fireworkInstances[data] = instance;
        }

        Debug.Log($"全 {fireworkInstances.Count} 種類の花火を初期化しました。");
    }

    // 花火の切り替え 
    private void SelectFirework(int index)
    {
        if (fireworkLibrary == null) return;

        FireworkData nextData = fireworkLibrary.GetFireworkData(index);
        if (nextData == null) return;

        // 現在の花火を非表示に
        if (currentFireworkData != null && fireworkInstances.ContainsKey(currentFireworkData))
        {
            fireworkInstances[currentFireworkData].SetActive(false);
        }

        // 新しい花火を表示
        if (fireworkInstances.ContainsKey(nextData))
        {
            fireworkInstances[nextData].SetActive(true);
            currentFireworkData = nextData;
            currentIndex = index;
            Debug.Log($"{currentFireworkData.FireworkName} を装備しました");
        }
        else
        {
            Debug.LogWarning($"{nextData.FireworkName} のインスタンスが見つかりません。");
        }
    }

    void UnequipFirework()
{
    if (currentFireworkInstance != null)
    {
        // オブジェクトを破壊せずに非表示にする
        currentFireworkInstance.SetActive(false);
        Debug.Log("花火を非表示にしました");
    }
}

    // 花火発射
    public void LaunchFirework()
    {
        if (currentFireworkData == null)
        {
            Debug.LogWarning("花火が選択されていません。");
            return;
        }

        StartCoroutine(Launch());
        Debug.Log($"{currentFireworkData.FireworkName} を打ち上げました！");
    }

    private System.Collections.IEnumerator Launch()
    {
        if (currentFireworkData.FireworkSE != null)
        {
            AudioSource.PlayClipAtPoint(currentFireworkData.FireworkSE, Vector3.zero);
        }

        yield return new WaitForSeconds(currentFireworkData.ExplosionDelay);

        Debug.Log($"{currentFireworkData.FireworkName} が爆発しました！");
    }

    public FireworkData CurrentFireworkData => currentFireworkData;
}
