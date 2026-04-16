using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraTransition : MonoBehaviour
{
    public static CameraTransition Instance { get; private set; }

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 0.3f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }
    }

    public Coroutine FadeOut()
    {
        return StartCoroutine(Fade(0f, 1f));
    }

    public Coroutine FadeIn()
    {
        return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        fadeImage.raycastTarget = true;
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        if (to == 0f)
            fadeImage.raycastTarget = false;
    }
}
