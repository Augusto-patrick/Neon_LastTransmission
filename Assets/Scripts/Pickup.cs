using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Ammo,
        Health
    }

    public PickupType pickupType;

    public int ammoAmount = 30;
    public float healthAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (pickupType == PickupType.Ammo)
        {
            Gun gun = other.GetComponentInChildren<Gun>();

            if (gun != null)
            {
                gun.reserveAmmo += ammoAmount;

                Debug.Log("Picked up ammo: +" + ammoAmount);
                Destroy(gameObject);
            }
        }

        if (pickupType == PickupType.Health)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.currentHealth =
                    Mathf.Min(
                        health.currentHealth + healthAmount,
                        health.maxHealth
                    );

                Debug.Log("Picked up health: +" + healthAmount);

                Destroy(gameObject);
            }
        }
    }
}