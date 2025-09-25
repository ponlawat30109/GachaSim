using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : Singleton<LoginManager>
{
    public string Username { get; set; }

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
}
