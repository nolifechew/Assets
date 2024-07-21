using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
    private Camera mainCamera;
    private float unitsPerPixel;
    private Vector3 previousIntendedPosition;

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        // Calculate the units per pixel based on orthographic size and screen height
        float orthographicSize = mainCamera.orthographicSize;
        float screenHeight = Screen.height;
        unitsPerPixel = (2 * orthographicSize) / screenHeight;

        // Initialize the previous intended position
        previousIntendedPosition = transform.position;
    }

    public Vector3 GetSnappedPosition(Vector3 intendedPosition)
    {
        // Calculate the offset from the previous intended position
        Vector3 offset = intendedPosition - previousIntendedPosition;

        // Project the offset onto the XZ plane
        Vector3 offsetXZ = new Vector3(offset.x, 0, offset.z);

        // Calculate the world units per pixel along the camera's forward and right directions
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        // Project the offset onto the camera's forward and right directions
        float forwardOffset = Vector3.Dot(offsetXZ, forward);
        float rightOffset = Vector3.Dot(offsetXZ, right);

        // Snap the offsets to the nearest pixel, adjusting for the 30-degree angle in the forward/backward direction
        forwardOffset = Mathf.Round(forwardOffset / (unitsPerPixel * 2)) * (unitsPerPixel * 2);
        rightOffset = Mathf.Round(rightOffset / unitsPerPixel) * unitsPerPixel;

        // Calculate the new snapped position based on the snapped offsets
        Vector3 snappedPosition = previousIntendedPosition + forward * forwardOffset + right * rightOffset;

        // Preserve the original Y position
        snappedPosition.y = previousIntendedPosition.y;

        // Update the previous intended position
        previousIntendedPosition = snappedPosition;

        return snappedPosition;
    }
}