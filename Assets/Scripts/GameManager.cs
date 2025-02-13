using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    private bool bossSpawned = false;
    private bool bossDefeated = false;

    [Header("UI Score Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int score = 0;

    [Header("Player Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private TextMeshProUGUI livesText;
    private GameObject currentPlayer;
    private bool isPlayerImmune = false;

    [Header("Asteroid Settings")]
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private int startingAsteroids = 3;
    [SerializeField] private float minSpawnDistance = 5f;
    [SerializeField] private float spawnRadius = 20f;

    private List<GameObject> spawnedAsteroids = new List<GameObject>();
    private int currentRound = 1;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void SpawnBoss()
    {
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            if (asteroid.GetComponent<Boss>() == null)
            {
                Destroy(asteroid);
            }
        }

        GameObject boss = Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
        bossSpawned = true;
        bossDefeated = false;

        // Disable all asteroid spawners
        AsteroidSpawner[] spawners = FindObjectsOfType<AsteroidSpawner>();
        foreach (AsteroidSpawner spawner in spawners)
        {
            spawner.SetSpawningEnabled(false);
        }
    }
    public void OnBossDefeated()
    {
        bossDefeated = true;
        bossSpawned = false;

        AsteroidSpawner[] spawners = FindObjectsOfType<AsteroidSpawner>();
        foreach (AsteroidSpawner spawner in spawners)
        {
            spawner.SetSpawningEnabled(true);
        }
    }

    private void Start()
    {
        UpdateLivesUI();
        SpawnPlayer();
    }

    private void Update()
    {
        if (playerLives == 0)
        {
            SceneManager.LoadScene("gameover");
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

    private void SpawnPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    }

    private void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        isPlayerImmune = true;

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        yield return new WaitForSeconds(1f);

        Vector3 safePosition = FindSafePosition();
        currentPlayer = Instantiate(playerPrefab, safePosition, Quaternion.identity);

        StartCoroutine(PlayerImmunityCoroutine());
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

        if (score >= 10000 && !bossSpawned)
        {
            SpawnBoss();
            bossSpawned = true;
        }
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