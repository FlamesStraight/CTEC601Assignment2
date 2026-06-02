using System.Collections.Generic;
using UnityEngine;

public class PressureButtonDoorController : MonoBehaviour
{
    [Header("Button Visual")]
    public Transform redButtonVisual;
    public float pressDistance = 0.08f;
    public float buttonMoveSpeed = 5f;

    [Header("Door")]
    public PortalSplitDoorController doorController;

    [Header("Status Screen Optional")]
    public MeshRenderer statusScreenRenderer;
    public Material redMaterial;
    public Material blueMaterial;

    [Header("Trigger Settings")]
    public bool playerOnly = false;
    public string playerTag = "Player";
    public string cubeTag = "Cube";

    private Vector3 buttonUpPosition;
    private Vector3 buttonDownPosition;

    private readonly HashSet<GameObject> pressingObjects = new HashSet<GameObject>();

    private void Start()
    {
        if (redButtonVisual != null)
        {
            buttonUpPosition = redButtonVisual.localPosition;
            buttonDownPosition = buttonUpPosition + Vector3.down * pressDistance;
        }

        UpdateButtonState();
    }

    private void Update()
    {
        bool isPressed = pressingObjects.Count > 0;

        if (redButtonVisual != null)
        {
            Vector3 targetPosition = isPressed ? buttonDownPosition : buttonUpPosition;

            redButtonVisual.localPosition = Vector3.Lerp(
                redButtonVisual.localPosition,
                targetPosition,
                Time.deltaTime * buttonMoveSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject validObject = GetValidPressingObject(other);

        if (validObject == null)
            return;

        pressingObjects.Add(validObject);
        UpdateButtonState();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject validObject = GetValidPressingObject(other);

        if (validObject == null)
            return;

        pressingObjects.Remove(validObject);
        UpdateButtonState();
    }

    private GameObject GetValidPressingObject(Collider other)
    {
        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag(playerTag))
            {
                return current.gameObject;
            }

            if (!playerOnly && current.CompareTag(cubeTag))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    private void UpdateButtonState()
    {
        bool isPressed = pressingObjects.Count > 0;

        if (doorController != null)
        {
            doorController.SetDoorOpen(isPressed);
        }

        if (statusScreenRenderer != null)
        {
            if (isPressed && blueMaterial != null)
            {
                statusScreenRenderer.material = blueMaterial;
            }
            else if (!isPressed && redMaterial != null)
            {
                statusScreenRenderer.material = redMaterial;
            }
        }
    }
}