using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : Singleton<LoginManager>
{
    public string Username { get; set; }
    // public UserData CurrentUser { get; set; }
    // public InventoryData[] Inventory { get; set; }

    public IEnumerator Login(string username)
    {
        WWWForm form = new();
        form.AddField("username", username);

        // using UnityWebRequest www = UnityWebRequest.Post("{ServerConfig.LocalUrl}/login", form);
        using UnityWebRequest www = UnityWebRequest.Post($"{ServerConfig.RemoteUrl}/login", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log("Login successful: " + www.downloadHandler.text);
            var response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
            if (response != null)
            {
                if (response.user != null)
                {
                    NetworkManager.Instance.CurrentUser = response.user;
                    GachaManager.Instance.GachaRemaining = response.user.gacha_remaining;
                }
                if (response.inventory != null)
                {
                    InventoryManager.Instance.Inventory = response.inventory;
                }
                StartCoroutine(InventoryManager.Instance.RefreshInventory());
            }
        }
    }

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
    //             Inventory = response.inventory;
    //         }
    //     }
    // }
}

// [System.Serializable]
// public class UserData
// {
//     public string name;
//     public int gacha_remaining;
// }

// [System.Serializable]
// public class InventoryData
// {
//     public int item_id;
//     public int quantity;
// }

// [System.Serializable]
// public class LoginResponse
// {
//     public bool success;
//     public UserData user;
//     public InventoryData[] inventory;
// }