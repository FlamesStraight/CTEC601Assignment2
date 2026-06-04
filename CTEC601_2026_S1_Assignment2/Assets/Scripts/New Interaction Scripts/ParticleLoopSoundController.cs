using UnityEngine;

public class ParticleLoopSoundController : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem targetParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip loopSound;
    public float volume = 0.25f;

    [Header("Settings")]
    public bool playSoundWhenParticlesPlay = true;
    public bool stopSoundWhenParticlesStop = true;

    private void Start()
    {
        if (targetParticles == null)
        {
            targetParticles = GetComponent<ParticleSystem>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = volume;

            if (loopSound != null)
            {
                audioSource.clip = loopSound;
            }
        }
    }

    private void Update()
    {
        if (targetParticles == null || audioSource == null)
        {
            return;
        }

        bool particlesAreActive = targetParticles.isPlaying || targetParticles.isEmitting;

        if (playSoundWhenParticlesPlay && particlesAreActive)
        {
            if (!audioSource.isPlaying)
            {
                if (loopSound != null)
                {
                    audioSource.clip = loopSound;
                }

                audioSource.volume = volume;
                audioSource.Play();
            }
        }

        if (stopSoundWhenParticlesStop && !particlesAreActive)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}