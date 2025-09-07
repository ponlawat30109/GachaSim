using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class GachaUIController : MonoBehaviour
{
    private VisualElement root;
    private Label announcementText;
    private Label remainingCount;
    private Button gachaButton;
    private Button multiGachaButton;
    private Button closeResultButton;
    private VisualElement resultPanel;
    [field: SerializeField] public VisualTreeAsset ResultItemTemplate { get; set; }

    [SerializeField, ReadOnlyInspector] private string notify;

    private bool isResultPanelVisible = false;

    public Label AnnouncementText { get => announcementText; set => announcementText = value; }
    public bool IsScrolling { get; private set; }

    public void Initialize()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        AnnouncementText = root.Q<Label>("announcementText");
        remainingCount = root.Q<Label>("remainingCount");
        gachaButton = root.Q<Button>("gachaButton");
        multiGachaButton = root.Q<Button>("multiGachaButton");
        resultPanel = root.Q<VisualElement>("resultPanel");
        closeResultButton = root.Q<Button>("closeResultButton");

        UpdateRemainingCount();

        gachaButton.clicked += OnGachaButtonClicked;
        multiGachaButton.clicked += OnMultiGachaButtonClicked;
        closeResultButton.clicked += HideResultPanel;

        AnnouncementText.style.display = DisplayStyle.None;
        resultPanel.style.display = DisplayStyle.None;
        closeResultButton.style.display = DisplayStyle.None;

        HideResultPanel();
    }

    void Start()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.Log("UnityMainThreadDispatcher Enqueue called in GachaUIController"));
    }

    private void OnGachaButtonClicked()
    {
        GachaManager.Instance.PullGacha(1, result =>
        {
            DisplayGachaResult(result);
            UpdateRemainingCount();
        });
    }

    private void OnMultiGachaButtonClicked()
    {
        GachaManager.Instance.PullGacha(10, result =>
        {
            DisplayGachaResult(result);
            UpdateRemainingCount();
        });
    }

    private void UpdateRemainingCount()
    {
        var remain = GachaManager.Instance.GachaRemaining;
        remainingCount.text = remain.ToString();
        remainingCount.style.display = DisplayStyle.Flex;

        bool canGacha = !isResultPanelVisible && remain >= 1;
        bool canMultiGacha = !isResultPanelVisible && remain >= 10;

        gachaButton.SetEnabled(canGacha);
        multiGachaButton.SetEnabled(canMultiGacha);

        gachaButton.style.opacity = canGacha ? 1f : 0.75f;
        multiGachaButton.style.opacity = canMultiGacha ? 1f : 0.75f;
    }

    private void SetGachaButtonsEnabled(bool enabled)
    {
        gachaButton.SetEnabled(enabled);
        multiGachaButton.SetEnabled(enabled);
        gachaButton.style.opacity = enabled ? 1f : 0.5f;
        multiGachaButton.style.opacity = enabled ? 1f : 0.5f;
    }

    private void DisplayGachaResult(GachaResult result)
    {
        resultPanel.Clear();
        foreach (var item in result.results)
        {
            var element = ResultItemTemplate.CloneTree();
            var nameLabel = element.Q<Label>("itemNameLabel");
            var rarityLabel = element.Q<Label>("itemRarityLabel");
            var iconImage = element.Q<Image>("itemIconImage");

            nameLabel.text = item.itemSO != null ? item.itemSO.itemName : item.name;
            rarityLabel.text = item.itemSO != null ? item.itemSO.rarity : item.rarity;
            iconImage.sprite = item.itemSO != null ? item.itemSO.icon : null;

            Color rarityColor = rarityLabel.text.ToUpper() switch
            {
                "E" => Color.gray,
                "D" => Color.green,
                "C" => Color.blue,
                "B" => new Color(0.5f, 0f, 1f),
                "A" => Color.red,
                _ => Color.black
            };
            nameLabel.style.color = new StyleColor(rarityColor);
            rarityLabel.style.color = new StyleColor(rarityColor);

            resultPanel.Add(element);
        }
        resultPanel.style.display = DisplayStyle.Flex;
        closeResultButton.style.display = DisplayStyle.Flex;
        isResultPanelVisible = true;

        SetGachaButtonsEnabled(false);
        InventoryManager.Instance.InventoryUI.SetInventoryPanelVisible(false);
    }

    public void HideResultPanel()
    {
        resultPanel.style.display = DisplayStyle.None;
        closeResultButton.style.display = DisplayStyle.None;
        isResultPanelVisible = false;

        SetGachaButtonsEnabled(true);
        InventoryManager.Instance.InventoryUI.SetInventoryPanelVisible(true);
    }

    public void ShowAnnouncementFromApi(RareAnnounce announce)
    {
        var username = announce.username;
        var item = announce.item;
        var notifyText = $"Congratulations! {username} got a {item}";
        AnnouncementText.text = notifyText;
        AnnouncementText.style.display = DisplayStyle.Flex;

        StartCoroutine(ScrollMessage());
    }

    IEnumerator ScrollMessage()
    {
        IsScrolling = true;
        yield return null;

        float duration = 3f;
        float elapsed = 0f;

        float textWidth = announcementText.resolvedStyle.width;
        float screenWidth = announcementText.parent.resolvedStyle.width;

        float startX = screenWidth / 2f + textWidth / 2f;
        float endX = -screenWidth / 2f - textWidth / 2f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentX = Mathf.Lerp(startX, endX, t);
            announcementText.style.translate = new Translate(new Length(currentX, LengthUnit.Pixel), 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        announcementText.text = "";
        announcementText.style.translate = new Translate(new Length(startX, LengthUnit.Pixel), 0);
        IsScrolling = false;
    }


    public void SetAnnouncementText(string text)
    {
        if (announcementText != null)
            announcementText.text = text;
    }

    public void HideAnnouncement()
    {
        if (announcementText != null)
            announcementText.style.display = DisplayStyle.None;
    }
}
