using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private int maxAsteroids = 20;
    [SerializeField] private float spawnHeight = 0f;

    // Define exact game boundaries
    private const float MIN_X = -20f;
    private const float MAX_X = 20f;
    private const float MIN_Z = -15f;
    private const float MAX_Z = 13f;

    private float nextSpawnTime;

    private void Update()
    {
        if (Time.time >= nextSpawnTime && GameObject.FindGameObjectsWithTag("Asteroid").Length < maxAsteroids)
        {
            SpawnAsteroid();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private void SpawnAsteroid()
    {
        // Generate random position within the defined boundaries
        Vector3 spawnPosition = new Vector3(
            Random.Range(MIN_X, MAX_X),
            spawnHeight,
            Random.Range(MIN_Z, MAX_Z)
        );

        // Select random asteroid prefab
        GameObject selectedPrefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];

        // Spawn the asteroid
        Instantiate(selectedPrefab, spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
    }
}