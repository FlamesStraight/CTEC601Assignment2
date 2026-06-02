using UnityEngine;

public class PortalSplitDoorController : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Opening Movement")]
    public Vector3 leftOpenOffset = new Vector3(-1.2f, 0f, 0f);
    public Vector3 rightOpenOffset = new Vector3(1.2f, 0f, 0f);

    [Header("Speed")]
    public float moveSpeed = 3f;

    [Header("Start State")]
    public bool startOpen = false;

    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    private bool isOpen;

    private void Start()
    {
        if (leftDoor != null)
        {
            leftClosedPosition = leftDoor.localPosition;
            leftOpenPosition = leftClosedPosition + leftOpenOffset;
        }

        if (rightDoor != null)
        {
            rightClosedPosition = rightDoor.localPosition;
            rightOpenPosition = rightClosedPosition + rightOpenOffset;
        }

        isOpen = startOpen;

        if (startOpen)
        {
            if (leftDoor != null)
            {
                leftDoor.localPosition = leftOpenPosition;
            }

            if (rightDoor != null)
            {
                rightDoor.localPosition = rightOpenPosition;
            }
        }
    }

    private void Update()
    {
        if (leftDoor != null)
        {
            Vector3 targetPosition = isOpen ? leftOpenPosition : leftClosedPosition;

            leftDoor.localPosition = Vector3.MoveTowards(
                leftDoor.localPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }

        if (rightDoor != null)
        {
            Vector3 targetPosition = isOpen ? rightOpenPosition : rightClosedPosition;

            rightDoor.localPosition = Vector3.MoveTowards(
                rightDoor.localPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    public void SetDoorOpen(bool open)
    {
        isOpen = open;
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }

    public bool IsDoorOpen()
    {
        return isOpen;
    }
}