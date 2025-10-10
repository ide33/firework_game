using UnityEngine;

[CreateAssetMenu(menuName = "Game/ScoreData")]
public class ScoreData : ScriptableObject
{
    public int totalScore;

    public void AddScore(int amount)
    {
        totalScore += amount;
        Debug.Log($"スコア加算: +{amount}（合計 {totalScore}）");
    }

    public void ResetScore()
    {
        totalScore = 0;
    }
}
