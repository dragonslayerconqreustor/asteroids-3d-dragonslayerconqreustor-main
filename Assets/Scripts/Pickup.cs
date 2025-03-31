using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    public float timeAlive = 5f;
    protected GameManager game;

    private void Awake()
    {
        game = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        timeAlive -= Time.deltaTime;
        if (timeAlive <= 0)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Activate()
    {
        Destroy(gameObject); // Default behavior: destroy itself
    }
}
