using CustomInspector;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoader
{
    public static Sprite LoadKachaImageAsSprite(string filename)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "KaChaImage");
        string path = Path.Combine(folderPath, filename);
        Debug.Log(path);
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(bytes))
            {
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                return sprite;
            }
            else
            {
                Debug.LogError("Failed to load image: " + filename);
            }
        }
        else
        {
            Debug.LogError("File not found: " + filename);
        }

        // 如果加载失败或文件不存在，返回null
        return null;
    }

    public static void LoadKachaImageAsSpriteAction(string filename, Action<Sprite> onLoaded)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "KaChaImage");
        string path = Path.Combine(folderPath, filename);
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(bytes))
            {
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                onLoaded?.Invoke(sprite);
            }
        }
        else
        {
            Debug.LogError("File not found: " + path);
        }
    }

    //Usage
    //void Start()
    //{
    //    // 尝试加载图片
    //    string filename = "userProfile.png"; // 确保这个文件存在于Application.persistentDataPath目录下
    //    ImageLoader.LoadImageAsSprite(filename, OnImageLoaded);
    //}

    //void OnImageLoaded(Sprite loadedSprite)
    //{
    //    // 这里可以处理加载完成后的操作，例如将图片设置为UI组件的Sprite
    //    Image myUIImage = GetComponent<Image>(); // 确保这个GameObject有一个Image组件
    //    myUIImage.sprite = loadedSprite;
    //}

}
