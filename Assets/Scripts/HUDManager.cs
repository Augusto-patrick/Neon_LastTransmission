using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public Gun gun;
    public PlayerHealth playerHealth;

    public TMP_Text ammoText;
    public TMP_Text healthText;

    void Update()
    {
        if (gun != null)
        {
            ammoText.text =
                gun.currentAmmo + " / " + gun.reserveAmmo;
        }

        if (playerHealth != null)
        {
            healthText.text =
                "HP: " + Mathf.CeilToInt(playerHealth.currentHealth);
        }
    }
}