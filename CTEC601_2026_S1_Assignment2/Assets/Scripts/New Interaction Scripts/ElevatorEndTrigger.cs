using UnityEngine;

public class ElevatorEndTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("End Screen")]
    public GameObject endGameCanvas;

    [Header("Optional End Sound")]
    public AudioSource audioSource;
    public AudioClip endSound;
    public float soundVolume = 1f;

    [Header("Freeze Player")]
    public bool freezePlayer = true;
    public Rigidbody playerRigidbody;
    public MonoBehaviour[] playerScriptsToDisable;

    [Header("Cursor")]
    public bool unlockCursor = true;

    private bool hasEnded = false;

    private void Start()
    {
        if (endGameCanvas != null)
        {
            endGameCanvas.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasEnded)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        EndPlaythrough(other);
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            return true;
        }

        if (other.transform.root.CompareTag(playerTag))
        {
            return true;
        }

        return false;
    }

    private void EndPlaythrough(Collider playerCollider)
    {
        hasEnded = true;

        if (endGameCanvas != null)
        {
            endGameCanvas.SetActive(true);
        }

        if (audioSource != null && endSound != null)
        {
            audioSource.PlayOneShot(endSound, soundVolume);
        }

        if (freezePlayer)
        {
            FreezePlayer(playerCollider);
        }

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log("Playthrough ended: player reached the elevator.");
    }

    private void FreezePlayer(Collider playerCollider)
    {
        if (playerScriptsToDisable != null)
        {
            foreach (MonoBehaviour script in playerScriptsToDisable)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        if (playerRigidbody == null)
        {
            playerRigidbody = playerCollider.GetComponentInParent<Rigidbody>();
        }

        if (playerRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            playerRigidbody.linearVelocity = Vector3.zero;
#else
            playerRigidbody.velocity = Vector3.zero;
#endif
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }
    }
}