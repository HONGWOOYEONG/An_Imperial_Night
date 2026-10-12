using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void Awake()
    {
        Transform existingPanel = transform.Find("GameTimerPanel");
        if (existingPanel != null)
            timerText = existingPanel.GetComponentInChildren<TMP_Text>(true);

        if (timerText == null)
            CreateTimerUI();
    }

    private void Update()
    {
        if (timerText == null)
            return;

        if (GameSessionManager.Instance == null)
            return;

        float remainingTime =
            GameSessionManager.Instance.CurrentDisplayTime;

        int totalSeconds =
            Mathf.CeilToInt(Mathf.Max(0f, remainingTime));

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void CreateTimerUI()
    {
        TMP_FontAsset sharedFont = GetComponentInChildren<TMP_Text>(true)?.font;

        GameObject panelObject = new GameObject(
            "GameTimerPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0f, -24f);
        panel.sizeDelta = new Vector2(220f, 72f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.035f, 0.04f, 0.055f, 0.88f);
        background.raycastTarget = false;

        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.78f, 0.64f, 0.28f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject(
            "TimerText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (sharedFont != null)
            text.font = sharedFont;

        text.text = "00:00";
        text.fontSize = 42f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.92f, 0.68f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        timerText = text;
    }
}
