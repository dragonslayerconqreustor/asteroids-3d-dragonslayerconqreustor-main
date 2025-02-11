using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public float lifetime = 3f; // Default lifetime of 3 seconds

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}