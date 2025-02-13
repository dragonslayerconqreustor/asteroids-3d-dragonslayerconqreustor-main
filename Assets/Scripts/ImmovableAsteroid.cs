using UnityEngine;



public class ImmovableAsteroid : MonoBehaviour
{
    private Boss boss;

    public void SetBoss(Boss boss)
    {
        this.boss = boss;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            boss.TakeDamage(10); // Deal damage to the boss
            Destroy(collision.gameObject); // Destroy the bullet
        }
    }
}