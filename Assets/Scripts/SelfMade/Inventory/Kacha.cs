using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Kacha : MonoBehaviour
{
    public bool TestButton;
    public RenderTexture renderTexture;
    public Image targetImage;
    private float RetryDelay;

    private string folderName;
    private string folderPath;

    private Queue<CaptureRequest> captureRequests = new Queue<CaptureRequest>(); // 存储待执行的调用请求

    void Start()
    {
        folderName = "KaChaImage"; // 新文件夹的名称
        folderPath = Path.Combine(Application.persistentDataPath, folderName); // 新文件夹的完整路径

        // 检查新文件夹是否已存在
        if (!Directory.Exists(folderPath))
        {
            // 创建新文件夹
            Directory.CreateDirectory(folderPath);
        }
        else
        {
            // 获取持久化数据路径中的所有文件
            string[] files = Directory.GetFiles(folderPath);

            // 删除所有文件
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }

    private void Update()
    {

    }

    IEnumerator ProcessCaptureRequests()
    {
        while (captureRequests.Count > 0)
        {
            CaptureRequest request = captureRequests.Dequeue();
            yield return StartCoroutine(CaptureAndApplyWithDelay(request.WS,request.SaveID, request.ImageThresholdKB, request.Delay));
        }
    }

    IEnumerator CaptureAndApplyWithDelay(WeaponCell ws, string saveID, float ImageThresholdKB, float delay)
    {
        bool TextureLeagal = false;
        Texture2D texture = null;
        byte[] bytes = null;

        while (!TextureLeagal)
        {
            // 使用支持透明度的TextureFormat创建Texture2D对象
            texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

            // 将当前激活的RenderTexture临时保存起来，以便之后恢复
            RenderTexture currentRT = RenderTexture.active;

            // 设置我们的RenderTexture为当前激活的RenderTexture
            RenderTexture.active = renderTexture;

            // 从RenderTexture复制图像到Texture2D
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            // 恢复之前激活的RenderTexture
            RenderTexture.active = currentRT;

            // 保存Texture2D到文件
            bytes = texture.EncodeToPNG(); // 使用PNG格式以保持透明度

            TextureLeagal = PNGLeagalCheck(bytes, ImageThresholdKB);

            if (!TextureLeagal)
            {
                yield return new WaitForSeconds(delay); // 等待一定时间后再继续下一次循环
            }
        }

        string path = Path.Combine(folderPath, saveID + ".YPic");
        File.WriteAllBytes(path, bytes);
        Debug.Log("Snapshot with transparency saved to: " + path);

        // 将Texture2D应用到UI的Image组件上
        // 创建一个支持透明度的Sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.Tight, new Vector4(0, 0, 0, 0), false);
        sprite.name = ws.name;
        //targetImage.color = Color.white;
        //targetImage.sprite = sprite;
        ws._weaponImage.color = Color.white;
        ws._weaponImage.sprite = sprite;
    }

    Texture2D ApplyGammaCorrection(Texture2D texture, float gamma = 0.1f)
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

    public bool PNGLeagalCheck(byte[] bytes, float thresholdKB)
    {
        int imageSizeInBytes = bytes.Length;
        float imageSizeInKB = imageSizeInBytes / 1024f; // 使用浮点数除法

        Debug.Log(imageSizeInKB);

        if (imageSizeInKB > thresholdKB && imageSizeInKB > 0)
            return true;
        else
            return false;
    }

    // 示例：你可以在适当的时候调用这个方法，例如在一个按钮的点击事件中
    public void CaptureSnapShot(WeaponCell ws,string saveID, float ImageThresholdKB,float retryDelay = -1)
    {
        CaptureRequest newRequest = new CaptureRequest();

        if (retryDelay >0)
            newRequest = new CaptureRequest(ws,saveID, ImageThresholdKB, retryDelay);
        else
            newRequest = new CaptureRequest(ws, saveID, ImageThresholdKB, RetryDelay);

        captureRequests.Enqueue(newRequest);
        StartCoroutine(ProcessCaptureRequests());
    }

    // 定义一个调用请求的结构体，用于存储调用所需的参数
    struct CaptureRequest
    {
        public WeaponCell WS;
        public string SaveID;
        public float ImageThresholdKB;
        public float Delay;

        public CaptureRequest(WeaponCell ws,string saveID, float imageThresholdKB, float delay)
        {
            WS = ws;
            SaveID = saveID;
            ImageThresholdKB = imageThresholdKB;
            Delay = delay;
        }
    }
}
