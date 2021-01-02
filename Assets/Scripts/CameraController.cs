using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float speed;

    private Vector3 targetPosition;
    private Vector3 prevMousePosition;
    private Camera cameraComponent;

    private float minZoom = 0.1f;
    private float maxZoom = 10.0f;

    void Start()
    {
        targetPosition = gameObject.transform.position;
        cameraComponent = GetComponent<Camera>();
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(0))
        {
             targetPosition += cameraComponent.ScreenToWorldPoint(prevMousePosition) - cameraComponent.ScreenToWorldPoint(Input.mousePosition);
        }
        targetPosition.x = Mathf.Clamp(targetPosition.x, Road.MinX, Road.MaxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, Road.MinY, Road.MaxY);
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, speed);
        prevMousePosition = Input.mousePosition;

        cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize - (Input.mouseScrollDelta.y * cameraComponent.orthographicSize / 10.0f), minZoom, maxZoom);
    }


    public void JumpTo(Vector3 position)
    {
        targetPosition = position;
        gameObject.transform.position = position;
    }
}
