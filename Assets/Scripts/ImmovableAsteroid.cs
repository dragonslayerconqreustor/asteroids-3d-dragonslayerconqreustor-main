using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ImmovableAsteroid : MonoBehaviour
{
    private Boss boss;
    private Rigidbody rb;
    private int hp;
    private bool isDestroyed = false;

    [Header("Asteroid Settings")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private GameObject destructionEffect;

    public void SetHP(int newHP)
    {
        hp = newHP;
        Debug.Log($"Asteroid HP set to {hp}");
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        gameObject.tag = "EnemyProjectile";
    }

    private void Start()
    {
        // Add some visual rotation to make the asteroid look dynamic
        if (rb != null)
        {
            rb.angularVelocity = new Vector3(
                Random.Range(-rotationSpeed, rotationSpeed),
                Random.Range(-rotationSpeed, rotationSpeed),
                Random.Range(-rotationSpeed, rotationSpeed)
            );
        }
    }

    public void SetBoss(Boss bossReference)
    {
        if (bossReference != null)
        {
            boss = bossReference;
            Debug.Log($"Boss reference set for asteroid: {gameObject.name}");
        }
        else
        {
            Debug.LogError("Attempted to set null boss reference!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Asteroid hit by bullet");
            TakeDamage(10);
            Destroy(collision.gameObject);
        }
        Debug.Log("Collided with: " + collision.gameObject.name + " on layer: " + collision.gameObject.layer);
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"Asteroid took {damage} damage. HP: {hp}");

        // Visual feedback for damage
        StartCoroutine(FlashDamage());

        if (hp <= 0 && !isDestroyed)
        {
            DestroyAsteroid();
        }
    }

    private void DestroyAsteroid()
    {
        isDestroyed = true;
        Debug.Log("Asteroid destroyed");

        // Play destruction effect if available
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }

        // Notify boss before destroying
        if (boss != null)
        {
            boss.RemoveImmovableAsteroid(this);
        }

        // Destroy the gameObject
        Destroy(gameObject);
    }

    private IEnumerator FlashDamage()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Store the original color
            Color originalColor = renderer.material.color;

            // Flash red
            renderer.material.color = Color.red;

            // Wait a short moment
            yield return new WaitForSeconds(0.1f);

            // Return to original color if not destroyed
            if (!isDestroyed)
            {
                renderer.material.color = originalColor;
            }
        }
    }

    // This is a fallback in case DestroyAsteroid isn't called properly
    private void OnDestroy()
    {
        if (!isDestroyed && boss != null)
        {
            Debug.Log("Asteroid destroyed through OnDestroy");
            boss.RemoveImmovableAsteroid(this);
        }
    }

    private void OnValidate()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }

    }
    
}