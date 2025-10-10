using UnityEngine;

public class FireworkManager : MonoBehaviour
{
    // FireworkLibraryのSO
    [SerializeField] private FireworkLibrary fireworkLibrary;

    // プレイヤーが持つ位置
    [SerializeField] private Transform fireworkHoldPoint;

    // 現在の花火
    private FireworkData currentFireworkData;
    private GameObject currentFireworkInstance;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectFirework(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectFirework(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectFirework(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectFirework(3);
    }

    // 花火の切り替え 
    void SelectFirework(int index)
    {
        UnequipFirework();

        currentFireworkData = fireworkLibrary.GetFireworkData(index);
        if (currentFireworkData != null)
        {
            Debug.Log($"{currentFireworkData.FireworkName} に切り替えました");
        }

        // モデルをプレイヤーの持ち位置に生成
        if (currentFireworkData.FireworkModel != null && fireworkHoldPoint != null)
        {
            currentFireworkInstance = Instantiate(
                currentFireworkData.FireworkModel,
                fireworkHoldPoint.position,
                fireworkHoldPoint.rotation,
                fireworkHoldPoint
            );
        }

        Debug.Log($"{currentFireworkData.FireworkName} を装備しました");
    }
    
    // 花火を外す
    void UnequipFirework()
    {
        if (currentFireworkInstance != null)
        {
            Destroy(currentFireworkInstance);
            currentFireworkInstance = null;
            Debug.Log("花火を外しました");
        }
    }

    // 花火発射
    public void LaunchFirework()
    {
        if (currentFireworkData == null)
        {
            Debug.LogWarning("花火が選択されていません");
            return;
        }
            
        StartCoroutine(Launch());
        Debug.Log($"{currentFireworkData.FireworkName} を打ち上げました");
    }

    // 爆発までのディレイ
    private System.Collections.IEnumerator Launch()
    {
       if (currentFireworkData.FireworkSE != null)
       {
           AudioSource.PlayClipAtPoint(currentFireworkData.FireworkSE, Vector3.zero);
       }

       yield return new WaitForSeconds(currentFireworkData.ExplosionDelay);

       Debug.Log($"{currentFireworkData.FireworkName} が爆発しました");
    }
}
