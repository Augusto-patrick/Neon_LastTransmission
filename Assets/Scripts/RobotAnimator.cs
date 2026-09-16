using System.Collections;
using UnityEngine;

public class RobotAnimator : MonoBehaviour
{
    [Header("Walk Bob")]
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 9f;

    [Header("Body Sway")]
    public float swayAmount = 4f;
    public float swaySpeed = 4f;

    [Header("Idle Bob")]
    public float idleBob = 0.012f;

    [Header("Attack Lunge")]
    public float lungeDistance = 0.12f;
    public float lungeDuration = 0.3f;

    private ZombieAI ai;
    private Vector3 restPosition;
    private float phase;
    private Coroutine lungeRoutine;

    void Awake()
    {
        ai = GetComponent<ZombieAI>();
        restPosition = transform.localPosition;
    }

    void Update()
    {
        if (ai == null)
        {
            ai = GetComponent<ZombieAI>();
        }

        bool moving = ai != null && ai.IsMoving;

        phase += Time.deltaTime * (moving ? bobSpeed : 1f);

        float bob = moving
            ? Mathf.Abs(Mathf.Sin(phase)) * bobAmplitude
            : Mathf.Sin(phase * 0.5f) * idleBob;

        Vector3 basePosition = transform.localPosition;
        basePosition.y = restPosition.y;

        transform.localPosition =
            basePosition + new Vector3(0f, bob, 0f);

        float sway = moving
            ? Mathf.Sin(phase * swaySpeed) * swayAmount
            : 0f;

        transform.localRotation =
            Quaternion.Euler(
                sway * 0.25f,
                transform.localRotation.eulerAngles.y,
                sway
            );
    }

    public void PlayLunge()
    {
        if (lungeRoutine != null)
        {
            StopCoroutine(lungeRoutine);
        }

        lungeRoutine = StartCoroutine(LungeRoutine());
    }

    IEnumerator LungeRoutine()
    {
        Vector3 basePosition = transform.localPosition;

        float t = 0f;

        while (t < lungeDuration)
        {
            float progress = t / lungeDuration;

            float offset =
                Mathf.Sin(progress * Mathf.PI) * lungeDistance;

            transform.localPosition =
                basePosition + transform.forward * offset;

            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = basePosition;
        lungeRoutine = null;
    }
}