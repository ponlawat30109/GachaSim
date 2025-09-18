using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GachaManager : Singleton<GachaManager>
{
    public int GachaRemaining { get; set; }
    [field: SerializeField, ReadOnlyInspector] public GachaResult CurrentGachaResult { get; set; }
    [field: SerializeField] public GachaUIController GachaUI { get; set; }

    private bool isAnnouncing = false;
    private Queue<RareAnnounce> announceQueue = new();

    private Dictionary<int, ItemDataSO> gachaItemDictionary = new();

    #region RefreshGachaRemain
    public void RefreshGachaData(Action onComplete = null)
    {
        string username = NetworkManager.Instance.CurrentUser?.name;
        if (string.IsNullOrEmpty(username))
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(RefreshGachaDataCoroutine(username, onComplete));
    }

    private IEnumerator RefreshGachaDataCoroutine(string username, Action onComplete)
    {
        // string url = $"{ServerConfig.LocalUrl}/login/refresh?username={username}";
        string url = $"{ServerConfig.RemoteUrl}/login/refresh?username={username}";
        using var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<GachaRefreshResponse>(www.downloadHandler.text);
            if (response != null && response.user != null)
            {
                GachaRemaining = response.user.gacha_remaining;
            }
        }
        onComplete?.Invoke();
    }
    #endregion

    #region PullGacha
    public void PullGacha(int count, Action<GachaResult> onResult)
    {
        if (GachaRemaining < count)
        {
            onResult?.Invoke(null);
            return;
        }
        string username = NetworkManager.Instance.CurrentUser?.name;
        if (string.IsNullOrEmpty(username))
        {
            onResult?.Invoke(null);
            return;
        }
        StartCoroutine(PullGachaCoroutine(username, count, onResult));
    }

    private IEnumerator PullGachaCoroutine(string username, int count, Action<GachaResult> onResult)
    {
        WWWForm form = new();
        form.AddField("username", username);
        form.AddField("count", count);
        // using var www = UnityWebRequest.Post("http://localhost:3000/gacha/pull", form);
        using var www = UnityWebRequest.Post("http://172.236.153.97:3000/gacha/pull", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            GachaRemaining -= count;
            CurrentGachaResult = JsonUtility.FromJson<GachaResult>(www.downloadHandler.text);
            BuildGachaItemDictionary();
            MapGachaResultItemSO(CurrentGachaResult);
            onResult?.Invoke(CurrentGachaResult);
            StartCoroutine(InventoryManager.Instance.RefreshInventory());
        }
        else
        {
            // Debug.LogError(www.error);
            CurrentGachaResult = null;
            onResult?.Invoke(null);
        }
    }

    private void BuildGachaItemDictionary()
    {
        gachaItemDictionary.Clear();
        var allItems = InventoryManager.Instance.AllItems;
        if (allItems != null)
        {
            foreach (var item in allItems)
            {
                if (item != null)
                    gachaItemDictionary[item.itemId] = item;
            }
        }
    }

    private void MapGachaResultItemSO(GachaResult result)
    {
        if (result == null || result.results == null) return;
        foreach (var item in result.results)
        {
            item.itemSO = gachaItemDictionary.TryGetValue(item.id, out var so) ? so : null;
        }
    }
    #endregion

    #region Announcement
    public void AnnounceRareItem(RareAnnounce announce)
    {
        announceQueue.Enqueue(announce);
        TryDisplayNextAnnounce();
    }

    private void TryDisplayNextAnnounce()
    {
        if (isAnnouncing || announceQueue.Count == 0 || GachaUI == null) return;
        isAnnouncing = true;
        var announce = announceQueue.Dequeue();
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GachaUI.ShowAnnouncementFromApi(announce);
            GachaUI.StartCoroutine(AnnounceDelayCoroutine());
        });
    }

    private IEnumerator AnnounceDelayCoroutine()
    {
        while (GachaUI != null && GachaUI.IsScrolling)
            yield return null;
        GachaUI.SetAnnouncementText("");
        yield return new WaitForSeconds(1f);
        GachaUI.HideAnnouncement();
        isAnnouncing = false;
        TryDisplayNextAnnounce();
    }
    #endregion
}


[Serializable]
public class GachaResult
{
    public bool success;
    public GachaItem[] results;
}

[Serializable]
public class GachaItem
{
    public int id;
    public string name;
    public string rarity;
    [SerializeField, ExposedScriptableObject] public ItemDataSO itemSO;
}

[Serializable]
public class GachaRefreshResponse
{
    public bool success;
    public UserData user;
}