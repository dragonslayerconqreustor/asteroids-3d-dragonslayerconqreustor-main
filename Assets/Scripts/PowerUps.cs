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
            
            Destroy(gameObject);
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

    private void RevertShip()
    {
        if (currentShip != null)
        {
           
            Vector3 transformedPosition = currentShip.transform.position;
            Quaternion transformedRotation = currentShip.transform.rotation;

           
            Rigidbody transformedRb = currentShip.GetComponent<Rigidbody>();
            Vector3 currentVelocity = transformedRb ? transformedRb.linearVelocity : Vector3.zero;

           
            Destroy(currentShip);

            
            originalShip.transform.position = transformedPosition;
            originalShip.transform.rotation = transformedRotation;
            originalShip.SetActive(true);

          
            Rigidbody originalRb = originalShip.GetComponent<Rigidbody>();
            if (originalRb && transformedRb)
            {
                originalRb.linearVelocity = currentVelocity;
            }
        }

        isPoweredUp = false;

        if (revertSound)
        {
           
            AudioSource.PlayClipAtPoint(revertSound, originalShip.transform.position);
        }
    }
}