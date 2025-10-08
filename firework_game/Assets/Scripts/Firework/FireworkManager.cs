using UnityEngine;

public class FireworkManager : MonoBehaviour
{
    [SerializeField] private FireworkLibrary fireworkLibrary;
    private FireworkData currentFireworkData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectFirework(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectFirework(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectFirework(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectFirework(3);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchFirework();
        }
    }

    void SelectFirework(int index)
    {
        currentFireworkData = fireworkLibrary.GetFireworkData(index);
        if (currentFireworkData != null)
        {
            Debug.Log($"{currentFireworkData.FireworkName} に切り替えました");
        }
    }

    void LaunchFirework()
    {
        if (currentFireworkData == null)
        {
            Debug.LogWarning("花火が選択されていません");
            return;
        }
            
        StartCoroutine(Launch());
        Debug.Log($"{currentFireworkData.FireworkName} を打ち上げました");
    }

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
