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

        Text frameText = frame.GetComponentInChildren<Text>();
        if (frameText != null)
        {
            if (string.IsNullOrEmpty(monsterName))
                frameText.text = $"  Room {depth}: {roomName}\n  BASE CASE - Pick up item";
            else
                frameText.text = $"  Room {depth}: {roomName}\n  {monsterName} — needs {requiredWeapon}";
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
