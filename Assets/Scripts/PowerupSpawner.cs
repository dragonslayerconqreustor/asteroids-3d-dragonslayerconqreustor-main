using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PowerupEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnWeight = 1f; // Higher weight = more likely to spawn
    }

    [Header("Powerup Settings")]
    [SerializeField] private PowerupEntry[] powerupTypes;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private int maxPowerups = 20;
    [SerializeField] private float spawnHeight = 0f;

   
    private const float MIN_X = -17f;
    private const float MAX_X = 17f;
    private const float MIN_Z = -12f;
    private const float MAX_Z = 10f;

    private float nextSpawnTime;
    private float totalSpawnWeight;

    private void Start()
    {
        // Calculate total weights for probability distribution
        CalculateTotalWeight();
    }

    private void CalculateTotalWeight()
    {
        totalSpawnWeight = 0f;
        foreach (var powerup in powerupTypes)
        {
            totalSpawnWeight += powerup.spawnWeight;
        }
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime && GameObject.FindGameObjectsWithTag("Powerup").Length < maxPowerups)
        {
            SpawnPowerup();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private GameObject SelectRandomPowerup()
    {
        float random = Random.Range(0f, totalSpawnWeight);
        float currentWeight = 0f;

        foreach (var powerup in powerupTypes)
        {
            currentWeight += powerup.spawnWeight;
            if (random <= currentWeight)
            {
                return powerup.prefab;
            }
        }

        // Fallback to first powerup if something goes wrong
        return powerupTypes[0].prefab;
    }

    private void SpawnPowerup()
    {
        if (powerupTypes == null || powerupTypes.Length == 0)
        {
            Debug.LogWarning("No powerup prefabs assigned to spawner!");
            return;
        }

        // Generate random position within the defined boundaries
        Vector3 spawnPosition = new Vector3(
            Random.Range(MIN_X, MAX_X),
            spawnHeight,
            Random.Range(MIN_Z, MAX_Z)
        );

        // Select random powerup based on weights
        GameObject selectedPrefab = SelectRandomPowerup();

        // Spawn the powerup
        Instantiate(selectedPrefab, spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
    }
}