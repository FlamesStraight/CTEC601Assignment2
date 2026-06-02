using UnityEngine;

public class PortalDoorTrigger : MonoBehaviour
{
    public enum DoorAction
    {
        Open,
        Close,
        Toggle
    }

    [Header("Door Controller")]
    public PortalSplitDoorController doorController;

    [Header("Trigger Action")]
    public DoorAction action = DoorAction.Open;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (doorController == null)
        {
            Debug.LogWarning("Door controller is not assigned.");
            return;
        }

        if (action == DoorAction.Open)
        {
            doorController.OpenDoor();
        }
        else if (action == DoorAction.Close)
        {
            doorController.CloseDoor();
        }
        else if (action == DoorAction.Toggle)
        {
            doorController.ToggleDoor();
        }
    }
}