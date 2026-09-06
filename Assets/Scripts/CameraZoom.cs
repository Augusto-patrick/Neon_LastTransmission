using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Camera playerCamera;

    public float normalFOV = 60f;
    public float zoomFOV = 30f;

    public float zoomSpeed = 10f;

    void Update()
    {
        float targetFOV;

        if (Input.GetMouseButton(1))
        {
            targetFOV = zoomFOV;
        }
        else
        {
            targetFOV = normalFOV;
        }

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );
    }
}