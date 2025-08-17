using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float distance = 5.0f;

    [Header("Orbit Settings")]
    [SerializeField]
    private float rotationSpeed = 5.0f;

    [SerializeField]
    private float minVerticalAngle = 8.0f;

    [SerializeField]
    private float maxVerticalAngle = 90.0f;

    [Header("Zoom Settings")]
    [SerializeField]
    private float zoomSpeed = 5f;

    [SerializeField]
    private float minDistance = 1f;

    [SerializeField]
    private float maxDistance = 5.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            rotationX = 0;
            rotationY = 90;
            distance = 10.0f;
        }

        if (Input.GetMouseButton(1))
        {
            rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;

            rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }
}
