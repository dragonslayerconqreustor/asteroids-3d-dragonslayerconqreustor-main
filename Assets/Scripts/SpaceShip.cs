using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class SpaceShip : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 5f;
    public float rotationSpeed = 100f;
    [SerializeField] GameObject[] shootingPoints;
    [SerializeField] GameObject bulletPrefab;


    [SerializeField] float healthpoints;
    [SerializeField] int bulletsPerShot = 1;  // How many bullets per shot
    [SerializeField] float bulletSpeed = 20f;  // Speed of bullets
    [SerializeField] float fireRate = 0.2f;    // Time between shots
    private float nextFireTime = 0f;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        // Get the Rigidbody component once at start
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
        audioSource = GetComponent<AudioSource>();

    }

    void FixedUpdate()
    {
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
                for (int i = 0; i < bulletsPerShot; i++)
                {


                    GameObject bullet = Instantiate(bulletPrefab, shootPoint.transform.position,
    shootPoint.transform.rotation * Quaternion.Euler(0, 180, 0));

                    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                    {
                        bulletRb.linearVelocity = shootPoint.transform.forward * bulletSpeed;
                    }
                }
            }
        }


    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ReportPlayerHit();
            }
        }
    }
}