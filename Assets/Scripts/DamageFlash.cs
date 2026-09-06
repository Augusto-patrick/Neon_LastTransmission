using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public GameObject damageFlash;
    public float flashDuration = 0.15f;

    public void Flash()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        damageFlash.SetActive(true);

        yield return new WaitForSeconds(flashDuration);

        damageFlash.SetActive(false);
    }
}