using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Boss : MonoBehaviour
{
    [System.Serializable]
    public class PhaseSettings
    {
        [Header("Phase Triggers")]
        [Range(0, 100)]
        public float healthPercentageThreshold = 100f;

        [Header("Immovable Asteroid Settings")]
        public int immovableAsteroidsToSpawn = 2;
        public int immovableAsteroidHP = 100;
        public float asteroidBulletInterval = 3f;
        public float asteroidBulletSpeed = 10f;
        public int asteroidBulletCount = 3;
        public float asteroidSpeed = 5f;

        [Header("Boss Attack Settings")]
        public int projectileCount = 4;
        public float projectileSpeed = 10f;
        [Tooltip("Time between regular projectile shots")]
        public float projectileInterval = 2f;

        [Header("Special Attacks")]
        public bool enableSpiralAttack = false;
        [Tooltip("How many projectiles in one spiral burst")]
        public int spiralProjectileCount = 50;
        [Tooltip("Angle between each spiral projectile")]
        public float spiralAngleStep = 20f;
        [Tooltip("Time between each projectile in spiral")]
        public float spiralProjectileDelay = 0.05f;
    }

    [Header("Homing Missile Settings")]
    [SerializeField] private GameObject homingMissilePrefab;
    [SerializeField] private float homingMissileInterval = 10f;
    [SerializeField] private int homingMissileCount = 3;
    [SerializeField] private float homingMissileSpeed = 15f;

    [Header("Boss Stats")]
    [SerializeField] private int maxHP = 1000;
    [SerializeField] private Slider hpBar;

    [Header("Phase Configuration")]
    [Tooltip("Configure each phase's settings. First phase is at 100% health")]
    [SerializeField] PhaseSettings[] phaseSettings;

    [Header("Prefabs")]
    [SerializeField] private GameObject immovableAsteroidPrefab;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject asteroidBulletPrefab;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool invincibleForTesting = false;

    private int currentHP;
    private int currentPhase = 0;
    private float nextShootTime;
    private float nextHomingMissileTime;
    private float nextSpiralTime;
    private List<ImmovableAsteroid> immovableAsteroids = new List<ImmovableAsteroid>();
    private Dictionary<ImmovableAsteroid, float> asteroidNextShootTimes = new Dictionary<ImmovableAsteroid, float>();
    private bool initialPhaseSpawned = false;

    public PhaseSettings GetCurrentPhaseSettings()
    {
        return phaseSettings[currentPhase];
    }

    private void OnValidate()
    {
        // Ensure phases are properly ordered by health percentage
        if (phaseSettings != null && phaseSettings.Length > 1)
        {
            for (int i = 1; i < phaseSettings.Length; i++)
            {
                if (phaseSettings[i].healthPercentageThreshold >= phaseSettings[i - 1].healthPercentageThreshold)
                {
                    Debug.LogWarning($"Phase {i + 1} threshold should be lower than Phase {i}");
                }
            }
        }
    }

    private void Start()
    {
        if (phaseSettings == null || phaseSettings.Length == 0)
        {
            Debug.LogError("No phase settings configured for boss!");
            return;
        }

        Debug.Log($"HP Bar assigned: {hpBar != null}");
        currentHP = maxHP;
        UpdateHPBar();
        SetHPBarVisibility(true);

        // Spawn initial phase asteroids
        if (!initialPhaseSpawned && phaseSettings.Length > 0)
        {
            Debug.Log("Spawning initial asteroids");
            SpawnImmovableAsteroids(phaseSettings[0].immovableAsteroidsToSpawn);
            initialPhaseSpawned = true;
        }
    }

    private void Update()
    {
        PhaseSettings currentPhaseSettings = GetCurrentPhaseSettings();

        // Regular projectile attacks
        if (Time.time >= nextShootTime)
        {
            ShootProjectiles();
            nextShootTime = Time.time + currentPhaseSettings.projectileInterval;
        }

        // Immovable Asteroid Shooting
        UpdateImmovableAsteroidShooting(currentPhaseSettings);

        // Spiral attacks
        if (currentPhaseSettings.enableSpiralAttack && Time.time >= nextSpiralTime)
        {
            StartCoroutine(ShootSpiralPattern());
            nextSpiralTime = Time.time + currentPhaseSettings.projectileInterval * 2f;
        }

        // Debug - print the count of immovable asteroids
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Current immovable asteroids: {immovableAsteroids.Count}");
            // Clean up null references
            immovableAsteroids.RemoveAll(a => a == null);
        }
    }

    private void UpdateImmovableAsteroidShooting(PhaseSettings settings)
    {
        // Create a temporary list to avoid modification during iteration
        List<ImmovableAsteroid> tempList = new List<ImmovableAsteroid>(immovableAsteroids);

        foreach (ImmovableAsteroid asteroid in tempList)
        {
            // Skip if asteroid is null or has been destroyed
            if (asteroid == null)
            {
                // Remove from tracking collections
                immovableAsteroids.Remove(asteroid);
                asteroidNextShootTimes.Remove(asteroid);
                continue;
            }

            // Initialize shoot time if not already set
            if (!asteroidNextShootTimes.ContainsKey(asteroid))
            {
                asteroidNextShootTimes[asteroid] = Time.time + settings.asteroidBulletInterval;
            }

            // Check if it's time for this asteroid to shoot
            if (Time.time >= asteroidNextShootTimes[asteroid])
            {
                ShootAsteroidBullets(asteroid, settings);
                asteroidNextShootTimes[asteroid] = Time.time + settings.asteroidBulletInterval;
            }
        }
    }

    private void ShootAsteroidBullets(ImmovableAsteroid asteroid, PhaseSettings settings)
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Calculate direction to player
        Vector3 directionToPlayer = (player.transform.position - asteroid.transform.position).normalized;

        // Shoot multiple bullets in a spread
        for (int i = 0; i < settings.asteroidBulletCount; i++)
        {
            // Add slight randomness to spread
            Vector3 spreadDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-15f, 15f), 0) * directionToPlayer;

            GameObject projectile = Instantiate(asteroidBulletPrefab, asteroid.transform.position, Quaternion.LookRotation(spreadDirection));

            // Set the projectile tag to something different than "Asteroid" or "Bullet"
            projectile.tag = "EnemyProjectile";
            projectile.layer = LayerMask.NameToLayer("EnemyProjectile");

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = spreadDirection * settings.asteroidBulletSpeed;
            }
        }
    }

    private void SpawnImmovableAsteroids(int count)
    {
        if (immovableAsteroidPrefab == null)
        {
            Debug.LogWarning("No immovable asteroid prefab assigned!");
            return;
        }

        Debug.Log($"Spawning {count} immovable asteroids");

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPositionAroundBoss();
            GameObject asteroidObject = Instantiate(immovableAsteroidPrefab, spawnPosition, Quaternion.identity);

            ImmovableAsteroid asteroid = asteroidObject.GetComponent<ImmovableAsteroid>();
            if (asteroid != null)
            {
                asteroid.SetHP(GetCurrentPhaseSettings().immovableAsteroidHP);
                asteroid.SetBoss(this);
                immovableAsteroids.Add(asteroid);
                Debug.Log($"Added asteroid to list. Count is now: {immovableAsteroids.Count}");
            }
            else
            {
                Debug.LogError("Failed to get ImmovableAsteroid component from prefab!");
            }
        }
    }

    private Vector3 GetRandomPositionAroundBoss()
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = UnityEngine.Random.Range(8f, 15f);
        return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
    }

    private void ShootProjectiles()
    {
        PhaseSettings settings = GetCurrentPhaseSettings();
        float angleStep = 360f / settings.projectileCount;

        for (int i = 0; i < settings.projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));

            // Set the tag to avoid collision confusion
            projectile.tag = "EnemyProjectile";

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * settings.projectileSpeed;
            }
        }
    }

    private IEnumerator ShootSpiralPattern()
    {
        PhaseSettings settings = GetCurrentPhaseSettings();
        float angle = 0f;

        for (int i = 0; i < settings.spiralProjectileCount; i++)
        {
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));

            // Set the tag to avoid collision confusion
            projectile.tag = "EnemyProjectile";

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * settings.projectileSpeed;
            }

            angle += settings.spiralAngleStep;
            yield return new WaitForSeconds(settings.spiralProjectileDelay);
        }
    }

    public void TakeDamage(int damage)
    {
        if (invincibleForTesting) return;

        // If there are immovable asteroids, they absorb damage
        if (immovableAsteroids.Count > 0)
        {
            Debug.Log("Boss protected by asteroids, ignoring damage");
            return;
        }

        currentHP -= damage;
        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        // Check for phase transitions
        float healthPercentage = (float)currentHP / maxHP * 100f;

        // Loop through phases in reverse to find the correct phase transition
        for (int i = phaseSettings.Length - 1; i >= 0; i--)
        {
            if (healthPercentage <= phaseSettings[i].healthPercentageThreshold && i > currentPhase)
            {
                TransitionToPhase(i);
                break;
            }
        }
    }

    private void TransitionToPhase(int newPhase)
    {
        if (newPhase < 0 || newPhase >= phaseSettings.Length) return;

        currentPhase = newPhase;
        Debug.Log($"Transitioning to phase: {newPhase} with health threshold: {phaseSettings[newPhase].healthPercentageThreshold}%");

        // Spawn new asteroids for this phase
        SpawnImmovableAsteroids(phaseSettings[newPhase].immovableAsteroidsToSpawn);

        // Reset attack timers
        nextShootTime = Time.time;
        nextSpiralTime = Time.time;

        // Clear and rebuild asteroid tracking
        asteroidNextShootTimes.Clear();
    }

    private void SetHPBarVisibility(bool isVisible)
    {
        if (hpBar != null)
        {
            Debug.Log($"Setting HP Bar visibility to: {isVisible}");
            hpBar.gameObject.SetActive(isVisible);
        }
        else
        {
            Debug.LogError("HP Bar reference is null!");
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
        if (immovableAsteroids.Contains(asteroid))
        {
            Debug.Log($"Removing asteroid from boss. Count before: {immovableAsteroids.Count}");
            immovableAsteroids.Remove(asteroid);
            asteroidNextShootTimes.Remove(asteroid);
            Debug.Log($"Asteroid removed. Count after: {immovableAsteroids.Count}");
        }
    }

    private void Die()
    {
        Debug.Log("Boss has been defeated!");

        if (GameManager.Instance != null)
        {
            Debug.Log("Notifying GameManager of boss defeat");
            GameManager.Instance.OnBossDefeated();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null! Cannot report boss defeat.");
        }

        SetHPBarVisibility(false);

        // Add a small delay before destroying the boss object
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check specifically for player bullets, not asteroids
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Boss hit by bullet");
            TakeDamage(10);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Boss hit by player");
            TakeDamage(10);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw spawn radius for immovable asteroids
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 8f);
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}