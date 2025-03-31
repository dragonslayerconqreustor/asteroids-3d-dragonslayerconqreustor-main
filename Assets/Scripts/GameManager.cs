using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public class BossData
{
    public GameObject bossPrefab;
    public int spawnScore;
    public int fragmentReward = 50; // Default fragments reward for defeating this boss
}

public class GameManager : MonoBehaviour
{

    private int roundCounter = 1; // Initialize to round 1
    public int[] asteroidHealthPerRound = { 25, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160, 165, 170, 175, 180, 180, 190, 200, 215, 225, 235, 245, 255, 265, 275, 285, 300, 310, 320, 330, 340, 350, 360, 370, 380, 405, 435, 450, 465, 480, 500, 550, 600, 1000, 1250, 1500, 1750, 2000, 2500, 3000, 3500, 4000, 4500, 5000, 6000, 7000, 8000, 9000, 10000, 12500, 15000, 20000, 25000, 30000, 40000, 50000, 75000, 100000 };
    public Asteroid[] asteroidPrefabs;


    [Header("Boss Settings")]
    [SerializeField] private BossData[] bosses; // Array of boss prefabs and their spawn scores
    private List<BossData> remainingBosses = new List<BossData>(); // Tracks which bosses haven't spawned yet
    private bool bossSpawned = false;
    private bool bossDefeated = false;

    [Header("UI Score Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    public int score = 0;

    [Header("Player Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private TextMeshProUGUI livesText;
    public TextMeshProUGUI roundCounterText;
    private GameObject currentPlayer;
    private bool isPlayerImmune = false;

    [Header("Asteroid Settings")]
    //[SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private int startingAsteroids = 3;
    [SerializeField] private float minSpawnDistance = 5f;
    [SerializeField] private float spawnRadius = 20f;
    private List<GameObject> spawnedAsteroids = new List<GameObject>();
    private int currentRound = 1;
    public int health;
    [SerializeField] public PowerUps powerUp;

    // Skill Tree Reference
    private SkillTreeManager skillTreeManager;

    public static GameManager Instance { get; private set; }

    public int GetRound()
    {
        return roundCounter;
    }

    public void IncrementRound()
    {
        roundCounter++;
        Debug.Log("Round: " + roundCounter); // Or update a UI element here
        if (roundCounterText != null)
        {
            roundCounterText.text = "Round: " + roundCounter.ToString(); // Or any other formatting you want
        }
        else
        {
            Debug.LogError("TextMeshPro Text object not assigned in the Inspector!");
        }
    }

    public int GetAsteroidHealthForRound(int round)
    {
        // Check if the round is within the bounds of the array
        if (round >= 1 && round <= asteroidHealthPerRound.Length)
        {
            return asteroidHealthPerRound[round - 1]; // Adjust index to start from 1
        }
        else
        {
            Debug.LogWarning("Round exceeds defined asteroid health values. Returning default health.");
            return 25; // Default health value
        }
    }

    private void Awake()
    {
        Instance = this;
        InitializeBossList();
    }

    private void Start()
    {
        UpdateLivesUI();
          SpawnPlayer();
        powerUp = Object.FindFirstObjectByType<PowerUps>();
        skillTreeManager = FindFirstObjectByType<SkillTreeManager>();

        if (skillTreeManager == null)
        {
            Debug.LogWarning("SkillTreeManager not found in the scene!");
        }
    }

    private void InitializeBossList()
    {
        // Sort bosses by spawn score to ensure they spawn in the correct order
        System.Array.Sort(bosses, (a, b) => a.spawnScore.CompareTo(b.spawnScore));
        remainingBosses = new List<BossData>(bosses);
    }

    private void SpawnBoss()
    {
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            if (asteroid.GetComponent<Asteroid>() == null)
            {
                Destroy(asteroid);
            }
        }

        if (remainingBosses.Count > 0)
        {
            GameObject boss = Instantiate(remainingBosses[0].bossPrefab, Vector3.zero, Quaternion.identity);
            remainingBosses.RemoveAt(0); // Remove the spawned boss from the list

            bossSpawned = true;
            bossDefeated = false;

            // Disable all asteroid spawners
            AsteroidSpawner[] spawners = FindObjectsOfType<AsteroidSpawner>();
            foreach (AsteroidSpawner spawner in spawners)
            {
                spawner.SetSpawningEnabled(false);
            }
        }
    }

    public void OnBossDefeated()
    {
        bossDefeated = true;
        bossSpawned = false;

        // Add bonus score for defeating boss
        AddScore(50);

        // Award fragments and show skill tree
        if (skillTreeManager != null)
        {
            
            int fragmentsToAward = 15; // Default amount

            if (bosses.Length - remainingBosses.Count - 1 >= 0)
            {
                fragmentsToAward = bosses[bosses.Length - remainingBosses.Count - 1].fragmentReward;
            }

            skillTreeManager.AddAsteroidFragments(fragmentsToAward);

            // Show the skill tree
            skillTreeManager.ShowSkillTree();
        }
        else
        {
            // If skill tree manager is not available, continue with normal flow
            ResumeGameAfterBoss();
        }
    }

    // Called when the player exits the skill tree
    public void ResumeGameAfterBoss()
    {
        AsteroidSpawner[] spawners = FindObjectsOfType<AsteroidSpawner>();
        foreach (AsteroidSpawner spawner in spawners)
        {
            spawner.SetSpawningEnabled(true);
        }

        // Increment round counter by 1
        Debug.Log("Before increment: " + roundCounter);
        IncrementRound();
        Debug.Log("After increment: " + roundCounter);

        UpdateAsteroidHealth();
    }

    private void UpdateAsteroidHealth()
    {
        // Get the health for the current round
        int healthForRound = GetAsteroidHealthForRound(roundCounter);

        // Update the health in all asteroid prefabs
        foreach (Asteroid asteroidPrefab in asteroidPrefabs)
        {
            asteroidPrefab.health = healthForRound;
        }

        Debug.Log($"Updated asteroid prefab health to {healthForRound} for round {roundCounter}");
    }




    private void Update()
    {
        if (playerLives == 0)
        {
            SceneManager.LoadScene("gameover");
        }

        // Check if we should spawn the next boss
        if (!bossSpawned && remainingBosses.Count > 0 && score >= remainingBosses[0].spawnScore)
        {
            SpawnBoss();
        }
    }

    public void ReportPlayerHit()
    {
        if (isPlayerImmune) return;

        playerLives--;
        UpdateLivesUI();

        if (playerLives <= 0)
        {
            GameOver();
        }
        else
        {
            RespawnPlayer();
        }
    }

    // Method for skills to add extra lives
    public void AddExtraLives(int amount)
    {
        playerLives += amount;
        UpdateLivesUI();
    }

    private void SpawnPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        AssignPowerUpToPlayer(currentPlayer);  // Assign the PowerUps reference
    }

    private void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        isPlayerImmune = true;

        // If player has a power-up active, tell the PowerUps class to revert the ship
        PowerUps activePowerUp = FindFirstObjectByType<PowerUps>();
        if (activePowerUp != null && activePowerUp.isPoweredUp && activePowerUp.currentShip == currentPlayer)
        {
            activePowerUp.RevertShip();
        }

        // Then destroy the player
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        yield return new WaitForSeconds(1f);

        Vector3 safePosition = FindSafePosition();
        currentPlayer = Instantiate(playerPrefab, safePosition, Quaternion.identity);
        AssignPowerUpToPlayer(currentPlayer); // Assign the PowerUps reference

        // Make sure the PowerUps class has the correct reference to the original ship
        PowerUps powerUpsInstance = FindFirstObjectByType<PowerUps>();
        if (powerUpsInstance != null)
        {
            powerUpsInstance.originalShip = currentPlayer;
        }

        StartCoroutine(PlayerImmunityCoroutine());
    }


    // Helper function to assign the PowerUps reference
    private void AssignPowerUpToPlayer(GameObject player)
    {
        if (player == null) return; // Safety check

        PowerUps powerUpsInstance = FindFirstObjectByType<PowerUps>();

        if (powerUpsInstance == null)
        {
            Debug.LogError("No PowerUps instance found in the scene!");
            return;
        }

        SpaceShip shipScript = player.GetComponent<SpaceShip>();
        if (shipScript != null)
        {
            shipScript.powerUp = powerUpsInstance;  // Assign it!
        }
        else
        {
            Debug.LogError("Player prefab is missing the SpaceShip script!");
        }
    }

    private Vector3 FindSafePosition()
    {
        Vector3 safePosition = Vector3.zero;
        int attempts = 0;
        int maxAttempts = 10;

        while (attempts < maxAttempts)
        {
            safePosition = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            if (IsPositionSafe(safePosition))
            {
                break;
            }
            attempts++;
        }
        return safePosition;
    }

    private bool IsPositionSafe(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 3f);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Asteroid"))
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator PlayerImmunityCoroutine()
    {
        yield return new WaitForSeconds(2f);
        isPlayerImmune = false;
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {playerLives}";
        }
    }

    private void GameOver()
    {
        SceneManager.LoadScene("gameover");
    }

    public void AddScore(int points)
    {

        score += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}