using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Destruction Settings")]
    [SerializeField] private float health = 100f;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private AudioClip destructionSound;

    // Define game boundaries
    private const float MIN_X = -41f;
    private const float MAX_X = 45f;
    private const float MIN_Z = -25f;
    private const float MAX_Z = 23f;

    private Vector3 movementDirection;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Set up physics to ignore other asteroids
        gameObject.layer = LayerMask.NameToLayer("Asteroid");

        // Initialize random movement on XZ plane only
        float randomAngle = Random.Range(0f, 360f);
        movementDirection = new Vector3(
            Mathf.Sin(randomAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Cos(randomAngle * Mathf.Deg2Rad)
        );

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        rb.linearVelocity = movementDirection * randomSpeed;

        // Lock Y position and rotation
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;

        // Only rotate around Y axis
        rb.angularVelocity = new Vector3(0f, rotationSpeed, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            health -= 25f;
            Destroy(collision.gameObject);

            if (health <= 0)
            {
                DestroyAsteroid();
            }
        }
    }

    private void DestroyAsteroid()
    {
        if (destructionEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity)
                .GetComponent<ParticleSystem>();

            // Destroy the effect after it's finished playing
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }

        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        bool needsWrapping = false;

        // Check X boundaries
        if (currentPosition.x < MIN_X)
        {
            currentPosition.x = MAX_X;
            needsWrapping = true;
        }
        else if (currentPosition.x > MAX_X)
        {
            currentPosition.x = MIN_X;
            needsWrapping = true;
        }

        // Check Z boundaries
        if (currentPosition.z < MIN_Z)
        {
            currentPosition.z = MAX_Z;
            needsWrapping = true;
        }
        else if (currentPosition.z > MAX_Z)
        {
            currentPosition.z = MIN_Z;
            needsWrapping = true;
        }

        // Update position if wrapping occurred
        if (needsWrapping)
        {
            transform.position = currentPosition;
        }
    }
}