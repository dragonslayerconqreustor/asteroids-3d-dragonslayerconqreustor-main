using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{

    [System.Serializable]
    public class PhaseSettings
    {
        public int maxAsteroids;
        public int immovableAsteroidHP;
        public float asteroidSpeed;
        public int projectileCount;
        public float projectileSpeed;
    }

    [Header("Phase Settings")]
    [SerializeField] private PhaseSettings[] phaseSettings;

    [Header("boss Shooting Settings")]
[SerializeField] private float bulletSpeed = 10f;
[SerializeField] private float shootDelay = 1.5f;


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
    
    public PhaseSettings GetCurrentPhaseSettings()
    {
        return phaseSettings[phase - 1];
    }
    private List<ImmovableAsteroid> immovableAsteroids = new List<ImmovableAsteroid>();


    private void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
        SpawnImmovableAsteroids(2);
        SetHPBarVisibility(false); // Hide HP bar at start
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
        if (immovableAsteroidPrefab == null) return;
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPositionAroundBoss();
            GameObject asteroid = Instantiate(immovableAsteroidPrefab, spawnPosition, Quaternion.identity);
            ImmovableAsteroid asteroidComponent = asteroid.GetComponent<ImmovableAsteroid>();
            if (asteroidComponent != null)
            {
                asteroidComponent.SetBoss(this);
                immovableAsteroids.Add(asteroidComponent);
            }
        }
    }



    private Vector3 GetRandomPositionAroundBoss()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(8f, 15f); // Increased the range
        return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
    }


    private void ShootProjectiles()
    {
        int projectileCount = phaseSettings[phase - 1].projectileCount;
        float angleStep = 360f / projectileCount;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * phaseSettings[phase - 1].projectileSpeed;
            }
        }
    }

    private IEnumerator ShootProjectilesCoroutine()
    {
        int projectileCount = phase;
        float angleStep = 360f / projectileCount;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }

            yield return new WaitForSeconds(shootDelay);
        }
    }

    private IEnumerator ShootSpiralPattern()
    {
        float angle = 0f;
        for (int i = 0; i < 50; i++)
        {
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * 8f;
            }

            angle += 20f;
            yield return new WaitForSeconds(0.05f);
        }
    }



    private void ShootHomingMissile()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            GameObject missile = Instantiate(homingMissilePrefab, transform.position, Quaternion.identity);
            HomingMissile homingMissile = missile.GetComponent<HomingMissile>();
            if (homingMissile != null)
            {
                homingMissile.SetTarget(player.transform);
                homingMissile.SetAsteroidInteraction(true);
            }
        }
    }


    public void TakeDamage(int damage)
    {
        if (immovableAsteroids.Count > 0)
        {
            // Boss is invulnerable while immovable asteroids exist
            return;
        }

        // Only apply damage if there are no immovable asteroids
        currentHP -= damage;
        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        // Check for phase transitions
        if (currentHP <= maxHP * 0.33f && phase < 3)
        {
            phase = 3;
            SpawnImmovableAsteroids(4);
        }
        else if (currentHP <= maxHP * 0.66f && phase < 2)
        {
            phase = 2;
            SpawnImmovableAsteroids(3);
        }
    }

    private void SetHPBarVisibility(bool isVisible)
    {
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(isVisible);
        }
    }


    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.value = (float)currentHP / maxHP;
        }
    }

    public void RemoveImmovableAsteroid(ImmovableAsteroid asteroid)
    {
        immovableAsteroids.Remove(asteroid);
    }


    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBossDefeated();
        }
        SetHPBarVisibility(false); // Hide HP bar when boss is defeated
        Destroy(gameObject);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(10);
        }
    }
}