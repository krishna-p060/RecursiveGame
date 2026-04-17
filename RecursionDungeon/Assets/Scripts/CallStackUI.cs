using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CallStackUI : MonoBehaviour
{
    public static CallStackUI Instance { get; private set; }

    [Header("Call Stack Container")]
    public Transform stackContainer;

    [Header("Frame Prefab")]
    public GameObject framePrefab;

    [Header("Colors")]
    public Color normalFrameColor = new Color(0.15f, 0.15f, 0.25f, 0.9f);
    public Color highlightFrameColor = new Color(0.2f, 0.4f, 0.7f, 0.95f);
    public Color baseCaseColor = new Color(0.1f, 0.5f, 0.2f, 0.95f);

    private List<GameObject> frameObjects = new List<GameObject>();
    private int highlightedIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PushFrame(int depth, string roomName, string monsterName, string requiredWeapon)
    {
        if (framePrefab == null || stackContainer == null) return;

        GameObject frame = Instantiate(framePrefab, stackContainer);
        frame.name = $"Frame_Depth{depth}";
        frame.SetActive(true);

        // Find the main text (the one that says "FrameText")
        Text[] allTexts = frame.GetComponentsInChildren<Text>(true);
        foreach (var t in allTexts)
        {
            if (t.gameObject.name == "FrameText")
            {
                if (string.IsNullOrEmpty(monsterName))
                    t.text = $"ROOM {depth}: {roomName}\n<BASE CASE>";
                else
                    t.text = $"ROOM {depth}: {roomName}\n{monsterName} needs\n{requiredWeapon}";
            }
        }

        // Set depth badge color based on depth
        Transform badge = frame.transform.Find("Body/DepthBadge");
        if (badge != null)
        {
            Image badgeImg = badge.GetComponent<Image>();
            if (badgeImg != null)
            {
                Color[] depthColors = {
                    new Color(1f, 0.25f, 0.3f),
                    new Color(1f, 0.6f, 0.1f),
                    new Color(0.7f, 0.3f, 1f),
                    new Color(0.3f, 1f, 0.5f)
                };
                if (depth >= 0 && depth < depthColors.Length)
                    badgeImg.color = depthColors[depth];

                // Add depth number text to badge
                if (badge.Find("BadgeText") == null)
                {
                    GameObject bt = new GameObject("BadgeText");
                    bt.transform.SetParent(badge, false);
                    RectTransform btRT = bt.AddComponent<RectTransform>();
                    btRT.anchorMin = Vector2.zero; btRT.anchorMax = Vector2.one;
                    btRT.offsetMin = Vector2.zero; btRT.offsetMax = Vector2.zero;
                    Text btText = bt.AddComponent<Text>();
                    btText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    btText.fontSize = 22;
                    btText.fontStyle = FontStyle.Bold;
                    btText.color = Color.white;
                    btText.alignment = TextAnchor.MiddleCenter;
                    btText.text = depth.ToString();
                    btText.raycastTarget = false;
                }
            }
        }

        bool isBaseCase = string.IsNullOrEmpty(monsterName);
        Image bg = frame.GetComponent<Image>();
        if (bg != null)
            bg.color = isBaseCase ? baseCaseColor : normalFrameColor;

        frameObjects.Add(frame);
        AnimateFrameIn(frame);
    }

    public void PopFrame()
    {
        if (frameObjects.Count == 0) return;

        int lastIndex = frameObjects.Count - 1;
        GameObject frame = frameObjects[lastIndex];
        frameObjects.RemoveAt(lastIndex);

        AnimateFrameOut(frame);
    }

    public void HighlightFrame(int depth)
    {
        for (int i = 0; i < frameObjects.Count; i++)
        {
            Image bg = frameObjects[i].GetComponent<Image>();
            if (bg == null) continue;

            if (i == frameObjects.Count - 1)
                bg.color = highlightFrameColor;
            else
                bg.color = normalFrameColor;
        }
        highlightedIndex = depth;
    }

    void AnimateFrameIn(GameObject frame)
    {
        CanvasGroup cg = frame.GetComponent<CanvasGroup>();
        if (cg == null) cg = frame.AddComponent<CanvasGroup>();
        StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, 0.3f));
    }

    void AnimateFrameOut(GameObject frame)
    {
        CanvasGroup cg = frame.GetComponent<CanvasGroup>();
        if (cg == null) cg = frame.AddComponent<CanvasGroup>();
        StartCoroutine(FadeAndDestroy(cg, frame, 0.3f));
    }

    System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    System.Collections.IEnumerator FadeAndDestroy(CanvasGroup cg, GameObject obj, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        Destroy(obj);
    }

    public void ClearAllFrames()
    {
        foreach (var frame in frameObjects)
            if (frame != null) Destroy(frame);
        frameObjects.Clear();
        highlightedIndex = -1;
    }
}
