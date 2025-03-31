using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class WeaponType
    {
        public string name;
        public GameObject bulletPrefab;
        public float fireRate;
        public float bulletSpeed;
        public int bulletsPerShot;
        public float spreadAngle;
        public int energyCost;
        public AudioClip fireSound;
        [TextArea(1, 3)]
        public string description;
    }

    [Header("Weapon Configuration")]
    [SerializeField] public WeaponType[] availableWeapons;
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] public Transform[] shootingPoints;

    [Header("UI Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI weaponNameText;
    [SerializeField] private TMPro.TextMeshProUGUI energyText;
    [SerializeField] private UnityEngine.UI.Slider energyBar;

    public int currentWeaponIndex = 0;
    private float currentEnergy;
    private float nextFireTime = 0f;

    private void Start()
    {
        if (availableWeapons.Length == 0)
        {
            Debug.LogError("No weapons configured in the weapon system!");
            return;
        }

        currentEnergy = maxEnergy;
        UpdateUI();
    }

    private void Update()
    {
        // Weapon switching with number keys
        for (int i = 0; i < availableWeapons.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }

        // Weapon switching with Q/E
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchWeapon((currentWeaponIndex - 1 + availableWeapons.Length) % availableWeapons.Length);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchWeapon((currentWeaponIndex + 1) % availableWeapons.Length);
        }

        // Fire weapon
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireCurrentWeapon();
        }

        // Energy regeneration
        RegenerateEnergy();
    }

    private void SwitchWeapon(int index)
    {
        if (index >= 0 && index < availableWeapons.Length)
        {
            currentWeaponIndex = index;
            UpdateUI();

            // Optional: Play weapon switch sound
            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
        }
    }

    private void FireCurrentWeapon()
    {
        WeaponType currentWeapon = availableWeapons[currentWeaponIndex];

        // Check if we have enough energy
        if (currentEnergy < currentWeapon.energyCost)
            return;

        // Set next fire time based on fire rate
        nextFireTime = Time.time + currentWeapon.fireRate;

        // Consume energy
        currentEnergy -= currentWeapon.energyCost;

        // Play sound effect
        if (audioSource != null && currentWeapon.fireSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.fireSound);
        }

        // Play muzzle flash effect
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Fire from all shooting points
        foreach (Transform shootPoint in shootingPoints)
        {
            FireFromPoint(shootPoint, currentWeapon);
        }

        UpdateUI();
    }

    private void FireFromPoint(Transform shootPoint, WeaponType weapon)
    {
        // For single bullet weapons
        if (weapon.bulletsPerShot <= 1)
        {
            SpawnBullet(shootPoint.position, shootPoint.rotation, weapon);
            return;
        }

        // For spread weapons
        float totalSpread = weapon.spreadAngle;
        float angleStep = totalSpread / (weapon.bulletsPerShot - 1);
        float startAngle = -totalSpread / 2f;

        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = shootPoint.rotation * Quaternion.Euler(0, angle, 0);
            SpawnBullet(shootPoint.position, rotation, weapon);
        }
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, WeaponType weapon)
    {
        GameObject bullet = Instantiate(weapon.bulletPrefab, position, rotation);

        // Set bullet speed
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = bullet.transform.forward * weapon.bulletSpeed;
        }

        // If it's a special bullet that needs configuration
        BulletBehavior bulletBehavior = bullet.GetComponent<BulletBehavior>();
        if (bulletBehavior != null)
        {
            bulletBehavior.SetDamage(10); // Set default damage, could be weapon specific
        }
    }

    private void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = availableWeapons[currentWeaponIndex].name;
        }

        if (energyText != null)
        {
            energyText.text = $"Energy: {Mathf.Floor(currentEnergy)}/{maxEnergy}";
        }

        if (energyBar != null)
        {
            energyBar.value = currentEnergy / maxEnergy;
        }
    }

    // Public methods for power-ups to use
    public void RechargeEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateUI();
    }

    public void TemporarilyEnhanceFireRate(float multiplier, float duration)
    {
        StartCoroutine(FireRateBoostCoroutine(multiplier, duration));
    }

    private System.Collections.IEnumerator FireRateBoostCoroutine(float multiplier, float duration)
    {
        // Store original fire rates
        float[] originalRates = new float[availableWeapons.Length];
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            originalRates[i] = availableWeapons[i].fireRate;
            availableWeapons[i].fireRate /= multiplier; // Lower time between shots
        }

        yield return new WaitForSeconds(duration);

        // Restore original fire rates
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            availableWeapons[i].fireRate = originalRates[i];
        }
    }
}