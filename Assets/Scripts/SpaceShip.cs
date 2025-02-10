using UnityEngine;
using UnityEngine.InputSystem;
public class SpaceShip : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 5f;
    public float rotationSpeed = 100f;
    [SerializeField] GameObject shootingpoint;
   [SerializeField] GameObject bulletPrefab;
    [SerializeField] float shootingspeed;
    [SerializeField] float bulletspeed;
    [SerializeField] float reloadspeed;
    [SerializeField] float healthpoints;
    private void Start()
    {
        // Get the Rigidbody component once at start
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
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
        if (Input.GetKey(KeyCode.Space))
        {
            Instantiate(bulletPrefab, new Vector3(in *2.0f, 0, 0), Quaternion.identity);
        }
    }
}