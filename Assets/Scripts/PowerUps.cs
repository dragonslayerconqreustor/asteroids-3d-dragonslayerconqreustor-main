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

    public GameObject originalShip;
    public GameObject currentShip;


    public bool isPoweredUp = false;
    private float powerupEndTime = 0f;
    private float cooldownEndTime = 0f;
    private AudioSource audioSource;





    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            originalShip = other.gameObject;
            ActivatePowerup();

            // Only destroy the visual part of the power-up
            Destroy(GetComponent<Renderer>());
            Destroy(GetComponent<Collider>());
        }
    }

    private void Start()
    {

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
        

        Vector3 shipPosition = originalShip.transform.position;
        Quaternion shipRotation = originalShip.transform.rotation;

        Rigidbody originalRb = originalShip.GetComponent<Rigidbody>();
        Vector3 currentVelocity = originalRb ? originalRb.linearVelocity : Vector3.zero;

        originalShip.SetActive(false);

        currentShip = Instantiate(transformedShipPrefab, shipPosition, shipRotation);

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
            ParticleSystem effect = Instantiate(transformEffect, shipPosition, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (audioSource && transformSound)
        {
            AudioSource.PlayClipAtPoint(transformSound, shipPosition);
        }
    }

    public void RevertShip()
    {
        if (currentShip != null)
        {
            Vector3 transformedPosition = currentShip.transform.position;
            Quaternion transformedRotation = currentShip.transform.rotation;

            Rigidbody transformedRb = currentShip.GetComponent<Rigidbody>();
            Vector3 currentVelocity = transformedRb ? transformedRb.linearVelocity : Vector3.zero;

            Destroy(currentShip);
            currentShip = null;

            // Only activate the original ship if it still exists
            if (originalShip != null)
            {
                originalShip.transform.position = transformedPosition;
                originalShip.transform.rotation = transformedRotation;
                originalShip.SetActive(true);

                Rigidbody originalRb = originalShip.GetComponent<Rigidbody>();
                if (originalRb && transformedRb)
                {
                    originalRb.linearVelocity = currentVelocity;
                }

                if (revertSound)
                {
                    AudioSource.PlayClipAtPoint(revertSound, originalShip.transform.position);
                }
            }
        }

        isPoweredUp = false;
        Destroy(gameObject);
    }
}