using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private int maxAsteroids = 20;
    [SerializeField] private float spawnHeight = 0f;

    private bool canSpawn = true;
    private float nextSpawnTime;

    private const float MIN_X = -20f;
    private const float MAX_X = 20f;
    private const float MIN_Z = -15f;
    private const float MAX_Z = 13f;

    private Boss boss;

    private void Start()
    {
        boss = FindFirstObjectByType<Boss>();
    }


    public void SetSpawningEnabled(bool enabled)
    {
        canSpawn = enabled;
        if (!enabled)
        {
            // Clear any existing asteroids when disabled
            GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
            foreach (GameObject asteroid in asteroids)
            {
                if (asteroid.GetComponent<ImmovableAsteroid>() == null)
                {
                    Destroy(asteroid);
                }
            }
        }
    }

    private void Update()
    {
        if (!canSpawn) return;

        if (Time.time >= nextSpawnTime && GameObject.FindGameObjectsWithTag("Asteroid").Length < maxAsteroids)
        {
            SpawnAsteroid();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private void SpawnAsteroid()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(MIN_X, MAX_X),
            spawnHeight,
            Random.Range(MIN_Z, MAX_Z)
        );

     

        GameObject selectedPrefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        GameObject asteroid = Instantiate(selectedPrefab, spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb != null && boss != null)
        {
            rb.linearVelocity = Random.insideUnitSphere.normalized * boss.GetCurrentPhaseSettings().asteroidSpeed;
        }
    }
}
