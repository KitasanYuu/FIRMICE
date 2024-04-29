using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Blur : MonoBehaviour
{
    public Image screenshotImage; // 引用模糊的材质
    private Texture2D capturedTexture;
    public bool CaptureBlur;

    private void Start()
    {
        screenshotImage = GetComponent<Image>();
    }

    private void Update()
    {
        // 按下特定键（例如空格键）时进行屏幕截图并应用模糊效果
        if (UnityEngine.Input.GetKeyDown(KeyCode.O)||CaptureBlur)
        {
            CaptureScreenshotAndSetSprite();
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

    private void CaptureScreenshotAndSetSprite()
    {
        StartCoroutine(CaptureScreenshotAndSetSpriteCoroutine());
    }

}
