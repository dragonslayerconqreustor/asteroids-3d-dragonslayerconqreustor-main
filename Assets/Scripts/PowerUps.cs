using UnityEngine;

public class PowerUps : MonoBehaviour
{
    [Header("Power-up Settings")]
    [SerializeField] private GameObject transformedShipPrefab;
    [SerializeField] private float powerupDuration = 10f;
    [SerializeField] private float cooldownTime = 15f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem transformEffect;
    [SerializeField] private AudioClip transformSound;
    [SerializeField] private AudioClip revertSound;

    private GameObject originalShip;
    private GameObject currentShip;
    private bool isPoweredUp = false;
    private float powerupEndTime = 0f;
    private float cooldownEndTime = 0f;
    private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            originalShip = other.gameObject;
            ActivatePowerup();
        }
    }

    private void Start()
    {
        originalShip = FindFirstObjectByType<SpaceShip>().gameObject;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isPoweredUp)
        {
            float remainingTime = powerupEndTime - Time.time;
            if (remainingTime <= 0)
            {
                RevertShip();
            }
        }
    }

    private void ActivatePowerup()
    {
        Debug.Log("Activating power-up!");

        // Store the original ship's position and rotation
        Vector3 shipPosition = originalShip.transform.position;
        Quaternion shipRotation = originalShip.transform.rotation;

        // Get the original ship's velocity
        Rigidbody originalRb = originalShip.GetComponent<Rigidbody>();
        Vector3 currentVelocity = originalRb ? originalRb.linearVelocity : Vector3.zero;

        // Deactivate the original ship
        originalShip.SetActive(false);

        // Spawn the transformed ship at the original ship's position
        currentShip = Instantiate(transformedShipPrefab, shipPosition, shipRotation);

        // Transfer velocity to the new ship
        Rigidbody newRb = currentShip.GetComponent<Rigidbody>();
        if (newRb && originalRb)
        {
            newRb.linearVelocity = currentVelocity;
        }

        isPoweredUp = true;
        powerupEndTime = Time.time + powerupDuration;
        cooldownEndTime = Time.time + cooldownTime;

        if (transformEffect != null)
        {
            Instantiate(transformEffect, shipPosition, Quaternion.identity);
        }

        if (audioSource && transformSound)
        {
            audioSource.PlayOneShot(transformSound);
        }
    }

    private void RevertShip()
    {
        if (currentShip != null)
        {
            // Store the transformed ship's position and rotation
            Vector3 transformedPosition = currentShip.transform.position;
            Quaternion transformedRotation = currentShip.transform.rotation;

            // Get the transformed ship's velocity
            Rigidbody transformedRb = currentShip.GetComponent<Rigidbody>();
            Vector3 currentVelocity = transformedRb ? transformedRb.linearVelocity : Vector3.zero;

            // Destroy the transformed ship
            Destroy(currentShip);

            // Reactivate the original ship at the transformed ship's position
            originalShip.transform.position = transformedPosition;
            originalShip.transform.rotation = transformedRotation;
            originalShip.SetActive(true);

            // Transfer velocity back to the original ship
            Rigidbody originalRb = originalShip.GetComponent<Rigidbody>();
            if (originalRb && transformedRb)
            {
                originalRb.linearVelocity = currentVelocity;
            }
        }

        isPoweredUp = false;

        if (audioSource && revertSound)
        {
            audioSource.PlayOneShot(revertSound);
        }
    }
}
