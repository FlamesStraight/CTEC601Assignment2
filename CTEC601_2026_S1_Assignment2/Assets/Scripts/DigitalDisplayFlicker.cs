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

    [Tooltip("Lower this if the display image looks too white/bright.")]
    public Color imageTint = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Startup Flicker")]
    public int startupFlickerBursts = 8;
    public float startupMinOnTime = 0.04f;
    public float startupMaxOnTime = 0.14f;
    public float startupMinOffTime = 0.04f;
    public float startupMaxOffTime = 0.18f;

    [Header("Random Flicker After Startup")]
    public bool flickerAfterStart = true;
    public float minTimeBetweenFlickers = 1.5f;
    public float maxTimeBetweenFlickers = 4f;
    public float minFlickerDuration = 0.05f;
    public float maxFlickerDuration = 0.25f;
    public int minFlickerBursts = 1;
    public int maxFlickerBursts = 4;

    private Material screenMaterial;

    private void Start()
    {
        if (screenRenderer == null)
        {
            screenRenderer = GetComponent<Renderer>();
        }

        if (screenRenderer == null)
        {
            Debug.LogError("DigitalDisplayFlicker: No screen renderer assigned.");
            return;
        }

        screenMaterial = new Material(screenRenderer.material);
        screenRenderer.material = screenMaterial;

        TurnScreenOff();

        StartCoroutine(StartScreenSequence());
    }

    private IEnumerator StartScreenSequence()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < startupFlickerBursts; i++)
        {
            TurnScreenOn();
            yield return new WaitForSeconds(Random.Range(startupMinOnTime, startupMaxOnTime));

            TurnScreenOff();
            yield return new WaitForSeconds(Random.Range(startupMinOffTime, startupMaxOffTime));
        }

        TurnScreenOn();

        if (flickerAfterStart)
        {
            StartCoroutine(RandomFlickerLoop());
        }
    }

    private IEnumerator RandomFlickerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers));

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

    private void TurnScreenOn()
    {
        if (screenMaterial == null)
        {
            return;
        }

        screenMaterial.mainTexture = displayTexture;
        screenMaterial.color = imageTint;

        if (screenMaterial.HasProperty("_EmissionColor"))
        {
            screenMaterial.DisableKeyword("_EMISSION");
            screenMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    private void TurnScreenOff()
    {
        if (screenMaterial == null)
        {
            return;
        }

        screenMaterial.mainTexture = null;
        screenMaterial.color = offColor;

        if (screenMaterial.HasProperty("_EmissionColor"))
        {
            screenMaterial.DisableKeyword("_EMISSION");
            screenMaterial.SetColor("_EmissionColor", Color.black);
        }
    }
}