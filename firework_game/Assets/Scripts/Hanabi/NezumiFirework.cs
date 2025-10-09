using UnityEngine;

public class NezumiFirework : MonoBehaviour
{
    public ParticleSystem fireParticle;
    public float spinSpeed = 720f; // 回転速度（度/秒）
    public float moveSpeed = 2f;   // 前進スピード

    private Vector3 moveDir;

    void Start()
    {
        fireParticle.Play();

        // ランダムな進行方向を決定
        float randomAngle = Random.Range(0f, 360f);
        moveDir = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
    }

    void Update()
    {
        // 自転（回転）
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

        // 前進
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // 地面との高さを固定（例：y=0.5）
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }
}
