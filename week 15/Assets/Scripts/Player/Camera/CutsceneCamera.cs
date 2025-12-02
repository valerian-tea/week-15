using UnityEngine;
using Yarn.Unity;

public class CutsceneCamera : MonoBehaviour
{
    public float smoothTime = 0.05f;
    float xRotation;
    float yRotation;
    float xVelocity;
    float yVelocity;
    public Transform orientation;

    [YarnCommand("pan_camera")]
    public void PanCamera(float degreesX, float degreesY)
    {
        yRotation += degreesX;
        xRotation -= degreesY;
    }

    void LateUpdate()
    {
        // Smoothly interpolate the rotation values every frame
        float smoothedXRotation = Mathf.SmoothDamp(
            transform.eulerAngles.x,
            xRotation,
            ref xVelocity,
            smoothTime
        );
        float smoothedYRotation = Mathf.SmoothDamp(
            transform.eulerAngles.y,
            yRotation,
            ref yVelocity,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(smoothedXRotation, smoothedYRotation, 0);
        orientation.rotation = Quaternion.Euler(0, smoothedYRotation, 0);
    }
}
