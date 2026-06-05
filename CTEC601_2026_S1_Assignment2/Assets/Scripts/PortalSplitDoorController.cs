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

    [Header("Door Open / Close Audio")]
    public AudioSource doorAudioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public float doorSoundVolume = 1f;

    [Header("Electrical Spark Audio")]
    public AudioSource sparkAudioSource;
    public AudioClip sparkLoopSound;
    public float sparkVolume = 0.7f;

    [Header("Door Sparks Optional")]
    public ParticleSystem[] doorSparks;

    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    private bool isOpen;
    private bool previousOpenState;

    private void Start()
    {
        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
        }

        if (sparkAudioSource != null)
        {
            sparkAudioSource.playOnAwake = false;
            sparkAudioSource.loop = true;
            sparkAudioSource.volume = sparkVolume;

            if (sparkLoopSound != null)
            {
                sparkAudioSource.clip = sparkLoopSound;
            }
        }

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
        previousOpenState = isOpen;

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

            StartSparks();
            StartSparkSound();
        }
        else
        {
            StopSparks();
            StopSparkSound();
        }
    }

    private void Update()
    {
        if (leftDoor != null)
        {
            Vector3 targetLeftPosition = isOpen ? leftOpenPosition : leftClosedPosition;

            leftDoor.localPosition = Vector3.MoveTowards(
                leftDoor.localPosition,
                targetLeftPosition,
                moveSpeed * Time.deltaTime
            );
        }

        if (rightDoor != null)
        {
            Vector3 targetRightPosition = isOpen ? rightOpenPosition : rightClosedPosition;

            rightDoor.localPosition = Vector3.MoveTowards(
                rightDoor.localPosition,
                targetRightPosition,
                moveSpeed * Time.deltaTime
            );
        }

        if (isOpen != previousOpenState)
        {
            if (isOpen)
            {
                PlayDoorSound(openSound);
                StartSparks();
                StartSparkSound();
            }
            else
            {
                PlayDoorSound(closeSound);
                StopSparks();
                StopSparkSound();
            }

            previousOpenState = isOpen;
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

    private void PlayDoorSound(AudioClip clip)
    {
        if (doorAudioSource == null)
        {
            return;
        }

        if (clip == null)
        {
            return;
        }

        doorAudioSource.PlayOneShot(clip, doorSoundVolume);
    }

    private void StartSparkSound()
    {
        if (sparkAudioSource == null)
        {
            return;
        }

        if (sparkLoopSound != null)
        {
            sparkAudioSource.clip = sparkLoopSound;
        }

        sparkAudioSource.volume = sparkVolume;
        sparkAudioSource.loop = true;

        if (!sparkAudioSource.isPlaying)
        {
            sparkAudioSource.Play();
        }
    }

    private void StopSparkSound()
    {
        if (sparkAudioSource == null)
        {
            return;
        }

        if (sparkAudioSource.isPlaying)
        {
            sparkAudioSource.Stop();
        }
    }

    private void StartSparks()
    {
        if (doorSparks == null)
        {
            return;
        }

        foreach (ParticleSystem spark in doorSparks)
        {
            if (spark != null && !spark.isPlaying)
            {
                spark.Play();
            }
        }
    }

    private void StopSparks()
    {
        if (doorSparks == null)
        {
            return;
        }

        foreach (ParticleSystem spark in doorSparks)
        {
            if (spark != null)
            {
                spark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}