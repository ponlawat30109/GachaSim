using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExporter
{
    [MenuItem("Tools/Export Sprite to PNG")]
    public static void ExportSprite()
    {
        // Select the GameObject in the scene
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        SpriteRenderer sr = selected.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("Selected GameObject does not have a SpriteRenderer with a sprite.");
            return;
        }

        Sprite sprite = sr.sprite;
        Texture2D texture = sprite.texture;

        // Create a readable copy of the texture
        Texture2D readableTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        Color[] pixels = texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height
        );
        readableTex.SetPixels(pixels);
        readableTex.Apply();

        // Encode to PNG
        byte[] pngData = readableTex.EncodeToPNG();
        if (pngData != null)
        {
            string path = Path.Combine(Application.dataPath, sprite.name + ".png");
            File.WriteAllBytes(path, pngData);
            Debug.Log("Sprite exported to: " + path);
        }
    }
}
