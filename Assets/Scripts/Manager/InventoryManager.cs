using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class InventoryManager : Singleton<InventoryManager>
{
    [field: SerializeField] public InventoryUIController InventoryUI { get; set; }
    [field: SerializeField, ReadOnlyInspector, ExposedScriptableObject] public ItemDataSO[] AllItems { get; set; }
    [field: SerializeField, ReadOnlyInspector] public InventoryData[] Inventory { get; set; }
    public Dictionary<int, ItemDataSO> ItemDictionary { get; set; } = new();

    public IEnumerator RefreshInventory()
    {
        if (NetworkManager.Instance.CurrentUser == null) yield break;
        // string url = $"http://localhost:3000/login/refresh?username={NetworkManager.Instance.CurrentUser.name}";
        string url = $"http://172.236.153.97:3000/login/refresh?username={NetworkManager.Instance.CurrentUser.name}";
        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
            if (response != null && response.inventory != null)
            {
                Inventory = response.inventory;
                BuildItemDictionary();
                MapInventorySO();

                InventoryUI.DisplayInventory();
            }
        }
    }

    public void BuildItemDictionary()
    {
        ItemDictionary.Clear();
        if (AllItems != null)
            foreach (var item in AllItems)
                if (item != null)
                    ItemDictionary[item.itemId] = item;
    }

    public void MapInventorySO()
    {
        foreach (var inv in Inventory)
        {
            inv.itemSO = ItemDictionary.TryGetValue(inv.item_id, out var so) ? so : null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Fill AllItems from Path")]
    public void AutoFillAllItemsFromPath()
    {
        string path = "Assets/Scripts/Items/ScriptableObject";
        var guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { path });
        var items = new List<ItemDataSO>();
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);
            if (so != null)
                items.Add(so);
        }
        AllItems = items.ToArray();
        Debug.Log($"Auto-filled {AllItems.Length} ItemDataSO from {path}");
    }

    [ContextMenu("Debug Inventory")]
    public void DebugInventorySO()
    {
        if (Inventory == null) return;
        foreach (var inv in Inventory)
        {
            Debug.Log($"item_id: {inv.item_id}, itemSO: {(inv.itemSO != null ? inv.itemSO.name : "null")}, quantity: {inv.quantity}");
        }
    }
#endif
}
