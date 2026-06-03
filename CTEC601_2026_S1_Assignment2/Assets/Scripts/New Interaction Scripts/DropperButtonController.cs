using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DropperButtonController : MonoBehaviour
{
    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("Button Visual")]
    public Transform redButtonVisual;
    public float pressDistance = 0.08f;
    public float buttonMoveSpeed = 8f;
    public float buttonStayDownTime = 2f;

    [Header("Dropper Barrier")]
    public GameObject hiddenBarrier;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip buttonPressSound;
    public AudioClip buttonReleaseSound;

    [Header("Settings")]
    public bool singleUse = true;
    public bool showDebugMessages = true;

    private Vector3 originalButtonLocalPosition;
    private Vector3 pressedButtonLocalPosition;

    private bool playerInside = false;
    private bool hasBeenUsed = false;
    private bool isPressing = false;

    private void Start()
    {
        if (redButtonVisual != null)
        {
            originalButtonLocalPosition = redButtonVisual.localPosition;
            pressedButtonLocalPosition = originalButtonLocalPosition + Vector3.down * pressDistance;
        }

        if (hiddenBarrier != null)
        {
            hiddenBarrier.SetActive(true);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (hasBeenUsed && singleUse)
            return;

        if (!playerInside)
            return;

        if (isPressing)
            return;

        if (PressedInteractKey())
        {
            if (showDebugMessages)
                Debug.Log("E pressed near dropper button.");

            StartCoroutine(PressButtonAndDropCube());
        }
    }

    private bool PressedInteractKey()
    {
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            pressed = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
        {
            pressed = true;
        }
#endif

        return pressed;
    }

    private IEnumerator PressButtonAndDropCube()
    {
        isPressing = true;

        PlaySound(buttonPressSound);

        yield return MoveButton(pressedButtonLocalPosition);

        if (hiddenBarrier != null)
        {
            hiddenBarrier.SetActive(false);

            if (showDebugMessages)
                Debug.Log("Dropper hidden barrier disabled. Cube should fall now.");
        }
        else
        {
            Debug.LogWarning("No hidden barrier assigned on DropperButtonController.");
        }

        yield return new WaitForSeconds(buttonStayDownTime);

        yield return MoveButton(originalButtonLocalPosition);

        PlaySound(buttonReleaseSound);

        if (singleUse)
        {
            hasBeenUsed = true;
        }

        isPressing = false;
    }

    private IEnumerator MoveButton(Vector3 targetLocalPosition)
    {
        if (redButtonVisual == null)
        {
            Debug.LogWarning("No red button visual assigned on DropperButtonController.");
            yield break;
        }

        while (Vector3.Distance(redButtonVisual.localPosition, targetLocalPosition) > 0.001f)
        {
            redButtonVisual.localPosition = Vector3.MoveTowards(
                redButtonVisual.localPosition,
                targetLocalPosition,
                buttonMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        redButtonVisual.localPosition = targetLocalPosition;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = true;

            if (showDebugMessages)
                Debug.Log("Player entered dropper button area.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInside = false;

            if (showDebugMessages)
                Debug.Log("Player left dropper button area.");
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag))
            return true;

        if (other.transform.root.CompareTag(playerTag))
            return true;

        return false;
    }
}