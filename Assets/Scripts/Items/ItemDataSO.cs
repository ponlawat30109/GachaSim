using UnityEngine;

[CreateAssetMenu(menuName = "Gacha/ItemData")]
public class ItemDataSO : ScriptableObject
{
    public int itemId;
    public string itemName;
    public string rarity;
    public Sprite icon;
}