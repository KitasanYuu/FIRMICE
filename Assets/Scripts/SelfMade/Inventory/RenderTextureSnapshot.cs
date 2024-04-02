using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureSnapshot : MonoBehaviour
{
    public bool TestButton;
    public RenderTexture renderTexture;
    public Image targetImage;

    private void Update()
    {
        if (TestButton)
        {
            CaptureAndApply();
            TestButton = false;
        }
    }

    void CaptureAndApply()
    {
        // 使用支持透明度的TextureFormat创建Texture2D对象
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

        // 将当前激活的RenderTexture临时保存起来，以便之后恢复
        RenderTexture currentRT = RenderTexture.active;

        // 设置我们的RenderTexture为当前激活的RenderTexture
        RenderTexture.active = renderTexture;

        // 从RenderTexture复制图像到Texture2D
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        texture.Apply();

        texture = ApplyGammaCorrection(texture);

        // 恢复之前激活的RenderTexture
        RenderTexture.active = currentRT;

        // 保存Texture2D到文件
        byte[] bytes = texture.EncodeToPNG(); // 使用PNG格式以保持透明度
        string path = Path.Combine(Application.persistentDataPath, "snapshotWithTransparency.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log("Snapshot with transparency saved to: " + path);

        // 将Texture2D应用到UI的Image组件上
        // 创建一个支持透明度的Sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.Tight, new Vector4(0, 0, 0, 0), false);
        targetImage.sprite = sprite;
    }

    Texture2D ApplyGammaCorrection(Texture2D texture, float gamma = 0.2f)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r = Mathf.Pow(pixels[i].r, 1.0f / gamma);
            pixels[i].g = Mathf.Pow(pixels[i].g, 1.0f / gamma);
            pixels[i].b = Mathf.Pow(pixels[i].b, 1.0f / gamma);
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }


    // 示例：你可以在适当的时候调用这个方法，例如在一个按钮的点击事件中
    public void OnCaptureButtonClick()
    {
        CaptureAndApply();
    }
}
