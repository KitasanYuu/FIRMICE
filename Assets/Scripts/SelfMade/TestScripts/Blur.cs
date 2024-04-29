using CustomInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Blur : MonoBehaviour
{
    public Shader blurShader;
    public Image screenshotImage; // 引用模糊的材质
    [SerializeField, FixedValues("ScreenSpace", "SpecificCamera")]
    private string RenderMode;

    [ShowIf(nameof(CameraRender))]
    public Camera specificCamera; // 指定的摄像机

    [Space2(10)]
    [Range(0, 1)] public float transparency = 0;
    [Range(0, 10)] public float brightness = 1;

    public bool CameraRender()
    => RenderMode == "SpecificCamera";

    public bool CaptureBlur;

    private Texture2D capturedTexture;
    private RenderTexture renderTexture;
    private Material _blurMat;

    private void Start()
    {
        screenshotImage = GetComponent<Image>();
        _blurMat = new Material(blurShader);
        screenshotImage.material = _blurMat;
        screenshotImage.color = Color.white;
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RGB111110Float);
    }

    private void Update()
    {
        _blurMat.SetFloat("_Transparency", transparency);
        _blurMat.SetFloat("_Brightness", brightness);
        // 按下特定键（例如空格键）时进行屏幕截图并应用模糊效果
        if (UnityEngine.Input.GetKeyDown(KeyCode.O)||CaptureBlur)
        {
            if(RenderMode == "ScreenSpace")
                CaptureScreenshotAndSetSprite();
            else
                CaptureCameraShotAndSetSprite();
            CaptureBlur = false;
        }
    }

    private IEnumerator CaptureScreenshotAndSetSpriteCoroutine()
    {
        // Capture screenshot and save as PNG file
        string screenshotPath = "screenshot.png";
        ScreenCapture.CaptureScreenshot(screenshotPath);

        // Wait for the end of the frame to ensure the screenshot is captured
        yield return new WaitForSeconds(0.5f);

        // Destroy previous captured texture if it exists
        if (capturedTexture != null)
        {
            Destroy(capturedTexture);
        }

        // Load PNG file and convert to Texture2D
        byte[] fileData = System.IO.File.ReadAllBytes(screenshotPath);
        capturedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, true); // Set linear color space to false
        capturedTexture.LoadImage(fileData);

        // Create Sprite from Texture2D
        Sprite screenshotSprite = Sprite.Create(capturedTexture, new Rect(0, 0, capturedTexture.width, capturedTexture.height), Vector2.zero);

        File.Delete(screenshotPath);

        // Set Sprite to the Image component
        screenshotImage.sprite = screenshotSprite;
    }

    // Convert RenderTexture to Texture2D
    private Texture2D TextureFromRenderTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height,TextureFormat.RGB24,true,true);
        //texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

        texture.Apply();

        RenderTexture.active = null;

        return texture;
    }


    private IEnumerator CaptureCameraShotAndSetSpriteCoroutine()
    {
        // Set target texture for specific camera
        specificCamera.targetTexture = renderTexture;

        // 设置清除标志为 Color，并设置背景颜色为不透明的黑色
        specificCamera.clearFlags = CameraClearFlags.Color;
        specificCamera.backgroundColor = new Color(0, 0, 0, 1);

        // Render camera
        specificCamera.Render();

        // Wait for end of frame to ensure rendering is complete
        yield return new WaitForEndOfFrame();

        // Reset target texture
        specificCamera.targetTexture = null;

        capturedTexture = TextureFromRenderTexture(renderTexture);

        // Create Sprite from RenderTexture
        Sprite screenshotSprite = Sprite.Create(capturedTexture, new Rect(0, 0, renderTexture.width, renderTexture.height), Vector2.zero);

        // Set Sprite to the Image component
        screenshotImage.sprite = screenshotSprite;
    }

    private void CaptureCameraShotAndSetSprite()
    {
        StartCoroutine(CaptureCameraShotAndSetSpriteCoroutine());
    }

    private void CaptureScreenshotAndSetSprite()
    {
        StartCoroutine(CaptureScreenshotAndSetSpriteCoroutine());
    }

}
