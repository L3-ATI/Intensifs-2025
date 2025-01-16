using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Space (20)]
    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomSpeed = 1f;
    [Space (20)]
    public float minX = 20f;
    public float maxX = 60f;
    public float minZ = 250f;
    public float maxZ = 400f;
    [Space (20)]
    public float minRotationX = 30f;
    public float maxRotationX = 60f;
    public float minRotationY = 175f;
    public float maxRotationY = 185f;
    public float rotationSpeed = 1f;
    [Space (20)]
    private float currentZoom = 10f;
    private float rotationX = 30f;
    private float rotationY = 0f;

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        HandleZoom();
        HandleCameraMovement();
        HandleCameraRotation();
        HandleKeyboardMovement();

    }
    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scrollInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, currentZoom, Time.deltaTime * 5f);
    }

    private void HandleCameraMovement()
    {
        if (Input.GetMouseButton(2)) // Clic du milieu de la souris
        {
            if (!isDragging)
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // Vitesse dynamique en fonction du zoom
            float movementSpeed = Mathf.Lerp(0.05f, 0.2f, (currentZoom - minZoom) / (maxZoom - minZoom));

            float moveX = delta.x * movementSpeed;
            float moveZ = delta.y * movementSpeed;

            float newX = Mathf.Clamp(transform.localPosition.x - moveX, minX, maxX);
            float newZ = Mathf.Clamp(transform.localPosition.z - moveZ, minZ, maxZ);

            transform.localPosition = new Vector3(newX, transform.localPosition.y, newZ);
        }
        else
        {
            isDragging = false;
        }
    }

    private void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1)) // Clic droit
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = -Input.GetAxis("Mouse Y");

            // Interpolation pour la rotation
            rotationY = Mathf.Lerp(rotationY, rotationY + deltaX * rotationSpeed, Time.deltaTime * 10f);
            rotationX = Mathf.Lerp(rotationX, rotationX + deltaY * rotationSpeed, Time.deltaTime * 10f);

            rotationY = Mathf.Clamp(rotationY, minRotationY, maxRotationY);
            rotationX = Mathf.Clamp(rotationX, minRotationX, maxRotationX);

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
    }
    
    private void HandleKeyboardMovement()
    {
        // Déplacement avec les touches WASD
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Vitesse dynamique en fonction du zoom
        float movementSpeed = Mathf.Lerp(0.1f, 0.5f, (currentZoom - minZoom) / (maxZoom - minZoom));

        transform.Translate(new Vector3(moveX * movementSpeed, 0, moveZ * movementSpeed));
    }


}
