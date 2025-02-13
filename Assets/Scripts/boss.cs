using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private int maxHP = 1000;
    [SerializeField] private Slider hpBar;
    [SerializeField] private GameObject immovableAsteroidPrefab;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject homingMissilePrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float homingMissileInterval = 5f;

    private int currentHP;
    private int phase = 1;
    private float nextShootTime;
    private float nextHomingMissileTime;

    private void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();

        // Spawn immovable asteroids for the first phase
        SpawnImmovableAsteroids(2);
    }

    private void Update()
    {
        if (Time.time >= nextShootTime)
        {
            ShootProjectiles();
            nextShootTime = Time.time + shootInterval;
        }

        if (phase >= 3 && Time.time >= nextHomingMissileTime)
        {
            ShootHomingMissile();
            nextHomingMissileTime = Time.time + homingMissileInterval;
        }
    }

    private void SpawnImmovableAsteroids(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPositionAroundBoss();
            GameObject asteroid = Instantiate(immovableAsteroidPrefab, spawnPosition, Quaternion.identity);
            asteroid.GetComponent<ImmovableAsteroid>().SetBoss(this);
        }
    }

    private Vector3 GetRandomPositionAroundBoss()
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(5f, 10f);
        return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
    }

    private void ShootProjectiles()
    {
        int projectileCount = phase; // 1, 2, or 3 projectiles based on phase
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));
        }
    }

    private void ShootHomingMissile()
    {
        GameObject missile = Instantiate(homingMissilePrefab, transform.position, Quaternion.identity);
        missile.GetComponent<HomingMissile>().SetTarget(FindObjectOfType<Player>().transform);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();
        }
        else if (currentHP <= maxHP * 0.33f && phase == 3)
        {
            // Already in phase 3, do nothing
        }
        else if (currentHP <= maxHP * 0.66f && phase == 2)
        {
            // Transition to phase 3
            phase = 3;
            SpawnImmovableAsteroids(4);
        }
        else if (currentHP <= maxHP * 0.33f && phase == 1)
        {
            // Transition to phase 2
            phase = 2;
            SpawnImmovableAsteroids(3);
        }
    }

    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.value = (float)currentHP / maxHP;
        }
    }

    private void Die()
    {
        Debug.Log("Boss Defeated!");
        Destroy(gameObject);


         void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {

                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Bullet"))
            {

                Destroy(gameObject);
            }
        }
    }
}
