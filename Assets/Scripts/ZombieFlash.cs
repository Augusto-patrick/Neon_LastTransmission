using UnityEngine;
using System.Collections;

public class ZombieFlash : MonoBehaviour
{
    public Renderer zombieRenderer;

    private Material material;
    private Color originalColor;

    void Start()
    {
        if (zombieRenderer != null)
        {
            material = zombieRenderer.material;
            originalColor = material.color;
        }
    }

    public void Flash()
    {
        if (material != null)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        material.color = originalColor;
    }
}