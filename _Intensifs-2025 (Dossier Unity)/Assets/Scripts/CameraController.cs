using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Space(20)]
    public CinemachineCamera[] cameras;
    private int currentCameraIndex = 0;

    [Space(20)]
    public float minZoom = 20f;
    public float maxZoom = 60f;
    public float zoomSpeed = 40f;

    [Space(20)]
    private float currentZoom = 30f;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Start()
    {
        SetActiveCamera(currentCameraIndex);
    }

    void Update()
    {
        HandleZoom();
        HandleCameraMovement();
        HandleCameraSwitch();
        HandleKeyboardMovement();
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scrollInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        CinemachineCamera activeCam = cameras[currentCameraIndex];
        Camera simpleActiveCamera = activeCam.GetComponent<Camera>();
        simpleActiveCamera.fieldOfView = Mathf.Lerp(simpleActiveCamera.fieldOfView, currentZoom, Time.deltaTime * 5f);
    }

    private void HandleCameraMovement()
    {
        if (Input.GetMouseButton(2))
        {
            if (!isDragging)
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            CinemachineCamera activeCam = cameras[currentCameraIndex];
            float movementSpeed = Mathf.Lerp(0.05f, 0.2f, (currentZoom - minZoom) / (maxZoom - minZoom));

            float moveX = delta.x * movementSpeed;
            float moveZ = delta.y * movementSpeed;

            Vector3 newPosition = activeCam.transform.position - new Vector3(moveX, 0, moveZ);
            activeCam.transform.position = newPosition;
        }
        else
        {
            isDragging = false;
        }
    }

    private void HandleCameraSwitch()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
            SetActiveCamera(currentCameraIndex);
        }
    }

    private void SetActiveCamera(int index)
    {
        foreach (CinemachineCamera cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }

        cameras[index].gameObject.SetActive(true);
    }

    private void HandleKeyboardMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        CinemachineCamera activeCam = cameras[currentCameraIndex];

        float movementSpeed = Mathf.Lerp(0.1f, 0.5f, (currentZoom - minZoom) / (maxZoom - minZoom));

        activeCam.transform.Translate(new Vector3(moveX * movementSpeed, 0, moveZ * movementSpeed));
    }
}
