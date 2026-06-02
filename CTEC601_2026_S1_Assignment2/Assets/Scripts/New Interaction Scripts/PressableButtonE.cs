using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PressableButtonE : MonoBehaviour
{
    [Header("Button Part")]
    public Transform redButton;

    [Header("Button Movement")]
    public float pressedDownAmount = 0.12f;
    public float moveSpeed = 8f;
    public float returnDelay = 2f;

    [Header("Interaction")]
    public string playerTag = "Player";

    [Header("Events For Later")]
    public UnityEvent onButtonPressed;

    private Vector3 startLocalPosition;
    private Vector3 pressedLocalPosition;

    private bool playerIsInside = false;
    private bool isMoving = false;

    private void Start()
    {
        if (redButton == null)
        {
            Debug.LogError("Red Button is not assigned on " + gameObject.name);
            return;
        }

        startLocalPosition = redButton.localPosition;
        pressedLocalPosition = startLocalPosition + Vector3.down * pressedDownAmount;
    }

    private void Update()
    {
        if (playerIsInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        if (redButton == null)
            return;

        if (isMoving)
            return;

        StartCoroutine(ButtonPressRoutine());

        // This still activates anything you connect later, like the cube dropper
        onButtonPressed.Invoke();
    }

    private IEnumerator ButtonPressRoutine()
    {
        isMoving = true;

        // Move button down
        yield return MoveButtonToPosition(pressedLocalPosition);

        // Wait before going back up
        yield return new WaitForSeconds(returnDelay);

        // Move button back up
        yield return MoveButtonToPosition(startLocalPosition);

        isMoving = false;
    }

    private IEnumerator MoveButtonToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(redButton.localPosition, targetPosition) > 0.001f)
        {
            redButton.localPosition = Vector3.Lerp(
                redButton.localPosition,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }

        redButton.localPosition = targetPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsInside = true;
            Debug.Log("Player near button. Press E.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsInside = false;
        }
    }
}