using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    [Header("Skill Tree UI")]
    [SerializeField] private GameObject skillTreeCanvas;
    [SerializeField] private TextMeshProUGUI fragmentsText;
    [SerializeField] private Button closeButton;

    [Header("References")]
    [SerializeField] private GameObject playerShip;

    private int asteroidFragments = 0;
    private GameManager gameManager;
    private ScreenWrapper screenWrapper;
    private AsteroidSpawner[] asteroidSpawners;

    // Singleton pattern
    public static SkillTreeManager Instance { get; private set; }

    private void Awake()
    {

        if (skillTreeCanvas == null || playerShip == null)
        {
            Debug.LogError("Required references are missing in SkillTreeManager.");
            return;
        }


        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate SkillTreeManager detected and destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (skillTreeCanvas != null)
            skillTreeCanvas.SetActive(false);
        else
            Debug.LogError("SkillTreeCanvas is not assigned in SkillTreeManager!");
    }


    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        // Get reference to all screen wrappers
        screenWrapper = FindFirstObjectByType<ScreenWrapper>();

        // Get all asteroid spawners
        asteroidSpawners = FindObjectsOfType<AsteroidSpawner>();

        // Set up close button
        closeButton.onClick.AddListener(CloseSkillTree);

        // Update UI
        UpdateFragmentsUI();
    }

    public void ShowSkillTree()
    {

        if (!gameObject.activeInHierarchy || skillTreeCanvas == null)
        {
            Debug.LogWarning("Attempted to show skill tree when it should not be active.");
            return;
        }

        // Disable screen wrapping
        if (screenWrapper != null)
        {
            screenWrapper.enabled = false;
        }

        // Disable asteroid spawning
        foreach (var spawner in asteroidSpawners)
        {
            spawner.SetSpawningEnabled(false);
        }

        // Freeze player movement (optional, can be implemented in SpaceShip.cs)
        if (playerShip != null)
        {
            SpaceShip shipController = playerShip.GetComponent<SpaceShip>();
            if (shipController != null)
            {
                shipController.enabled = false;
            }

            Rigidbody rb = playerShip.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Show skill tree UI
        skillTreeCanvas.SetActive(true);

        // Update UI
        UpdateFragmentsUI();
    }

    public void CloseSkillTree()
    {
        // Re-enable screen wrapping
        if (screenWrapper != null)
        {
            screenWrapper.enabled = true;
        }

        // Re-enable asteroid spawning
        foreach (var spawner in asteroidSpawners)
        {
            spawner.SetSpawningEnabled(true);
        }

        // Unfreeze player movement
        if (playerShip != null)
        {
            SpaceShip shipController = playerShip.GetComponent<SpaceShip>();
            if (shipController != null)
            {
                shipController.enabled = true;
            }
        }

        // Hide skill tree UI
        skillTreeCanvas.SetActive(false);
    }

    public void AddAsteroidFragments(int amount)
    {
        asteroidFragments += amount;
        UpdateFragmentsUI();
    }

    private void UpdateFragmentsUI()
    {
        if (fragmentsText != null)
        {
            fragmentsText.text = $"Asteroid Fragments: {asteroidFragments}";
        }
    }

    // Method to be called from skill buttons
    public bool SpendFragments(int amount)
    {
        if (asteroidFragments >= amount)
        {
            asteroidFragments -= amount;
            UpdateFragmentsUI();
            return true;
        }
        return false;
    }
}