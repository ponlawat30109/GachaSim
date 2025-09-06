using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using SocketIOClient;
using System;
using SocketIOClient.Newtonsoft.Json;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkManager : Singleton<NetworkManager>
{
    public SocketIOUnity Socket { get; set; }
    public UserData CurrentUser { get; set; }

    // [field: SerializeField, ReadOnlyInspector, ExposedScriptableObject] public ItemDataSO[] AllItems { get; set; }
    // [field: SerializeField, ReadOnlyInspector] public InventoryData[] Inventory { get; set; }

    // public Dictionary<int, ItemDataSO> ItemDictionary { get; set; } = new();

    public void Initialize()
    {
        // var uri = new Uri("http://localhost:3000");
        var uri = new Uri("http://172.236.153.97:3000");
        Socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string> { { "token", "UNITY" } },
            EIO = EngineIO.V4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        })
        {
            JsonSerializer = new NewtonsoftJsonSerializer()
        };

        Socket.OnConnected += (sender, e) => { Debug.Log("Socket.IO Connected! Server is running"); };
        Socket.On("RareNotify", (response) =>
        {
            var json = response.GetValue().ToString();
            RareAnnounce announce = JsonConvert.DeserializeObject<RareAnnounce>(json);
            if (announce != null)
                GachaManager.Instance.AnnounceRareItem(announce);
        });
        Socket.Connect();
    }

    // public void BuildItemDictionary()
    // {
    //     ItemDictionary.Clear();
    //     if (AllItems != null)
    //         foreach (var item in AllItems)
    //             if (item != null)
    //                 ItemDictionary[item.itemId] = item;
    // }

    // public void MapInventorySO()
    // {
    //     foreach (var inv in Inventory)
    //     {
    //         inv.itemSO = ItemDictionary.TryGetValue(inv.item_id, out var so) ? so : null;
    //     }
    // }

    // public IEnumerator RefreshInventory()
    // {
    //     if (CurrentUser == null) yield break;
    //     string url = $"http://localhost:3000/login/refresh?username={CurrentUser.name}";
    //     string url = $"http://172.236.153.97:3000/login/refresh?username={CurrentUser.name}";
    //     using UnityWebRequest www = UnityWebRequest.Get(url);
    //     yield return www.SendWebRequest();
    //     if (www.result == UnityWebRequest.Result.Success)
    //     {
    //         var response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
    //         if (response != null && response.inventory != null)
    //         {
    //             // Debug.Log("Inventory refreshed: " + www.downloadHandler.text);
    //             Inventory = response.inventory;
    //             BuildItemDictionary();
    //             MapInventorySO();
    //         }
    //     }
    // }

    // #if UNITY_EDITOR
    //     [ContextMenu("Auto Fill AllItems from Path")]
    //     public void AutoFillAllItemsFromPath()
    //     {
    //         string path = "Assets/Scripts/Items/ScriptableObject";
    //         var guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { path });
    //         var items = new List<ItemDataSO>();
    //         foreach (var guid in guids)
    //         {
    //             var assetPath = AssetDatabase.GUIDToAssetPath(guid);
    //             var so = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);
    //             if (so != null)
    //                 items.Add(so);
    //         }
    //         AllItems = items.ToArray();
    //         Debug.Log($"Auto-filled {AllItems.Length} ItemDataSO from {path}");
    //     }

    //     [ContextMenu("Debug Inventory")]
    //     public void DebugInventorySO()
    //     {
    //         if (Inventory == null) return;
    //         foreach (var inv in Inventory)
    //         {
    //             Debug.Log($"item_id: {inv.item_id}, itemSO: {(inv.itemSO != null ? inv.itemSO.name : "null")}, quantity: {inv.quantity}");
    //         }
    //     }
    // #endif
}

[Serializable]
public class RareAnnounce
{
    public string username;
    public string item;
    public string rarity;
}

[Serializable]
public class UserData
{
    public string name;
    public int gacha_remaining;
}

[Serializable]
public class InventoryData
{
    public int id;
    public int item_id;
    public int quantity;
    [SerializeField, ExposedScriptableObject] public ItemDataSO itemSO;
}

[Serializable]
public class LoginResponse
{
    public bool success;
    public UserData user;
    public InventoryData[] inventory;
}