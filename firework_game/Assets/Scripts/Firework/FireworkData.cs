using UnityEngine;

[CreateAssetMenu(fileName = "FireworkData", menuName = "Fireworks/FireworkData")]
public class FireworkData : ScriptableObject
{
    // 花火の名前
    [SerializeField] private string fireworkName;

    // 爆発までの時間
    [SerializeField] private float explosionDelay;

    // 範囲
    [SerializeField] private float range;

    // スコア
    [SerializeField] private int score;

    // 3Dモデル
    [SerializeField] private GameObject fireworkModel;

    // 効果音
    [SerializeField] private AudioClip fireworkSE;
    
    // 外部アクセス用のプロパティ
    public string FireworkName => fireworkName;
    public float ExplosionDelay => explosionDelay;
    public float Range => range;
    public int Score => score;
    public GameObject FireworkModel => fireworkModel;
    public AudioClip FireworkSE => fireworkSE;
}
