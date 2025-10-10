using UnityEngine;

public class CubeCollisionHandler : MonoBehaviour
{
    [Header("衝突時のエフェクト")]
    public GameObject spherePrefab; // Inspectorで設定する球のプレハブ
    public ParticleSystem particleSystem; // 停止するパーティクルシステム（PlayerParticleShooterから設定される）

    [Header("球のサイズ設定")]
    [Range(0.1f, 1000f)]
    public float sphereRadius = 1f; // 生成する球の半径（Inspectorで調整可能）

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Cube: トリガー検知 - {other.name} (タグ: {other.tag})");
        if (other.CompareTag("NPC"))
        {
            HandleCollision(other.transform.position);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("NPC"))
        {
            HandleCollision(collision.contacts[0].point);
        }
    }

    void HandleCollision(Vector3 collisionPoint)
    {
        // パーティクルを即座に停止
        if (particleSystem != null)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // 球のプレハブを衝突地点に生成
        if (spherePrefab != null)
        {
            GameObject sphereInstance = Instantiate(spherePrefab, collisionPoint, Quaternion.identity);

            // 球の大きさを設定（localScaleで調整）
            sphereInstance.transform.localScale = Vector3.one * sphereRadius * 2f;
            // ※ 球の直径 = 半径 × 2

            // 球にRigidbodyを追加（必要に応じて）
            Rigidbody sphereRigidbody = sphereInstance.GetComponent<Rigidbody>();
            if (sphereRigidbody == null)
            {
                sphereRigidbody = sphereInstance.AddComponent<Rigidbody>();
            }
        }

        // このCubeを削除
        Destroy(gameObject);
    }
}
