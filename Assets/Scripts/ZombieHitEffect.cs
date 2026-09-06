using UnityEngine;

public class ZombieHitEffect : MonoBehaviour
{
    public float flashTime = 0.1f;

    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] =
                renderers[i].material.color;
        }
    }

    public void Hit()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRed());
    }

    System.Collections.IEnumerator FlashRed()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = Color.red;
        }

        yield return new WaitForSeconds(flashTime);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color =
                originalColors[i];
        }
    }
}