using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class SpaceShip : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 5f;
    public float rotationSpeed = 100f;
    [SerializeField] public GameObject[] shootingPoints;
    [SerializeField] public GameObject bulletPrefab;


    [SerializeField] float healthpoints;
    [SerializeField] public int bulletsPerShot = 1;  // How many bullets per shot
    [SerializeField] public float bulletSpeed = 20f;  // Speed of bullets
    [SerializeField] public float fireRate = 0.2f;    // Time between shots
    private float nextFireTime = 0f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] public PowerUps powerUp;
    private PowerUps activePowerUp;
    private bool controlsEnabled = true;

    private void Start()
    {
        // Get the Rigidbody component once at start
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        powerUp = Object.FindFirstObjectByType<PowerUps>();

        // Disable extra shooting points initially - they'll be enabled by skills
        for (int i = 1; i < shootingPoints.Length; i++)
        {
            if (shootingPoints[i] != null)
            {
                shootingPoints[i].SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        if (!controlsEnabled) return;

        // Check for W key in FixedUpdate (since we're using physics)
        if (Input.GetKey(KeyCode.W))
        {
            // Move forward
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        // Rotate right with D
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            audioSource.Play();
            nextFireTime = Time.time + fireRate;

            // Fire from each shooting point
            foreach (GameObject shootPoint in shootingPoints)
            {
                if (shootPoint != null && shootPoint.activeInHierarchy)
                {
                    for (int i = 0; i < bulletsPerShot; i++)
                    {
                        GameObject bullet = Instantiate(bulletPrefab, shootPoint.transform.position,
                            shootPoint.transform.rotation * Quaternion.Euler(0, 180, 0));

                        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                        Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent<Collider>());
                        if (bulletRb != null)
                        {
                            bulletRb.linearVelocity = shootPoint.transform.forward * bulletSpeed;
                        }
                    }
                }
            }
        }
    }

    // Method for SkillTreeManager to enable/disable controls
    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        // If disabling controls, also stop any movement
        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision detected with asteroid. PowerUp: {powerUp != null}, IsPoweredUp: {powerUp?.isPoweredUp}, CurrentShip matches: {powerUp?.currentShip == gameObject}");

        if (collision.gameObject.CompareTag("Asteroid"))
        {
            // First check if this is a powered-up ship
            PowerUps activePowerUp = FindFirstObjectByType<PowerUps>();
            if (activePowerUp != null && activePowerUp.isPoweredUp && activePowerUp.currentShip == gameObject)
            {
                activePowerUp.RevertShip();
                return;
            }

            // If no power-up is active, report the hit directly
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ReportPlayerHit();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Pickup pickup = other.GetComponent<Pickup>();
        if (pickup != null)
        {
            pickup.Activate();
        }
    }
}