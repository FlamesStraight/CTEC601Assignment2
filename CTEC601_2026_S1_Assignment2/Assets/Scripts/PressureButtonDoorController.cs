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

    [Header("Status Screen")]
    public Renderer statusScreenRenderer;
    public Material inactiveMaterial; // Red material
    public Material activeMaterial;   // Blue material
    public GameObject checkMarkObject;

    [Header("Trigger Settings")]
    public bool playerOnly = true;
    public string playerTag = "Player";

    private Vector3 buttonUpPosition;
    private Vector3 buttonDownPosition;

    private bool isPressed = false;

    private readonly HashSet<Collider> collidersOnButton = new HashSet<Collider>();

    private void Start()
    {
        if (redButtonVisual != null)
        {
            buttonUpPosition = redButtonVisual.localPosition;
            buttonDownPosition = buttonUpPosition + new Vector3(0f, -pressDistance, 0f);
        }

        SetPressedState(false);
    }

    private void Update()
    {
        if (redButtonVisual != null)
        {
            Vector3 targetPosition = isPressed ? buttonDownPosition : buttonUpPosition;

            redButtonVisual.localPosition = Vector3.MoveTowards(
                redButtonVisual.localPosition,
                targetPosition,
                buttonMoveSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidPressObject(other)) return;

        collidersOnButton.Add(other);

        if (collidersOnButton.Count > 0)
        {
            SetPressedState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidPressObject(other)) return;

        if (collidersOnButton.Contains(other))
        {
            collidersOnButton.Remove(other);
        }

        if (collidersOnButton.Count == 0)
        {
            SetPressedState(false);
        }
    }

    private bool IsValidPressObject(Collider other)
    {
        if (other == null) return false;

        // Stops the button from detecting its own visual parts.
        if (other.transform.IsChildOf(transform)) return false;

        if (redButtonVisual != null && other.transform.IsChildOf(redButtonVisual)) return false;

        if (!playerOnly)
        {
            return true;
        }

        if (other.CompareTag(playerTag))
        {
            return true;
        }

        if (other.transform.root != null && other.transform.root.CompareTag(playerTag))
        {
            return true;
        }

        Transform current = other.transform.parent;

        while (current != null)
        {
            if (current.CompareTag(playerTag))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void SetPressedState(bool pressed)
    {
        isPressed = pressed;

        if (doorController != null)
        {
            doorController.SetDoorOpen(pressed);
        }

        if (statusScreenRenderer != null)
        {
            if (pressed && activeMaterial != null)
            {
                statusScreenRenderer.material = activeMaterial;
            }
            else if (!pressed && inactiveMaterial != null)
            {
                statusScreenRenderer.material = inactiveMaterial;
            }
        }

        if (checkMarkObject != null)
        {
            checkMarkObject.SetActive(pressed);
        }
    }
}