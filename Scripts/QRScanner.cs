using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.Networking;


public class QRScanner : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private WebCamTexture frontCam;
    private Texture defaultBackground;
    private Rect screenRect;

    private string type = "";
    private string level = "";
    private string colorrr = "";
    private string colorName = "";
    // private string qrcodeId = "";
    //private bool isColorIng = false; // 控制有沒有在變色

    public RawImage background;
    public AspectRatioFitter fit;

    private void Start()
    {

        defaultBackground = background.texture;
        backCam = new WebCamTexture();
        backCam.requestedHeight = Screen.height;
        backCam.requestedWidth = Screen.width;
        if (backCam != null)
        {
            backCam.Play();
            background.texture = backCam;
            camAvailable = true;

            StartCoroutine(colorEveryFiveSeconds());
        }

    }
    private IEnumerator colorEveryFiveSeconds()
    {
        while (true) // 無限循環
        {
            yield return new WaitForSeconds(5f); // 等待五秒

            if (camAvailable)
            {
                int centerX = backCam.width / 2;
                int centerY = backCam.height / 2;

                Color32 centerColor = backCam.GetPixel(centerX, centerY);
                colorrr = "中間顏色 R: " + centerColor.r + ", G: " + centerColor.g + ", B: " + centerColor.b;
                Debug.Log("中間顏色 R: " + centerColor.r + ", G: " + centerColor.g + ", B: " + centerColor.b);

                string rgbString = $"rgb({centerColor.r},{centerColor.g},{centerColor.b})";

                StartCoroutine(GetColorInfoFromAPI(rgbString));


            }
        }
    }

    private IEnumerator GetColorInfoFromAPI(string rgbString)
    {
        string apiUrl = $"https://www.thecolorapi.com/id?rgb={rgbString}";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        yield return request.SendWebRequest();

        if (!request.isNetworkError && !request.isHttpError)
        {
            string responseText = request.downloadHandler.text;

            // Parse JSON response using Unity's JSON utility
            ColorApiResponse colorInfo = JsonUtility.FromJson<ColorApiResponse>(responseText);
            colorName = colorInfo.name.value;
            Debug.Log($"Color Name: {colorName}");
        }
        else
        {
            Debug.LogWarning($"API request failed: {request.error}");
        }
    }

    // Update is called once per frame
    private void OnGUI()
    {
        if (!camAvailable)
            return;

        Color32[] pixelData = backCam.GetPixels32();

        int width = backCam.width;
        int height = backCam.height;

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels32(pixelData);

        //GUI.DrawTexture(screenRect, backCam, ScaleMode.ScaleToFit);
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            // decode the current frame
            var result = barcodeReader.Decode(backCam.GetPixels32(),
              backCam.width, backCam.height);
            if (result != null)
            {
                UnityEngine.Debug.Log(result);
                UnityEngine.Debug.Log("掃到哭阿扣啦");
                if (result.Text[0].ToString() == "B")
                {
                    type = "protanomalous";
                }
                else if (result.Text[0].ToString() == "C")
                {
                    type = "deuteranomalous";
                }
                else if (result.Text[0].ToString() == "D")
                {
                    type = "tritanomalous";
                }
                else if (result.Text[0].ToString() == "A")
                {
                    type = "normal";
                    level = "normal";
                }

                if (type != "normal")
                {
                    if (result.Text[1].ToString() == "1")
                    {
                        level = "severe";
                    }
                    else if (result.Text[1].ToString() == "2")
                    {
                        level = "moderate";
                    }
                    else if (result.Text[1].ToString() == "3")
                    {
                        level = "mild";
                    }
                }

                UnityEngine.Debug.Log("色盲型態 : " + type + " " + level);

            }

        }
        catch (Exception ex) { Debug.LogWarning(ex.Message); }

        double factor = 1;

        if (level == "severe")
        {
            // severe
            factor = 1.5;
        }
        else if (level == "moderate")
        {
            // moderate
            factor = 1.3;
        }
        else if (level == "mild")
        {
            // mild;
            factor = 1.15;
        }


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                Color32 pixelColor = pixelData[index];

                int r = pixelColor.r;
                int g = pixelColor.g;
                int b = pixelColor.b;

                if (type == "protanomalous")
                {
                    r = (int)Mathf.Min(255, pixelColor.r * (float)factor);
                    g = (int)Mathf.Min(255, pixelColor.g);
                    b = (int)Mathf.Min(255, pixelColor.b * (float)factor);

                }
                else if (type == "deuteranomalous")
                {
                    r = (int)Mathf.Min(255, pixelColor.r);
                    g = (int)Mathf.Min(255, pixelColor.g * (float)factor);
                    b = (int)Mathf.Min(255, pixelColor.b * (float)factor);
                }
                else if (type == "tritanomalous")
                {
                    r = (int)Mathf.Min(255, pixelColor.r * (float)factor);
                    g = (int)Mathf.Min(255, pixelColor.g * (float)factor);
                    b = (int)Mathf.Min(255, pixelColor.b);
                }

                // 創建新的像素顏色
                Color32 newColor = new Color32((byte)r, (byte)g, (byte)b, pixelColor.a);
                texture.SetPixel(x, y, newColor);

            }
        }


        // 更新Texture2D套到背景的texture
        texture.Apply();
        background.texture = texture;

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);    //非鏡像
        //background.rectTransform.localScale = new Vector3(-1f, scaleY, 1f);    //鏡像

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);


        //這邊開始是雙螢幕設定 感覺要拔掉變成全螢幕
        screenRect = new Rect(0, 0, Screen.width, Screen.height);

        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();

        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        string text = type + " " + level + "\n" + colorrr +"  "+ colorName;
        GUI.Label(screenRect, text, style);



    }
}

[Serializable]
public class ColorApiResponse
{
    public ColorName name;
}

[Serializable]
public class ColorName
{
    public string value;
}