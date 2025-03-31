using UnityEngine;

public class SkillImplementations : MonoBehaviour
{
    [SerializeField] private SkillTreeManager skillTreeManager;

    // These are now PRIVATE
    private SpeedBoostSkill speedBoost;
    private BulletSpeedSkill bulletSpeed;
    private HealthIncreaseSkill healthIncrease;
    private FireRateSkill fireRate;
    private BulletDamageSkill bulletDamage; // Added BulletDamageSkill

    private void Awake()
    {
        // Initialize skill instances in Awake
        speedBoost = gameObject.AddComponent<SpeedBoostSkill>();
        bulletSpeed = gameObject.AddComponent<BulletSpeedSkill>();
        healthIncrease = gameObject.AddComponent<HealthIncreaseSkill>();
        fireRate = gameObject.AddComponent<FireRateSkill>();
        bulletDamage = gameObject.AddComponent<BulletDamageSkill>(); // Initialize BulletDamageSkill

        // Optionally hide the skill components in the inspector for cleaner UI
        speedBoost.hideFlags = HideFlags.HideInInspector;
        bulletSpeed.hideFlags = HideFlags.HideInInspector;
        healthIncrease.hideFlags = HideFlags.HideInInspector;
        fireRate.hideFlags = HideFlags.HideInInspector;
        bulletDamage.hideFlags = HideFlags.HideInInspector; // Hide BulletDamageSkill
    }

    public void UpgradeSpeedBoost() { TryUpgrade(speedBoost); }
    public void UpgradeBulletSpeed() { TryUpgrade(bulletSpeed); }
    public void UpgradeHealthIncrease() { TryUpgrade(healthIncrease); }
    public void UpgradeFireRate() { TryUpgrade(fireRate); }
    public void UpgradeBulletDamage() { TryUpgrade(bulletDamage); } // Add method for BulletDamageSkill

    private void TryUpgrade(Skill skill)
    {
        if (skillTreeManager != null)
        {
            if (skillTreeManager.SpendFragments(skill.skillCost))
            {
                skill.TryUpgradeSkill();
            }
            else
            {
                Debug.Log("Not enough fragments to upgrade " + skill.name);
            }
        }
        else
        {
            Debug.LogError("SkillTreeManager reference is missing in SkillImplementations!");
        }
    }

    public SpeedBoostSkill GetSpeedBoost() { return speedBoost; }
    public BulletSpeedSkill GetBulletSpeed() { return bulletSpeed; }
    public HealthIncreaseSkill GetHealthIncrease() { return healthIncrease; }
    public FireRateSkill GetFireRate() { return fireRate; }
    public BulletDamageSkill GetBulletDamage() { return bulletDamage; } // Add getter for BulletDamageSkill
}

// Example implementation of a speed boost skill
public class SpeedBoostSkill : Skill
{
    [SerializeField] private float speedBoostPerLevel = 1.0f;
    protected override void ApplySkillEffect()
    {
        SpaceShip playerShip = FindFirstObjectByType<SpaceShip>();
        if (playerShip != null)
        {
            playerShip.speed += speedBoostPerLevel;
            Debug.Log($"Speed increased to {playerShip.speed}");
        }
    }
}

// Example implementation of a bullet speed skill
public class BulletSpeedSkill : Skill
{
    [SerializeField] private float bulletSpeedBoostPerLevel = 5.0f;
    protected override void ApplySkillEffect()
    {
        SpaceShip playerShip = FindFirstObjectByType<SpaceShip>();
        if (playerShip != null)
        {
            playerShip.bulletSpeed += bulletSpeedBoostPerLevel;
            Debug.Log($"Bullet speed increased");
        }
    }
}

// Example implementation of a health increase skill
public class HealthIncreaseSkill : Skill
{
    [SerializeField] private int extraLivesPerLevel = 1;
    protected override void ApplySkillEffect()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddExtraLives(extraLivesPerLevel);
            Debug.Log($"Added {extraLivesPerLevel} extra lives");
        }
    }
}

// Example implementation of a fire rate skill
public class FireRateSkill : Skill
{
    [SerializeField] private float fireRateReductionPerLevel = 0.05f;
    protected override void ApplySkillEffect()
    {
        SpaceShip playerShip = FindFirstObjectByType<SpaceShip>();
        if (playerShip != null)
        {
            playerShip.fireRate -= fireRateReductionPerLevel;
            playerShip.fireRate = Mathf.Max(playerShip.fireRate, 0.05f);
            Debug.Log($"Fire rate decreased to {playerShip.fireRate}");
        }
    }
}

// Fixed implementation of bullet damage skill to work with BulletBehavior class
public class BulletDamageSkill : Skill
{
    [SerializeField] private int damageIncreasePerLevel = 5;

    // Static variable to track the total damage increase across all upgrades
    private static int totalDamageIncrease = 0;

    protected override void ApplySkillEffect()
    {
        // Increment the total damage increase
        totalDamageIncrease += damageIncreasePerLevel;

        // Update existing bullets in the scene
        BulletBehavior[] activeBullets = FindObjectsOfType<BulletBehavior>();
        foreach (BulletBehavior bullet in activeBullets)
        {
            int currentDamage = bullet.GetDamage();
            bullet.SetDamage(currentDamage + damageIncreasePerLevel);
        }

        Debug.Log($"Bullet damage increased by {damageIncreasePerLevel}. Total damage bonus: {totalDamageIncrease}");
    }

    // Method to get the total damage increase for new bullets
    public static int GetTotalDamageIncrease()
    {
        return totalDamageIncrease;
    }
}
