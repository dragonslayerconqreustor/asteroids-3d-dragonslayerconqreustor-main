using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Destruction Settings")]
    [SerializeField] public float health = 25f;
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
            // Get the actual damage from the bullet
            BulletBehavior bullet = collision.gameObject.GetComponent<BulletBehavior>();
            if (bullet != null)
            {
                // Apply damage from the bullet
                TakeDamage(bullet.GetDamage());
            }
            else
            {
                // Fallback to default damage if BulletBehavior is not found
                TakeDamage(25);
            }

            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            DestroyAsteroid();
        }
    }

    private void DestroyAsteroid()
    {
        // Verhoog de score wanneer een asteroïde wordt vernietigd
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.score <= 6500)
        {
            gameManager.AddScore(100);
        }

        if (gameManager != null && gameManager.score >= 6500 && gameManager.score <= 25000)
        {
            gameManager.AddScore(250);
        }

        if (gameManager != null && gameManager.score >= 25000 && gameManager.score <= 75000)
        {
            gameManager.AddScore(350);
        }

        if (gameManager != null && gameManager.score >= 75000 && gameManager.score <= 175000)
        {
            gameManager.AddScore(550);
        }

        if (gameManager != null && gameManager.score >= 175000 && gameManager.score <= 37500)
        {
            gameManager.AddScore(750);
        }

        if (gameManager != null && gameManager.score >= 37500 && gameManager.score <= 137500)
        {
            gameManager.AddScore(1250);
        }

        if (gameManager != null && gameManager.score >= 137500 && gameManager.score <= 637500)
        {
            gameManager.AddScore(2050);
        }

        if (gameManager != null && gameManager.score >= 1375000 && gameManager.score <= 63750000)
        {
            gameManager.AddScore(22500);
        }
        if (gameManager != null && gameManager.score >= 6375000 && gameManager.score <= 637500000)
        {
            gameManager.AddScore(52500);
        }

        if (destructionEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity)
                .GetComponent<ParticleSystem>();

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