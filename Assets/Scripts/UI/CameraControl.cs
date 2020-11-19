using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Camera controlledCamera;

    public float mapCameraZDistance;

    public float panSpeed;
    public float zoomSpeed;

    public float minHorizontal;
    public float maxHorizontal;

    public float minVertical;
    public float maxVertical;

    public float minSize;
    public float maxSize;

    private void Start()
    {
        Vector3 mapCenter = HexGridManager.Instance.GetMapCenter();
        controlledCamera.transform.position = new Vector3(mapCenter.x, mapCenter.y, mapCameraZDistance);
    }

    private void Update()
    {
        float nextPositionX = this.transform.position.x;
        float nextPositionY = this.transform.position.y;

        if (Input.GetKey(KeyCode.W) == true &&
            Input.GetKey(KeyCode.S) == false)
        {
            nextPositionY = Mathf.Clamp(nextPositionY + panSpeed * Time.deltaTime, minVertical, maxVertical);
        }

        if (Input.GetKey(KeyCode.W) == false &&
            Input.GetKey(KeyCode.S) == true)
        {
            nextPositionY = Mathf.Clamp(nextPositionY - panSpeed * Time.deltaTime, minVertical, maxVertical);
        }

        if (Input.GetKey(KeyCode.A) == false &&
            Input.GetKey(KeyCode.D) == true)
        {
            nextPositionX = Mathf.Clamp(nextPositionX + panSpeed * Time.deltaTime, minHorizontal, maxHorizontal);
        }

        if (Input.GetKey(KeyCode.A) == true &&
            Input.GetKey(KeyCode.D) == false)
        {
            nextPositionX = Mathf.Clamp(nextPositionX - panSpeed * Time.deltaTime, minHorizontal, maxHorizontal);
        }

        controlledCamera.transform.position = new Vector3(nextPositionX, nextPositionY, mapCameraZDistance);

        if (Input.mouseScrollDelta.y != 0.0f)
        {
            controlledCamera.orthographicSize = Mathf.Clamp(controlledCamera.orthographicSize - Input.mouseScrollDelta.y * zoomSpeed, minSize, maxSize);
        }
    }
}