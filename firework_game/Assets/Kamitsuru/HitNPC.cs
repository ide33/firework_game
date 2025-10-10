using UnityEngine;

public class HitNPC : MonoBehaviour
{
    // Trigger-based detection (use when Collider.isTrigger = true)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Destroy(other.gameObject);
        }
    }

    // Collision-based detection (use when Collider.isTrigger = false)
    void OnCollisionEnter(Collision collision)
    {
        GameObject otherObject = collision.gameObject;
        if (otherObject.CompareTag("NPC"))
        {
            Destroy(otherObject);
        }
    }
}
