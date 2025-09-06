using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class InventoryUIController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement inventoryPanel;
    [field: SerializeField] public VisualTreeAsset ResultItemTemplate { get; set; }
    private ScrollView inventoryScrollView;

    private void Awake()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        inventoryPanel = root.Q<VisualElement>("inventoryPanel");
        inventoryScrollView = inventoryPanel.Q<ScrollView>();
    }

    public void DisplayInventory()
    {
        if (inventoryScrollView == null || ResultItemTemplate == null) return;
        inventoryScrollView.Clear();
        var inventory = InventoryManager.Instance.Inventory;
        if (inventory == null || inventory.Length == 0) return;
        foreach (var item in inventory)
        {
            var element = ResultItemTemplate.CloneTree();
            var nameLabel = element.Q<Label>("itemNameLabel");
            var quantityLabel = element.Q<Label>("itemRarityLabel");
            var iconImage = element.Q<Image>("itemIconImage");

            nameLabel.text = item.itemSO != null ? item.itemSO.itemName : $"ID:{item.item_id}";
            quantityLabel.text = item.quantity.ToString();
            iconImage.sprite = item.itemSO != null ? item.itemSO.icon : null;

            quantityLabel.style.color = Color.black;
            quantityLabel.style.display = DisplayStyle.Flex;
            element.AddToClassList("inventory-item");
            inventoryScrollView.Add(element);
        }
    }
}
