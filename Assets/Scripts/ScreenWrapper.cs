using UnityEngine;

public class ScreenWrapper : MonoBehaviour
{
   
    [SerializeField] private float minX = -41f;
    [SerializeField] private float maxX = 45f;
    [SerializeField] private float minZ = -25f;
    [SerializeField] private float maxZ = 23f;

    private void LateUpdate()
    {
        Vector3 position = transform.position;

        // Wrap X-axis
        if (position.x < minX)
        {
            position.x = maxX;
        }
        else if (position.x > maxX)
        {
            position.x = minX;
        }

        // Wrap Z-axis
        if (position.z < minZ)
        {
            position.z = maxZ;
        }
        else if (position.z > maxZ)
        {
            position.z = minZ;
        }

        // Apply new position
        transform.position = position;
    }
}

