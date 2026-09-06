using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Setup")]
    public Camera playerCamera;
    public ParticleSystem muzzleFlash;

    [Header("Shooting")]
    public float range = 100f;
    public float fireRate = 0.25f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 120;

    [Header("Reload")]
    public float reloadTime = 2f;

    private float nextFireTime = 0f;
    private bool isReloading = false;

    void Update()
    {
        // Reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            if (currentAmmo < magazineSize && reserveAmmo > 0)
            {
                StartCoroutine(Reload());
            }
        }

        // Shoot
        if (Input.GetMouseButtonDown(0)
            && Time.time >= nextFireTime
            && !isReloading)
        {
            if (currentAmmo > 0)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
            else
            {
                Debug.Log("Out of ammo! Press R to reload.");
            }
        }
    }

    void Shoot()
    {
        currentAmmo--;

        // Muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Raycast
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            ZombieHealth zombie = hit.collider.GetComponent<ZombieHealth>();

            if (zombie != null)
            {
                zombie.TakeDamage(25f);
                ZombieHitEffect hitEffect =
    hit.collider.GetComponentInParent<ZombieHitEffect>();

                if (hitEffect != null)
                {
                    hitEffect.Hit();
                }

                ZombieAI zombieAI =
                    hit.collider.GetComponent<ZombieAI>();

                if (zombieAI != null)
                {
                    Vector3 hitDirection =
                        hit.collider.transform.position -
                        playerCamera.transform.position;

                    hitDirection.y = 0f;

                    zombieAI.Knockback(hitDirection);
                }
            }
        }

        Debug.Log("Ammo: " + currentAmmo + "/" + reserveAmmo);
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;

        Debug.Log("Reload complete!");
        Debug.Log("Ammo: " + currentAmmo + "/" + reserveAmmo);
    }
}