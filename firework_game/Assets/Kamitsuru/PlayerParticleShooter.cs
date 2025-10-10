using UnityEngine;

public class PlayerParticleShooter : MonoBehaviour
{
    [Header("パーティクル設定")]
    public GameObject particlePrefab;
    public Transform firePoint;
    public float particleSpeed = 10f;
    public float particleLifetime = 3f;

    [Header("Cube設定")]
    public GameObject cubePrefab;
    public float cubeSpeed = 10f;
    public float cubeLifetime = 3f;

    [Header("衝突エフェクト")]
    public GameObject spherePrefab;

    [Header("発射設定")]
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        // firePointが存在し、アクティブなときのみ処理する
        if (firePoint != null && firePoint.gameObject.activeSelf)
        {
            // 左クリックを検出
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time >= nextFireTime)
                {
                    ShootParticle();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }
    }

    void ShootParticle()
    {
        if (particlePrefab == null || cubePrefab == null) return;

        Vector3 forwardDirection = transform.forward;

        GameObject particleInstance = Instantiate(particlePrefab, firePoint.position, firePoint.rotation);

        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startSpeed = particleSpeed;
            main.startLifetime = particleLifetime;
            particleSystem.Play();
        }

        GameObject cubeInstance = Instantiate(cubePrefab, firePoint.position, firePoint.rotation);

        Rigidbody cubeRigidbody = cubeInstance.GetComponent<Rigidbody>();
        if (cubeRigidbody == null)
        {
            cubeRigidbody = cubeInstance.AddComponent<Rigidbody>();
        }

        cubeRigidbody.linearVelocity = forwardDirection * cubeSpeed;

        CubeCollisionHandler collisionHandler = cubeInstance.GetComponent<CubeCollisionHandler>();
        if (collisionHandler == null)
        {
            collisionHandler = cubeInstance.AddComponent<CubeCollisionHandler>();
        }

        if (particleSystem != null)
        {
            collisionHandler.particleSystem = particleSystem;
        }

        if (spherePrefab != null)
        {
            collisionHandler.spherePrefab = spherePrefab;
        }

        Destroy(particleInstance, particleLifetime + 1f);
        Destroy(cubeInstance, cubeLifetime);
    }
}
