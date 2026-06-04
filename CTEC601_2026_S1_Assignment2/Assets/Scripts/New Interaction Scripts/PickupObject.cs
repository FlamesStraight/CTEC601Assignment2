using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using TMPro;

public class ReadableClipboard : MonoBehaviour
{
    [Header("Player Camera")]
    public Camera playerCamera;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3f;

    [Header("Hold Position")]
    public Vector3 holdLocalPosition = new Vector3(0f, -0.15f, 1.2f);
    public Vector3 holdLocalRotation = new Vector3(70f, 0f, 0f);

    [Header("Scale")]
    public bool keepOriginalScaleWhenHeld = true;
    public Vector3 customHeldScale = Vector3.one;

    [Header("Instruction Text")]
    public TMP_Text instructionsText;

    [TextArea(8, 15)]
    public string manualText =
@"HOW TO PLAY

WASD - Move
Mouse - Look Around
Shift - Sprint
Space - Jump

E - Interact / Pick Up
E again - Drop Object

Press the small red button to release the cube.
Pick up the companion cube and place it on the large red pressure button.
The pressure button opens the portal door.
Walk through the open door to continue.";

    [Header("Drop Settings")]
    public float dropForwardForce = 0.5f;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private Rigidbody rb;
    private Collider[] colliders;
    private bool[] colliderOriginalStates;

    private bool originalUseGravity;
    private bool originalIsKinematic;

    private bool isHeld = false;
    private Transform holdPoint;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();

        colliderOriginalStates = new bool[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            colliderOriginalStates[i] = colliders[i].enabled;
        }

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;

        if (rb != null)
        {
            originalUseGravity = rb.useGravity;
            originalIsKinematic = rb.isKinematic;
        }

        if (instructionsText != null)
        {
            instructionsText.text = manualText;
        }

        CreateHoldPoint();
    }

    private void Update()
    {
        if (PressedInteractKey())
        {
            if (isHeld)
            {
                DropClipboard();
            }
            else
            {
                TryPickUpClipboard();
            }
        }
    }

    private bool PressedInteractKey()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(interactKey))
        {
            return true;
        }
#endif

        return false;
    }

    private void CreateHoldPoint()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("ReadableClipboard: No player camera assigned.");
            return;
        }

        GameObject holdObject = new GameObject("ClipboardHoldPoint");
        holdObject.transform.SetParent(playerCamera.transform);
        holdObject.transform.localPosition = holdLocalPosition;
        holdObject.transform.localRotation = Quaternion.Euler(holdLocalRotation);

        holdPoint = holdObject.transform;
    }

    private void TryPickUpClipboard()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, ~0, QueryTriggerInteraction.Ignore))
        {
            bool hitThisClipboard = hit.transform == transform || hit.transform.IsChildOf(transform);

            if (hitThisClipboard)
            {
                PickUpClipboard();
            }
        }
    }

    private void PickUpClipboard()
    {
        if (holdPoint == null)
        {
            return;
        }

        isHeld = true;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif

            rb.angularVelocity = Vector3.zero;
        }

        SetCollidersEnabled(false);

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (keepOriginalScaleWhenHeld)
        {
            transform.localScale = originalLocalScale;
        }
        else
        {
            transform.localScale = customHeldScale;
        }
    }

    private void DropClipboard()
    {
        isHeld = false;

        transform.SetParent(originalParent);

        SetCollidersEnabled(true);

        if (rb != null)
        {
            rb.useGravity = originalUseGravity;
            rb.isKinematic = originalIsKinematic;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = playerCamera.transform.forward * dropForwardForce;
#else
            rb.velocity = playerCamera.transform.forward * dropForwardForce;
#endif

            rb.angularVelocity = Vector3.zero;
        }

        transform.localScale = originalLocalScale;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                if (enabled)
                {
                    colliders[i].enabled = colliderOriginalStates[i];
                }
                else
                {
                    colliders[i].enabled = false;
                }
            }
        }
    }
}