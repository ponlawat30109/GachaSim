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

    public void Initialize()
    {
        // var uri = new Uri($"{ServerConfig.LocalUrl}");
        var uri = new Uri($"{ServerConfig.RemoteUrl}");
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