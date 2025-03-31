using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float explosionRadius = 0f; // 0 means no explosion

    [Header("Hit Effect")]
    [SerializeField] public GameObject hitEffectPrefab; // Assign your animation prefab here
    [SerializeField] private AudioClip hitSound;

    public enum BulletType { Standard, Piercing, Explosive, Laser, Homing }
    [Header("Bullet Type")]
    [SerializeField] private BulletType type = BulletType.Standard;

    [Header("Piercing Settings")]
    [SerializeField] private int pierceCount = 2; // How many targets it can go through
    private int currentPierces = 0;

    [Header("Homing Settings")]
    [SerializeField] private float homingForce = 15f;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float maxRotationSpeed = 200f;
    private Transform target;
    private Rigidbody rb;

    [Header("Laser Settings")]
    [SerializeField] private float beamLength = 50f;
    [SerializeField] private float beamWidth = 0.5f;
    [SerializeField] private Color beamColor = Color.red;
    private LineRenderer lineRenderer;

    // Add a getter method for the damage
    public int GetDamage()
    {
        return damage;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void Awake()
    {
        // Get/initialize components based on bullet type
        rb = GetComponent<Rigidbody>();

        if (type == BulletType.Laser)
        {
            // Set up line renderer for laser
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = beamColor;
            lineRenderer.endColor = beamColor;
            lineRenderer.positionCount = 2;
        }
        // Apply damage increase from skills to new bullets
        damage += BulletDamageSkill.GetTotalDamageIncrease();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (type == BulletType.Homing)
        {
            FindTarget();
        }
    }

    void Update()
    {
        if (type == BulletType.Homing && target != null && rb != null)
        {
            HomingBehavior();
        }
        else if (type == BulletType.Laser)
        {
            UpdateLaser();
        }
    }

    void FindTarget()
    {
        // Find all asteroids within range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float closestDistance = detectionRadius;
        Transform closestTarget = null;

        foreach (Collider col in colliders)
        {
            if (col.gameObject.CompareTag("Asteroid") || col.gameObject.CompareTag("Boss"))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = col.transform;
                }
            }
        }

        target = closestTarget;
    }

    void HomingBehavior()
    {
        // Calculate direction to target
        Vector3 direction = (target.position - transform.position).normalized;

        // Calculate rotation to target
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            maxRotationSpeed * Time.deltaTime
        );

        // Apply rotation and movement
        rb.MoveRotation(rotation);
        rb.linearVelocity = transform.forward * rb.linearVelocity.magnitude;

        // Apply homing force
        rb.AddForce(transform.forward * homingForce, ForceMode.Acceleration);
    }

    void UpdateLaser()
    {
        if (lineRenderer != null)
        {
            // Start position is the bullet's position
            lineRenderer.SetPosition(0, transform.position);

            // End position is calculated with raycast for impact or max length
            RaycastHit hit;
            Vector3 endPosition;

            if (Physics.Raycast(transform.position, transform.forward, out hit, beamLength))
            {
                endPosition = hit.point;

                // Apply damage to what was hit
                if (hit.collider.CompareTag("Asteroid") || hit.collider.CompareTag("Boss"))
                {
                    ApplyDamage(hit.collider.gameObject);
                }
            }
            else
            {
                endPosition = transform.position + (transform.forward * beamLength);
            }

            lineRenderer.SetPosition(1, endPosition);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore laser collision as it's handled in UpdateLaser
        if (type == BulletType.Laser)
            return;

        if (collision.gameObject.CompareTag("Asteroid") || collision.gameObject.CompareTag("Boss"))
        {
            // Apply damage to the object that was hit
            ApplyDamage(collision.gameObject);

            // Handle different bullet behaviors
            if (type == BulletType.Piercing)
            {
                currentPierces++;
                if (currentPierces >= pierceCount)
                {
                    HandleBulletDestruction(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                }
                else
                {
                    // Continue through the object without being destroyed
                    PlayHitEffect(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                }
            }
            else if (type == BulletType.Explosive)
            {
                Explode();
                HandleBulletDestruction(transform.position, Quaternion.identity);
            }
            else
            {
                // Standard bullet is destroyed after hitting
                HandleBulletDestruction(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }
        }
    }

    void Explode()
    {
        // Apply damage to all objects within explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Asteroid") || collider.CompareTag("Boss"))
            {
                // Calculate damage based on distance from explosion
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                float damagePercent = 1.0f - (distance / explosionRadius);
                int explosionDamage = Mathf.RoundToInt(damage * damagePercent);

                if (explosionDamage > 0)
                {
                    ApplyDamage(collider.gameObject, explosionDamage);
                }
            }
        }

        // Create explosion effect
        GameObject explosionEffect = new GameObject("ExplosionEffect");
        explosionEffect.transform.position = transform.position;

        ParticleSystem explosion = explosionEffect.AddComponent<ParticleSystem>();
        var main = explosion.main;
        main.startSize = explosionRadius * 2;
        main.startLifetime = 0.5f;
        main.startSpeed = 1f;
        main.startColor = Color.yellow;

        Destroy(explosionEffect, 2f);
    }

    void ApplyDamage(GameObject target, int damageAmount = -1)
    {
        // Use specified amount or default to this bullet's damage
        int actualDamage = damageAmount > 0 ? damageAmount : damage;

        // Check for different damageable components
        if (target.CompareTag("Boss"))
        {
            Boss boss = target.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(actualDamage);
            }
        }
        else if (target.CompareTag("Asteroid"))
        {
            // Use the new TakeDamage method in the Asteroid class
            Asteroid asteroid = target.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(actualDamage);
            }
            else
            {
                ImmovableAsteroid immovableAsteroid = target.GetComponent<ImmovableAsteroid>();
                if (immovableAsteroid != null)
                {
                    immovableAsteroid.TakeDamage(actualDamage);
                }
            }
        }
    }

    void PlayHitEffect(Vector3 position, Quaternion rotation)
    {
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, position, rotation);
            Destroy(hitEffect, 2f); // Adjust the duration as needed
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }

    void HandleBulletDestruction(Vector3 position, Quaternion rotation)
    {
        PlayHitEffect(position, rotation);
        Destroy(gameObject);
    }
}