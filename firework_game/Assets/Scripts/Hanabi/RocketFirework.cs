using UnityEngine;

public class RocketFirework : MonoBehaviour
{
    public ParticleSystem particleEffect; // 対象のパーティクル（インスペクタで設定）

    void Update()
    {
        // 左クリック（マウスボタン0）を押した瞬間
        if (Input.GetMouseButtonDown(0))
        {
            if (particleEffect != null)
            {
                particleEffect.Play(); // パーティクルを再生
            }

            // 2秒後にこのオブジェクトを削除
            Destroy(gameObject, 2f);
        }
    }
}
