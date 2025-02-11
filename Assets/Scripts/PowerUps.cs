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
        Debug.Log("triggered");
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
            else if (remainingTime <= 3f)
            {
                
            }
        }
    }

    private void ActivatePowerup()
    {
        Debug.Log("Activating power-up!");
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        Rigidbody currentRb = GetComponent<Rigidbody>();
        Vector3 currentVelocity = currentRb ? currentRb.linearVelocity : Vector3.zero;

      
        originalShip.SetActive(false);
        
      
        currentShip = Instantiate(transformedShipPrefab, currentPosition, currentRotation);

     
        Rigidbody newRb = currentShip.GetComponent<Rigidbody>();
        if (newRb && currentRb)
        {
            newRb.linearVelocity = currentVelocity;
        }

   
        isPoweredUp = true;
        powerupEndTime = Time.time + powerupDuration;
        cooldownEndTime = Time.time + cooldownTime;

        if (transformEffect != null)
        {
            Instantiate(transformEffect, transform.position, Quaternion.identity);
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
            Destroy(currentShip);
        }

       
        originalShip.SetActive(true);

        isPoweredUp = false;
    

       
        if (audioSource && revertSound)
        {
            audioSource.PlayOneShot(revertSound);
        }
    }
}
