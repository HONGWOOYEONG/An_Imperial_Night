using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GameFlameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text flameText;

    private void Awake()
    {
        Transform existingPanel = transform.Find("GameFlamePanel");
        if (existingPanel != null)
            flameText = existingPanel.GetComponentInChildren<TMP_Text>(true);

        if (flameText == null)
            CreateFlameUI();
    }

    private void Update()
    {
        if (flameText == null) return;

        int flameCount = GameSessionManager.Instance != null
            ? GameSessionManager.Instance.RemainingFlameCount
            : 0;

        flameText.text = $"불꽃  ×  {flameCount}";
    }

    private void CreateFlameUI()
    {
        TMP_FontAsset sharedFont = GetComponentInChildren<TMP_Text>(true)?.font;

        GameObject panelObject = new GameObject(
            "GameFlamePanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(24f, -24f);
        panel.sizeDelta = new Vector2(260f, 72f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.035f, 0.04f, 0.055f, 0.88f);
        background.raycastTarget = false;

        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.78f, 0.24f, 0.12f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject(
            "FlameText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 4f);
        textRect.offsetMax = new Vector2(-16f, -4f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (sharedFont != null)
            text.font = sharedFont;

        text.text = "불꽃  ×  0";
        text.fontSize = 36f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = new Color(1f, 0.42f, 0.18f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        flameText = text;
    }
}
