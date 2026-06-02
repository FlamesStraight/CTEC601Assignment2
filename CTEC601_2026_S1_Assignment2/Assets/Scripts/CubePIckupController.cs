using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CubePickupController : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;

    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public float holdDistance = 2f;
    public float holdHeight = -0.15f;
    public float followStrength = 14f;
    public float maxFollowSpeed = 8f;
    public float breakDistance = 4f;

    [Header("Optional Filter")]
    public string requiredTag = ""; 
    // Leave this empty if you want it to pick up any Rigidbody.
    // If you only want the cube to be picked up, set the cube tag to PickupCube,
    // then type PickupCube here.

    public LayerMask pickupMask = ~0;

    private Transform holdPoint;
    private Rigidbody heldRb;

    private bool originalUseGravity;
    private bool originalIsKinematic;
    private CollisionDetectionMode originalCollisionMode;
    private RigidbodyInterpolation originalInterpolation;
    private RigidbodyConstraints originalConstraints;

#if UNITY_6000_0_OR_NEWER
    private float originalLinearDamping;
    private float originalAngularDamping;
#else
    private float originalDrag;
    private float originalAngularDrag;
#endif

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponent<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        CreateHoldPoint();
    }

    private void Update()
    {
        if (PressedE())
        {
            if (heldRb == null)
            {
                TryPickup();
            }
            else
            {
                DropCube();
            }
        }

        if (heldRb != null)
        {
            float distanceFromHoldPoint = Vector3.Distance(heldRb.position, holdPoint.position);

            if (distanceFromHoldPoint > breakDistance)
            {
                DropCube();
            }
        }
    }

    private void FixedUpdate()
    {
        if (heldRb == null)
        {
            return;
        }

        MoveHeldCube();
    }

    private bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private void CreateHoldPoint()
    {
        GameObject holdObject = new GameObject("CubeHoldPoint");
        holdObject.transform.SetParent(playerCamera.transform);
        holdObject.transform.localPosition = new Vector3(0f, holdHeight, holdDistance);
        holdObject.transform.localRotation = Quaternion.identity;

        holdPoint = holdObject.transform;
    }

    private void TryPickup()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask, QueryTriggerInteraction.Ignore))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;

            if (rb == null)
            {
                rb = hit.collider.GetComponentInParent<Rigidbody>();
            }

            if (rb == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(requiredTag))
            {
                bool hasCorrectTag = rb.gameObject.tag == requiredTag || rb.transform.root.gameObject.tag == requiredTag;

                if (!hasCorrectTag)
                {
                    return;
                }
            }

            PickupCube(rb);
        }
    }

    private void PickupCube(Rigidbody rb)
    {
        heldRb = rb;

        originalUseGravity = heldRb.useGravity;
        originalIsKinematic = heldRb.isKinematic;
        originalCollisionMode = heldRb.collisionDetectionMode;
        originalInterpolation = heldRb.interpolation;
        originalConstraints = heldRb.constraints;

#if UNITY_6000_0_OR_NEWER
        originalLinearDamping = heldRb.linearDamping;
        originalAngularDamping = heldRb.angularDamping;
#else
        originalDrag = heldRb.drag;
        originalAngularDrag = heldRb.angularDrag;
#endif

        heldRb.useGravity = false;
        heldRb.isKinematic = false;
        heldRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        heldRb.interpolation = RigidbodyInterpolation.Interpolate;

#if UNITY_6000_0_OR_NEWER
        heldRb.linearDamping = 8f;
        heldRb.angularDamping = 8f;
#else
        heldRb.drag = 8f;
        heldRb.angularDrag = 8f;
#endif
    }

    private void MoveHeldCube()
    {
        Vector3 targetPosition = holdPoint.position;
        Vector3 directionToTarget = targetPosition - heldRb.position;

        Vector3 desiredVelocity = directionToTarget * followStrength;

        if (desiredVelocity.magnitude > maxFollowSpeed)
        {
            desiredVelocity = desiredVelocity.normalized * maxFollowSpeed;
        }

#if UNITY_6000_0_OR_NEWER
        heldRb.linearVelocity = desiredVelocity;
#else
        heldRb.velocity = desiredVelocity;
#endif

        heldRb.angularVelocity = Vector3.Lerp(
            heldRb.angularVelocity,
            Vector3.zero,
            10f * Time.fixedDeltaTime
        );
    }

    private void DropCube()
    {
        if (heldRb == null)
        {
            return;
        }

        heldRb.useGravity = originalUseGravity;
        heldRb.isKinematic = originalIsKinematic;
        heldRb.collisionDetectionMode = originalCollisionMode;
        heldRb.interpolation = originalInterpolation;
        heldRb.constraints = originalConstraints;

#if UNITY_6000_0_OR_NEWER
        heldRb.linearDamping = originalLinearDamping;
        heldRb.angularDamping = originalAngularDamping;
#else
        heldRb.drag = originalDrag;
        heldRb.angularDrag = originalAngularDrag;
#endif

        heldRb = null;
    }
}