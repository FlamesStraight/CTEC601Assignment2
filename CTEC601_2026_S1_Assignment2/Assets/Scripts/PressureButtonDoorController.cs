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
    public GameObject checkMarkObject;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip buttonPressSound;
    public AudioClip buttonReleaseSound;
    public float soundVolume = 1f;

    [Header("Trigger Settings")]
    public bool playerOnly = false;
    public string playerTag = "Player";
    public string cubeTag = "Cube";

    private Vector3 buttonUpPosition;
    private Vector3 buttonDownPosition;

    private bool isPressed = false;

    private readonly HashSet<GameObject> pressingObjects = new HashSet<GameObject>();

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (redButtonVisual != null)
        {
            buttonUpPosition = redButtonVisual.localPosition;
            buttonDownPosition = buttonUpPosition + Vector3.down * pressDistance;
        }

        UpdateButtonState(false, false);
    }

    private void Update()
    {
        MoveButtonVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject validObject = GetValidPressingObject(other);

        if (validObject == null)
        {
            return;
        }

        pressingObjects.Add(validObject);

        bool shouldBePressed = pressingObjects.Count > 0;
        UpdateButtonState(shouldBePressed, true);
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject validObject = GetValidPressingObject(other);

        if (validObject == null)
        {
            return;
        }

        if (pressingObjects.Contains(validObject))
        {
            pressingObjects.Remove(validObject);
        }

        bool shouldBePressed = pressingObjects.Count > 0;
        UpdateButtonState(shouldBePressed, true);
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

    private void UpdateButtonState(bool pressed, bool playSound)
    {
        if (isPressed == pressed)
        {
            return;
        }

        isPressed = pressed;

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

        if (checkMarkObject != null)
        {
            checkMarkObject.SetActive(isPressed);
        }

        if (playSound)
        {
            if (isPressed)
            {
                PlaySound(buttonPressSound);
            }
            else
            {
                PlaySound(buttonReleaseSound);
            }
        }
    }

    private void MoveButtonVisual()
    {
        if (redButtonVisual == null)
        {
            return;
        }

        Vector3 targetPosition = isPressed ? buttonDownPosition : buttonUpPosition;

        redButtonVisual.localPosition = Vector3.MoveTowards(
            redButtonVisual.localPosition,
            targetPosition,
            buttonMoveSpeed * Time.deltaTime
        );
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
        {
            return;
        }

        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, soundVolume);
    }
}