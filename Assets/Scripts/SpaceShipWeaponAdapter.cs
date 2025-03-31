
using TMPro;

using UnityEngine;

// This script should be attached to the same GameObject as SpaceShip
// It handles integration between the existing SpaceShip script and the new WeaponSystem
public class SpaceShipWeaponAdapter : MonoBehaviour
{
    [Header("Integration Settings")]
    [SerializeField] private SpaceShip spaceShipScript;
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private bool useOriginalShooting = false;

    [Header("UI")]
    [SerializeField] private GameObject weaponHUD;
    [SerializeField] private TMPro.TextMeshProUGUI weaponInfoText;

    // Cache original bullet references from SpaceShip
    private GameObject originalBulletPrefab;
    private GameObject[] originalShootingPoints;

    private void Awake()
    {
        // Get references
        if (spaceShipScript == null)
            spaceShipScript = GetComponent<SpaceShip>();

        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();

        // Cache original values immediately
        if (spaceShipScript != null)
        {
            originalBulletPrefab = spaceShipScript.bulletPrefab;
            originalShootingPoints = spaceShipScript.shootingPoints;
        }
    }

    void Start()
    {
        // Make sure we have references
        if (spaceShipScript == null)
        {
            spaceShipScript = GetComponent<SpaceShip>();
        }

        if (weaponSystem == null)
        {
            weaponSystem = GetComponent<WeaponSystem>();
        }

        // Cache original values
        if (spaceShipScript != null)
        {
            originalBulletPrefab = spaceShipScript.bulletPrefab;
            originalShootingPoints = spaceShipScript.shootingPoints;
        }

        // Transfer shooting points if needed
        if (weaponSystem != null && weaponSystem.shootingPoints.Length == 0 && originalShootingPoints != null)
        {
            // Convert GameObject[] to Transform[] before assignment
            Transform[] shootingPointTransforms = new Transform[originalShootingPoints.Length];
            for (int i = 0; i < originalShootingPoints.Length; i++)
            {
                shootingPointTransforms[i] = originalShootingPoints[i].transform;
            }
            weaponSystem.shootingPoints = shootingPointTransforms;
        }

        UpdateWeaponInfoUI();
    }

    void Update()
    {
        // Toggle between weapon systems with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            useOriginalShooting = !useOriginalShooting;
            ToggleWeaponSystems();
        }

        // Update weapon info when weapon changes
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Alpha5))
        {
            UpdateWeaponInfoUI();
        }
    }

    private void ToggleWeaponSystems()
    {
        if (spaceShipScript != null)
        {
            spaceShipScript.enabled = useOriginalShooting;
        }

        if (weaponSystem != null)
        {
            weaponSystem.enabled = !useOriginalShooting;
        }

        if (weaponHUD != null)
        {
            weaponHUD.SetActive(!useOriginalShooting);
        }

        // Show feedback to player
        string message = useOriginalShooting ? "Standard Weapons" : "Advanced Weapon System";
        ShowTemporaryMessage(message);
    }

    private void ShowTemporaryMessage(string message)
    {
        // Create a temporary text object
        GameObject messageObj = new GameObject("TempMessage");
        messageObj.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 10f));

        TMPro.TextMeshPro textMesh = messageObj.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = message;
        textMesh.fontSize = 10;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        textMesh.color = Color.yellow;

        // Destroy after delay
        Destroy(messageObj, 2f);
    }

    private void UpdateWeaponInfoUI()
    {
        if (weaponInfoText != null && weaponSystem != null && weaponSystem.availableWeapons.Length > 0)
        {
            WeaponSystem.WeaponType currentWeapon = weaponSystem.availableWeapons[weaponSystem.currentWeaponIndex];
            weaponInfoText.text = $"{currentWeapon.name}: {currentWeapon.description}";
        }
    }

    // Public methods that can be called from other scripts

    public void UnlockAllWeapons()
    {
        // Could be implemented to unlock weapons that start as locked
        ShowTemporaryMessage("All weapons unlocked!");
    }

    public void TemporaryFireRateBoost()
    {
        if (weaponSystem != null)
        {
            weaponSystem.TemporarilyEnhanceFireRate(2f, 10f);
            ShowTemporaryMessage("Fire rate boosted for 10 seconds!");
        }
    }

    public void FullEnergyRecharge()
    {
        if (weaponSystem != null)
        {
            weaponSystem.RechargeEnergy(100f);
            ShowTemporaryMessage("Energy fully recharged!");
        }
    }
}