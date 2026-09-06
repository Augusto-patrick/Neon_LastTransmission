using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    public Light flashlight;

    [Header("Battery")]
    public float maxBattery = 100f;
    public float battery = 100f;
    public float drainRate = 5f;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // Press F to toggle
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (battery > 0)
            {
                flashlight.enabled = !flashlight.enabled;
            }
        }

        // Drain battery while ON
        if (flashlight.enabled)
        {
            battery -= drainRate * Time.deltaTime;

            if (battery <= 0)
            {
                battery = 0;
                flashlight.enabled = false;

                Debug.Log("FLASHLIGHT BATTERY EMPTY");
            }
        }
    }
}