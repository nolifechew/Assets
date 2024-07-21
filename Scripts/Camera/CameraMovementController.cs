using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    public float forwardBackwardSpeed = 5f; // Speed for forward and backward movement
    public float leftRightSpeed = 5f; // Speed for left and right movement
    public float rotationSpeed = 90f; // Degrees per second
    public float accelerationTime = 0.5f; // Time to reach full speed
    public float decelerationTime = 0.3f; // Time to come to a stop
    private PixelPerfectCamera pixelPerfectCamera;
    private Vector3 intendedPosition;

    private bool isRotating = false;
    private float targetRotation;
    private float startRotation;
    private float rotationProgress = 0f;
    private float rotationDirection = 0f;

    void Start()
    {
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        intendedPosition = transform.position; // Initialize intended position
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            direction += Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            direction += Vector3.ProjectOnPlane(-transform.forward, Vector3.up);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction += Vector3.ProjectOnPlane(-transform.right, Vector3.up);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            direction += Vector3.ProjectOnPlane(transform.right, Vector3.up);
        }

        if (direction != Vector3.zero)
        {
            Vector3 movement = Vector3.zero;

            // Apply forward/backward speed
            float fbComponent = Vector3.Dot(direction, transform.forward);
            movement += transform.forward * fbComponent * forwardBackwardSpeed * Time.deltaTime;

            // Apply left/right speed
            float lrComponent = Vector3.Dot(direction, transform.right);
            movement += transform.right * lrComponent * leftRightSpeed * Time.deltaTime;

            intendedPosition += movement;
            intendedPosition.y = transform.position.y;
            Vector3 snappedPosition = pixelPerfectCamera.GetSnappedPosition(intendedPosition);
            transform.position = snappedPosition;
        }
    }

    void HandleRotation()
    {
        if (!isRotating)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                StartRotation(45f);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                StartRotation(-45f);
            }
        }
        else
        {
            rotationProgress += Time.deltaTime / (accelerationTime + decelerationTime);
            if (rotationProgress >= 1f)
            {
                CompleteRotation();
            }
            else
            {
                float t = EaseInOutQuad(rotationProgress);
                float currentRotation = Mathf.Lerp(startRotation, targetRotation, t);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentRotation, transform.eulerAngles.z);
            }
        }
    }

    void StartRotation(float angle)
    {
        isRotating = true;
        rotationDirection = angle;
        startRotation = transform.eulerAngles.y;
        targetRotation = startRotation + angle;
        rotationProgress = 0f;
    }

    void CompleteRotation()
    {
        isRotating = false;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetRotation, transform.eulerAngles.z);
    }

    float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}