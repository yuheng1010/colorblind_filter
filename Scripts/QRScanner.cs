//這是新的
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;


public class QRScanner : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private WebCamTexture frontCam;
    private Texture defaultBackground;
    //private Rect screenRect;

    private string type = "";
    private string level = "";
    // private string qrcodeId = "";


    public RawImage background;
    public AspectRatioFitter fit;

    // Use this for initialization
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
        }
        //WebCamDevice[] devices = WebCamTexture.devices;
        //if (devices.Length == 0)
        //{
        //    Debug.Log("No camera detected");
        //    camAvailable = false;
        //    return;
        //}
        //for (int i = 0; i < devices.Length; i++)
        //{

        //    // if (!devices [i].isFrontFacing) {    //開啟後鏡頭
        //    if (devices[i].isFrontFacing)
        //    {    //開啟前鏡頭
        //        backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
        //    }
        //}
        //if (backCam == null)
        //{
        //    Debug.Log("Unable to find back camera");
        //    return;
        //}
        //backCam.Play();
        //background.texture = backCam;
        //camAvailable = true;
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

        // 取每個pixel
        UnityEngine.Debug.Log("factor" + factor);
        // EditorUtility.DisplayDialog ("Title here", "Your text", "Ok");

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

                    // 創建新的像素顏色
                    Color32 newColor = new Color32((byte)r, (byte)g, (byte)b, pixelColor.a);
                    texture.SetPixel(x, y, newColor);

                    // 輸出 RGB 值
                    // if (x < 5 && y < 5)
                    // {
                    //Debug.Log("old Pixel (" + x + ", " + y + ") - R: " + pixelColor.r + ", G: " + pixelColor.g + ", B: " + pixelColor.b);
                    //Debug.Log("new Pixel (" + x + ", " + y + ") - R: " + r + ", G: " + g + ", B: " + b);
                    // }

                }
            }


        // 更新Texture2D套到背景的texture
        texture.Apply();
        background.texture = texture;

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        // background.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);    //非鏡像
        background.rectTransform.localScale = new Vector3(-1f, scaleY, 1f);    //鏡像

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        //screenRect = new Rect(0, 0, Screen.width, Screen.height);
        
        //int w = Screen.width, h = Screen.height;
        //GUIStyle style = new GUIStyle();
 
        //style.alignment = TextAnchor.UpperLeft;
        //style.fontSize = h * 2 / 50;
        //style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        //string text = type + " " + level;
        //GUI.Label(screenRect, text, style);



    }
}

