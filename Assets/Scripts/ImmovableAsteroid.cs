using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // This ensures a Rigidbody is always added
[RequireComponent(typeof(Collider))]  // This ensures a Collider is always added
public class ImmovableAsteroid : MonoBehaviour
{
    private Boss boss;
    private Rigidbody rb;

    private int hp;

    public void SetHP(int newHP)
    {
        hp = newHP;
    }
    private void Awake()
    {
        // Get and setup Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Make sure we have the correct tag
        gameObject.tag = "Asteroid";
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
            TakeDamage(10); // Adjust damage as needed
            Destroy(collision.gameObject);
        }
    }
    private void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }


    // Optional: Visual feedback when hit
    private void OnValidate()
    {
        // This will run in the editor to ensure components exist
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Rigidbody missing! Adding one.");
            gameObject.AddComponent<Rigidbody>();
        }

        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("Collider missing! Adding a SphereCollider.");
            gameObject.AddComponent<SphereCollider>();
        }
    }
    private void OnDestroy()
    {
        if (boss != null)
        {
            boss.RemoveImmovableAsteroid(this);
        }
    }


}