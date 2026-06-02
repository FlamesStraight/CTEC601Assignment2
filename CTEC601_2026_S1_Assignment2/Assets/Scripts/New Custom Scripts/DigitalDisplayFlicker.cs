using System.Collections;
using UnityEngine;

public class DigitalDisplayFlicker : MonoBehaviour
{
    [Header("Screen Renderer")]
    public Renderer screenRenderer;

    [Header("Display Image")]
    public Texture displayTexture;

    [Header("Startup")]
    public float startDelay = 10f;

    [Header("Screen Colours")]
    public Color offColor = Color.black;

    // This controls how bright the image looks when ON
    public Color imageTint = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Flicker Settings")]
    public bool flickerAfterStart = true;
    public float minTimeBetweenFlickers = 1.5f;
    public float maxTimeBetweenFlickers = 4f;
    public float minFlickerDuration = 0.05f;
    public float maxFlickerDuration = 0.25f;
    public int minFlickerBursts = 1;
    public int maxFlickerBursts = 4;

    private Material screenMaterial;

    void Start()
    {
        if (screenRenderer == null)
            screenRenderer = GetComponent<Renderer>();

        screenMaterial = screenRenderer.material;

        TurnScreenOff();
        StartCoroutine(StartScreenSequence());
    }

    IEnumerator StartScreenSequence()
    {
        // wait before screen starts turning on
        yield return new WaitForSeconds(startDelay);

        // startup flicker
        int startupBursts = 6;

        for (int i = 0; i < startupBursts; i++)
        {
            TurnScreenOn();
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            TurnScreenOff();
            yield return new WaitForSeconds(Random.Range(0.05f, 0.18f));
        }

        TurnScreenOn();

        if (flickerAfterStart)
        {
            StartCoroutine(RandomFlickerLoop());
        }
    }

    IEnumerator RandomFlickerLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
            yield return new WaitForSeconds(waitTime);

            int burstCount = Random.Range(minFlickerBursts, maxFlickerBursts + 1);

            for (int i = 0; i < burstCount; i++)
            {
                TurnScreenOff();
                yield return new WaitForSeconds(Random.Range(minFlickerDuration, maxFlickerDuration));

                TurnScreenOn();
                yield return new WaitForSeconds(Random.Range(minFlickerDuration, maxFlickerDuration));
            }

            TurnScreenOn();
        }
    }

    void TurnScreenOn()
    {
        if (displayTexture != null)
        {
            screenMaterial.mainTexture = displayTexture;
        }

        // darker grey tint instead of bright white
        screenMaterial.color = imageTint;

        if (screenMaterial.HasProperty("_EmissionColor"))
        {
            screenMaterial.DisableKeyword("_EMISSION");
            screenMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    void TurnScreenOff()
    {
        // remove texture entirely so OFF is really black
        screenMaterial.mainTexture = null;
        screenMaterial.color = offColor;

        if (screenMaterial.HasProperty("_EmissionColor"))
        {
            screenMaterial.DisableKeyword("_EMISSION");
            screenMaterial.SetColor("_EmissionColor", Color.black);
        }
    }
}