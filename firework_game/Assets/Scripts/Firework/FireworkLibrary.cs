using UnityEngine;

[CreateAssetMenu(fileName = "FireworkLibrary", menuName = "Fireworks/FireworkLibrary")]
public class FireworkLibrary : ScriptableObject
{
    [SerializeField] private FireworkData[] fireworkDatas;

    // 花火の検索
    public FireworkData GetFireworkData(int index)
    {
        if (index < 0 || index >= fireworkDatas.Length)
        {
            Debug.LogError("花火のインデックスが範囲外です。");
            return null;
        }
        return fireworkDatas[index];
    } 
}
