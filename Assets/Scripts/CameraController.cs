using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float speed;
    public float minZoom = 0.1f;
    public float maxZoom = 10.0f;

    private Vector3 targetPosition;
    private Vector3 prevMousePosition;
    private Camera cameraComponent;


    private bool panning = false;

    void Start()
    {
        targetPosition = gameObject.transform.position;
        cameraComponent = GetComponent<Camera>();
    }

    void Update()
    {
        bool isHoveringOverMenu = EventSystem.current.IsPointerOverGameObject();
        if (Input.GetMouseButtonDown(0) && !isHoveringOverMenu)
        {
            panning = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            panning = false;
        }

        if (panning)
        {
             targetPosition += cameraComponent.ScreenToWorldPoint(prevMousePosition) - cameraComponent.ScreenToWorldPoint(Input.mousePosition);
        }
        int cityWidth = (int)(Road.MaxX - Road.MinX);
        int cityHeight = (int)(Road.MaxY - Road.MinY);
        targetPosition.x = Mathf.Clamp(targetPosition.x, Road.MinX - cityWidth * 0.5f, Road.MaxX + cityWidth * 0.5f);
        targetPosition.y = Mathf.Clamp(targetPosition.y, Road.MinY - cityHeight * 0.5f, Road.MaxY + cityHeight * 0.5f);
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, speed);
        prevMousePosition = Input.mousePosition;

        if (!isHoveringOverMenu)
            cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize - (Input.mouseScrollDelta.y * cameraComponent.orthographicSize / 10.0f), minZoom, maxZoom);
    }


    public void JumpTo(Vector3 position)
    {
        targetPosition = position;
        gameObject.transform.position = position;
    }
}
