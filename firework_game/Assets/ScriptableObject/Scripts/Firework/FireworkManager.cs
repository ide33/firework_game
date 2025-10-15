using System.Collections.Generic;
using UnityEngine;

public class FireworkManager : MonoBehaviour
{
    // FireworkLibraryのSO
    [SerializeField]
    private FireworkLibrary fireworkLibrary;

    // プレイヤーが持つ位置
    [SerializeField]
    private Transform fireworkHoldPoint;

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
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectFirework(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectFirework(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectFirework(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SelectFirework(3);
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
            GameObject prefab = data.FireworkModel;
            GameObject instance = Instantiate(prefab, fireworkHoldPoint);
            instance.name = $"{data.FireworkName}_Instance"; // デバッグ用に名前付け
            instance.SetActive(false);
            instance.SetActive(false);

            fireworkInstances[data] = instance;
        }

        Debug.Log($"全 {fireworkInstances.Count} 種類の花火を初期化しました。");
    }

    // 花火の切り替え
    private void SelectFirework(int index)
    {
        if (fireworkLibrary == null)
            return;

        FireworkData nextData = fireworkLibrary.GetFireworkData(index);
        if (nextData == null)
            return;

        // 現在の花火を非表示にする
        if (
            currentFireworkData != null
            && fireworkInstances.TryGetValue(currentFireworkData, out GameObject currentInstance)
        )
        {
            if (currentInstance != null)
            {
                currentInstance.SetActive(false);
            }
        }

        // 新しい花火を表示
        if (fireworkInstances.TryGetValue(nextData, out GameObject nextInstance))
        {
            if (nextInstance != null)
            {
                nextInstance.SetActive(true);
                currentFireworkData = nextData;
                currentFireworkInstance = nextInstance;
                currentIndex = index;
                Debug.Log($"{currentFireworkData.FireworkName} を装備しました");
            }
            else
            {
                Debug.LogWarning(
                    $"{nextData.FireworkName} のインスタンスが破壊されています。再生成します。"
                );
                RegenerateFireworkInstance(nextData);
            }
        }
    }

    private void RegenerateFireworkInstance(FireworkData data)
    {
        GameObject newInstance = Instantiate(data.FireworkModel, fireworkHoldPoint);
        newInstance.SetActive(true);
        fireworkInstances[data] = newInstance;
        Debug.Log($"{data.FireworkName} のインスタンスを再生成しました。");
    }

    // void UnequipFirework()
    // {
    //     if (currentFireworkInstance != null)
    //     {
    //         // オブジェクトを破壊せずに非表示にする
    //         currentFireworkInstance.SetActive(false);
    //         Debug.Log("花火を非表示にしました");
    //     }
    // }

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
