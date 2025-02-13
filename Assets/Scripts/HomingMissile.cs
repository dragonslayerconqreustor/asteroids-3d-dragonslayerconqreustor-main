using UnityEngine;
public class HomingMissile : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float speed = 5f;
    private bool interactWithAsteroids = false;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetAsteroidInteraction(bool interact)
    {
        interactWithAsteroids = interact;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
        else if (interactWithAsteroids && collision.gameObject.CompareTag("Asteroid"))
        {
            // Interact with asteroid (e.g., destroy it or change its course)
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
