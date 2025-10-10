using UnityEngine;
using System.Collections.Generic;

public class NezumiFirework : MonoBehaviour
{
    [Header("複数のエフェクトを登録")]
    public List<ParticleSystem> fireParticles = new List<ParticleSystem>();

    public float spinSpeed = 720f; // 回転速度（度/秒）
    public float lifetime = 5f;    // 生存時間（秒）

    private bool hasStarted = false;
    private float timer = 0f;

    void Update()
    {
        // 左クリックで起動（1回だけ）
        if (Input.GetMouseButtonDown(0) && !hasStarted)
        {
            hasStarted = true;

            // 登録された全エフェクトを再生
            foreach (var ps in fireParticles)
            {
                if (ps != null)
                    ps.Play();
            }
        }

        // 起動後の処理
        if (hasStarted)
        {
            // 回転
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

            // 高さ固定
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

            // 寿命カウント
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
