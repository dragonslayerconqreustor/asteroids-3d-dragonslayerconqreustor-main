using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [System.Serializable]
    public enum PickupType
    {
        EnergyRecharge,
        FireRateBoost,
        UnlockWeapon,
        TemporaryWeaponUpgrade
    }

    [Header("Pickup Settings")]
    [SerializeField] private PickupType pickupType = PickupType.EnergyRecharge;
    [SerializeField] private int weaponIndexToUnlock = 0;
    [SerializeField] private float energyAmount = 50f;
    [SerializeField] private float boostDuration = 10f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 1f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private Color glowColor = Color.blue;
    [SerializeField] private float glowIntensity = 1.5f;
    [SerializeField] private AudioClip pickupSound;

    private Vector3 initialPosition;
    private float bobTime;
    private Light glowLight;

    void Start()
    {
        initialPosition = transform.position;
        bobTime = Random.Range(0f, 2f * Mathf.PI); // Random start phase

        // Add glow light if not present
        glowLight = GetComponent<Light>();
        if (glowLight == null)
        {
            glowLight = gameObject.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 2f;
            glowLight.intensity = glowIntensity;
            glowLight.color = glowColor;
        }

        // Set color based on pickup type
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color pickupColor = GetColorForPickupType();
            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.SetColor("_EmissionColor", pickupColor * glowIntensity);
                rend.material.EnableKeyword("_EMISSION");
            }
            else
            {
                rend.material.color = pickupColor;
            }
        }
    }

    void Update()
    {
        // Rotate the pickup
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        bobTime += Time.deltaTime * bobFrequency;
        transform.position = initialPosition + new Vector3(0f, Mathf.Sin(bobTime) * bobAmplitude, 0f);
    }

    private Color GetColorForPickupType()
    {
        switch (pickupType)
        {
            case PickupType.EnergyRecharge:
                return Color.blue;
            case PickupType.FireRateBoost:
                return Color.red;
            case PickupType.UnlockWeapon:
                return Color.green;
            case PickupType.TemporaryWeaponUpgrade:
                return Color.yellow;
            default:
                return glowColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // Get adapter component from player to apply effects
            SpaceShipWeaponAdapter weaponAdapter = other.GetComponent<SpaceShipWeaponAdapter>();
            WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();

            if (weaponAdapter != null)
            {
                ApplyPickupEffect(weaponAdapter);
                PlayPickupEffects();
                Destroy(gameObject);
            }
            else if (weaponSystem != null)
            {
                // Apply directly to weapon system if adapter not found
                ApplyPickupEffectDirectly(weaponSystem);
                PlayPickupEffects();
                Destroy(gameObject);
            }
        }
    }

    private void ApplyPickupEffect(SpaceShipWeaponAdapter adapter)
    {
        switch (pickupType)
        {
            case PickupType.EnergyRecharge:
                adapter.FullEnergyRecharge();
                break;
            case PickupType.FireRateBoost:
                adapter.TemporaryFireRateBoost();
                break;
            case PickupType.UnlockWeapon:
                adapter.UnlockAllWeapons();
                break;
            case PickupType.TemporaryWeaponUpgrade:
                // This could be implemented with a new method in the adapter
                adapter.TemporaryFireRateBoost(); // Using fire rate boost as fallback
                break;
        }
    }

    private void ApplyPickupEffectDirectly(WeaponSystem weaponSystem)
    {
        switch (pickupType)
        {
            case PickupType.EnergyRecharge:
                weaponSystem.RechargeEnergy(energyAmount);
                break;
            case PickupType.FireRateBoost:
                weaponSystem.TemporarilyEnhanceFireRate(2f, boostDuration);
                break;
                // Other effects would require additional implementation in WeaponSystem
        }
    }

    private void PlayPickupEffects()
    {
        // Play particle effect
        if (pickupEffect != null)
        {
            ParticleSystem effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Play sound effect
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}