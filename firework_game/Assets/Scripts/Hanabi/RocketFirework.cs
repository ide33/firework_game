using UnityEngine;

public class RocketFirework : MonoBehaviour
{
    public ParticleSystem fuseParticle;
    public ParticleSystem explosionParticle;

    private bool isActivated = false;

    void Start()
    {
        // ゲーム開始時に導火線を止めておく
        if (fuseParticle.isPlaying)
        {
            fuseParticle.Stop();
            Debug.Log("💤 導火線パーティクルを停止しました（待機状態）");
        }

        if (explosionParticle.isPlaying)
        {
            explosionParticle.Stop();
            Debug.Log("💤 爆発パーティクルも停止しました（待機状態）");
        }
    }

    void Update()
    {
        // 毎フレームチェック
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("🖱️ 右クリックが押されました。");

            if (!isActivated)
            {
                Debug.Log("🚀 花火を起動します！");
                isActivated = true;
                StartCoroutine(FireworkSequence());
            }
            else
            {
                Debug.Log("⚠️ すでに起動中のため、再起動できません。");
            }
        }
    }

    private System.Collections.IEnumerator FireworkSequence()
    {
        // 1️⃣ 導火線点火
        Debug.Log("🔥 導火線に火をつけました（3秒間）");
        fuseParticle.Play();
        yield return new WaitForSeconds(3f);

        // 2️⃣ 上昇開始（減速つき）
        Debug.Log("🚀 上昇開始！");
        fuseParticle.Stop();
        float timer = 0f;
        float speed = 25f;   // 初速
        float gravity = 10f; // 減速の強さ

        while (timer < 2.5f)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            speed -= gravity * Time.deltaTime;
            timer += Time.deltaTime;

            if (Mathf.Abs(timer % 0.5f) < 0.02f)
            {
                Debug.Log($"⬆️ 上昇中... 時間:{timer:F2}s 速度:{speed:F2} 高さ:{transform.position.y:F2}");
            }

            yield return null;
        }

        // 3️⃣ 爆発！
        Debug.Log("💥 爆発！！");
        explosionParticle.Play();

        // 4️⃣ 削除前の待機
        yield return new WaitForSeconds(2f);
        Debug.Log("🧹 花火オブジェクトを削除します。");
        Destroy(gameObject);
    }
}
